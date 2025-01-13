using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApplicationMVC.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Passwordhash))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.Name),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View();
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(string login, string email, string password, string confirmedPassword)
        {
            if (password != confirmedPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            if (_context.Users.Any(u => u.Username == login || u.Email == email))
            {
                ModelState.AddModelError("", "Username or email is already taken.");
                return View();
            }

            var role = _context.Roles.FirstOrDefault(r => r.Name == "User");

            if (role == null)
            {
                ModelState.AddModelError("", "Role 'User' does not exist.");
                return View();
            }

            var user = new User
            {
                Username = login,
                Email = email,
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(password),
                Roleid = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
    }
}
