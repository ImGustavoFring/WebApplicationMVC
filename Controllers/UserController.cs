using Microsoft.AspNetCore.Authentication;
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
            .Include(u => u.SubscriptionAuthors)
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

    [HttpGet("Articles/{id?}")]
    public async Task<IActionResult> Articles(int? id)
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

        var articles = await _context.Articles
            .Where(a => a.Userid == id)
            .ToListAsync();

        return View(articles);
    }

    [HttpGet("Subscriptions/{id?}")]
    public async Task<IActionResult> Subscriptions(int? id)
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

        var subscriptions = await _context.Subscriptions
            .Where(s => s.Subscriberid == id)
            .Include(s => s.Author)
            .ToListAsync();

        return View(subscriptions);
    }

    [HttpGet("Subscribers/{id?}")]
    public async Task<IActionResult> Subscribers(int? id)
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

        var subscribers = await _context.Subscriptions
            .Where(s => s.Authorid == id)
            .Include(s => s.Subscriber)
            .ToListAsync();

        return View(subscribers);
    }

    [HttpPost("Delete/{id?}")]
    public async Task<IActionResult> Delete(int? id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Если id не указан или совпадает с текущим пользователем
        if (!id.HasValue || id == currentUserId)
        {
            id = currentUserId;
        }

        // Если пользователь пытается удалить не себя и не является администратором
        if (id != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Если удаляется текущий пользователь, разлогиниваем его
        if (id == currentUserId)
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Register", "Auth");
        }

        return RedirectToAction("Index", "Home");
    }

    // Подписка на пользователя
    [HttpPost("Subscribe/{id}")]
    public async Task<IActionResult> Subscribe(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (currentUserId == id)
        {
            return BadRequest("Нельзя подписаться на себя.");
        }

        var existingSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Subscriberid == currentUserId && s.Authorid == id);

        if (existingSubscription != null)
        {
            return BadRequest("Вы уже подписаны на этого пользователя.");
        }

        var subscription = new Subscription
        {
            Subscriberid = currentUserId,
            Authorid = id
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Articles), new { id });
    }

    // Отписка от пользователя
    [HttpPost("Unsubscribe/{id}")]
    public async Task<IActionResult> Unsubscribe(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Subscriberid == currentUserId && s.Authorid == id);

        if (subscription == null)
        {
            return BadRequest("Вы не подписаны на этого пользователя.");
        }

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Articles), new { id });
    }
}
