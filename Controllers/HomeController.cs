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
        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                                         .Include(a => a.Tags)
                                         .Include(a => a.User)
                                         .Include(a => a.Views)
                                         .Include(a => a.Ratings)
                                         .OrderByDescending(a => a.Createdat)
                                         .ToListAsync();

            ViewData["Tags"] = await _context.Tags.ToListAsync();

            return View(articles);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search(string searchString, string criterion = "date", string order = "desc")
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return View(new List<Article>());
            }

            var articlesQuery = _context.Articles
                .Where(a => a.Title.Contains(searchString));

            switch (criterion.ToLower())
            {
                case "views":
                    articlesQuery = order == "desc"
                        ? articlesQuery.OrderByDescending(a => a.Views.Count)
                        : articlesQuery.OrderBy(a => a.Views.Count);
                    break;
                case "comments":
                    articlesQuery = order == "desc"
                        ? articlesQuery.OrderByDescending(a => a.Comments.Count)
                        : articlesQuery.OrderBy(a => a.Comments.Count);
                    break;
                case "ratings":
                    articlesQuery = order == "desc"
                        ? articlesQuery.OrderByDescending(a => a.Ratings.Count)
                        : articlesQuery.OrderBy(a => a.Ratings.Count);
                    break;
                case "date":
                default:
                    articlesQuery = order == "desc"
                        ? articlesQuery.OrderByDescending(a => a.Createdat)
                        : articlesQuery.OrderBy(a => a.Createdat);
                    break;
            }

            var articles = await articlesQuery
                .Include(a => a.Tags)
                .Include(a => a.User)
                .Include(a => a.Views)
                .Include(a => a.Ratings)
                .Include(a => a.Comments)
                .ToListAsync();

            return View(articles);
        }

        [HttpGet("/About")]
        public IActionResult About()
        {
            return View();
        }
    }
}
