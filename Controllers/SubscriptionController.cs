using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    [Route("Subscription")]
    public class SubscriptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Subscriptions")]
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

        [HttpGet("Subscribers")]
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

        [HttpPost("Subscribe")]
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

            return RedirectToAction("Subscriptions", "Subscription");
        }

        [HttpPost("Unsubscribe")]
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

            return RedirectToAction("Subscriptions", "Subscription");  
        }
    }
}
