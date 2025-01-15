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
            // Получаем все статьи с дополнительными данными
            var articles = await _context.Articles
                                         .Include(a => a.Tags)
                                         .Include(a => a.User)
                                         .Include(a => a.Views)
                                         .Include(a => a.Ratings)
                                         .OrderByDescending(a => a.Createdat)
                                         .ToListAsync();

            ViewData["Tags"] = await _context.Tags.ToListAsync();

            // Передаем данные в представление
            return View(articles);
        }

        [HttpGet("/About")]
        public IActionResult About()
        {
            return View();
        }
    }
}
