using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    [Route("User")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Details")]
        public async Task<IActionResult> Details(int? id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            var targetUserId = id ?? currentUserId;

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.SubscriptionAuthors)
                .FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View("SearchResults", new List<User>());
            }

            var users = await _context.Users
                .Where(u => EF.Functions.Like(u.Username, $"%{query}%") ||
                            EF.Functions.Like(u.Email, $"%{query}%") ||
                            EF.Functions.Like(u.Fullname, $"%{query}%"))
                .ToListAsync();

            return View("Search", users);
        }

        [HttpGet("Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            var targetUserId = id ?? currentUserId;

            if (targetUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _context.Roles.ToListAsync();

            ViewBag.Roles = roles;

            return View(user);
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit(int? id, int RoleId,
            string Username, string Email,
            string Fullname, string Bio,
            string Contactinfo, string Avatarurl,
            string newPassword, string newConfirmedPassword)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            var targetUserId = id ?? currentUserId;

            if (targetUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var existingUser = await _context.Users.FindAsync(targetUserId);

            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.Username = Username;
            existingUser.Email = Email;
            existingUser.Fullname = Fullname;
            existingUser.Bio = Bio;
            existingUser.Contactinfo = Contactinfo;
            existingUser.Avatarurl = Avatarurl;

            // Изменение пароля
            if (!string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(newConfirmedPassword))
            {
                if (newPassword != newConfirmedPassword)
                {
                    ModelState.AddModelError("newPassword", "Пароли не совпадают.");
                    var roles = await _context.Roles.ToListAsync();
                    ViewBag.Roles = roles;
                    return View(existingUser);
                }

                existingUser.Passwordhash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            // Изменение роли доступно только админу
            if (User.IsInRole("Admin"))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == RoleId);
                if (role == null)
                {
                    ModelState.AddModelError("RoleId", "Недопустимая роль.");
                    var roles = await _context.Roles.ToListAsync();
                    ViewBag.Roles = roles;
                    return View(existingUser);
                }

                existingUser.Roleid = role.Id;
            }

            await _context.SaveChangesAsync();

            if (currentUserId == targetUserId)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, existingUser.Username),
                    new Claim(ClaimTypes.Email, existingUser.Email),
                    new Claim(ClaimTypes.Role, existingUser.Role?.Name ?? "User"),
                    new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);
            }


            return RedirectToAction(nameof(Details), new { id = targetUserId });
        }


        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            var targetUserId = id ?? currentUserId;

            if (targetUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(targetUserId);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            if (currentUserId == targetUserId)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Logout", "Auth");
            }

            return RedirectToAction("Details", "User", new { id = currentUserId });
        }
    }
}
