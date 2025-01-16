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
        public async Task<IActionResult> Search(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return View(new List<User>());
            }

            var users = await _context.Users
                .Where(u => u.Fullname.Contains(searchString))
                .ToListAsync();

            return View(users);
        }


        [HttpGet("Edit")]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null || !int.TryParse(userId, out int parsedId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == parsedId);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _context.Roles.ToListAsync();

            ViewBag.Roles = roles;

            return View(user);
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit(int RoleId,
            string Username, string Email,
            string Fullname, string Bio,
            string Contactinfo, string Avatarurl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null || !int.TryParse(userId, out int parsedId))
            {
                return Unauthorized();
            }

            var existingUser = await _context.Users.FindAsync(parsedId);

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

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == RoleId);
            if (role == null)
            {
                ModelState.AddModelError("RoleId", "Invalid role.");
                var roles = await _context.Roles.ToListAsync();
                ViewBag.Roles = roles;
                return View(existingUser);
            }

            existingUser.Roleid = role.Id;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details));
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !int.TryParse(userId, out int parsedId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(parsedId);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync();
            return RedirectToAction("Register", "Auth");
        }
    }
}
