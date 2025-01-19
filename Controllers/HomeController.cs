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
        public async Task<IActionResult> Index(string searchString = "", string criterion = "date", string order = "desc")
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
                "author" => order == "asc" ? query.OrderBy(a => a.User.Username) : query.OrderByDescending(a => a.User.Username),
                _ => order == "asc" ? query.OrderBy(a => a.Createdat) : query.OrderByDescending(a => a.Createdat),
            };

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
