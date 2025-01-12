using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create(string username, string email, string password, string roleName)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(roleName))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            if (_context.Users.Any(u => u.Username == username || u.Email == email))
            {
                ModelState.AddModelError("", "Username or email is already taken.");
                return View();
            }

            var role = _context.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                ModelState.AddModelError("", "Role not found.");
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(password),
                Roleid = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("Details/{userId}")]
        public async Task<IActionResult> Details(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet("Edit/{userId}")]
        public async Task<IActionResult> Edit(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit(int id, string username, string email, string bio, string contactInfo)
        {
            if (id <= 0 || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Invalid input.");
                return View();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(username) && user.Username != username)
            {
                if (_context.Users.Any(u => u.Username == username))
                {
                    ModelState.AddModelError("", "Username is already taken.");
                    return View();
                }

                user.Username = username;
            }

            if (!string.IsNullOrEmpty(email) && user.Email != email)
            {
                if (_context.Users.Any(u => u.Email == email))
                {
                    ModelState.AddModelError("", "Email is already taken.");
                    return View();
                }

                user.Email = email;
            }

            user.Bio = bio;
            user.Contactinfo = contactInfo;
            user.Updatedat = DateTime.Now;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { userId = user.Id });
        }

        [HttpPost("Delete/{userId}")]
        public async Task<IActionResult> Delete(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
