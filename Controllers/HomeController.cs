using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString = "", string criterion = "date", string order = "desc", int[] selectedTags = null, string filterBySubscription = "all")
        {
            var query = _context.Articles
                                .Include(a => a.Tags)
                                .Include(a => a.User)
                                .Include(a => a.Views)
                                .Include(a => a.Ratings)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.Title.Contains(searchString) ||
                                         (a.User != null && a.User.Username.Contains(searchString)));
            }

            query = criterion switch
            {
                "title" => order == "asc" ? query.OrderBy(a => a.Title) : query.OrderByDescending(a => a.Title),
                "views" => order == "asc" ? query.OrderBy(a => a.Views.Count) : query.OrderByDescending(a => a.Views.Count), 
                _ => order == "asc" ? query.OrderBy(a => a.Createdat) : query.OrderByDescending(a => a.Createdat),
            };

            query = query.Where(a => a.Visibility.Name != "Private");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null && filterBySubscription == "subscribed")
            {
                var subscribedAuthors = await _context.Subscriptions
                                                       .Where(s => s.Subscriberid.ToString() == userId)
                                                       .Select(s => s.Authorid)
                                                       .ToListAsync();
                query = query.Where(a => subscribedAuthors.Contains(a.Userid));
            }

            if (filterBySubscription == "own" && userId != null)
            {
                query = query.Where(a => a.Userid.ToString() == userId);
            }

            if (selectedTags != null && selectedTags.Length > 0)
            {
                query = query.Where(a => a.Tags.Any(t => selectedTags.Contains(t.Id)));
            }

            var articles = await query.ToListAsync();

            ViewData["Tags"] = await _context.Tags.ToListAsync();

            return View(articles);
        }


        [HttpGet("/About")]
        public IActionResult About()
        {
            return View();
        }
    }
}
