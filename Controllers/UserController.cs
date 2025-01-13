using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;

[Authorize(Policy = "UserPolicy")]
[Route("User")]
public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("Details/{id?}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !int.TryParse(userId, out int parsedId))
            {
                return Unauthorized();
            }
            id = parsedId;
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpGet("Edit/{id?}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (!id.HasValue)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !int.TryParse(userId, out int parsedId))
            {
                return Unauthorized();
            }
            id = parsedId;
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (id != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _context.Roles.Select(r => r.Name).ToListAsync();
        ViewBag.Roles = roles;

        return View(user);
    }

    [HttpPost("Edit/{id?}")]
    public async Task<IActionResult> Edit(int? id, string RoleName, string Username, string Email, string Fullname, string Bio, string Contactinfo, string Avatarurl)
    {
        if (!id.HasValue)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !int.TryParse(userId, out int parsedId))
            {
                return Unauthorized();
            }
            id = parsedId;
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (id != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var existingUser = await _context.Users.FindAsync(id);
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

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleName);
        if (role == null)
        {
            ModelState.AddModelError("RoleName", "Невалидная роль.");
            var roles = await _context.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = roles;
            return View(existingUser);
        }

        existingUser.Roleid = role.Id;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = existingUser.Id });
    }
}
