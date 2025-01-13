using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplicationMVC.Data;
using Microsoft.EntityFrameworkCore;
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
            // Получаем все теги
            var tags = await _context.Tags.ToListAsync();

            // Получаем все статьи, отсортированные по дате (по убыванию)
            var articles = await _context.Articles
                                         .Include(a => a.Tags)
                                         .Include(a => a.User)
                                         .OrderByDescending(a => a.Createdat)
                                         .ToListAsync();

            // Передаем данные в представление
            ViewBag.Tags = tags;
            return View(articles);
        }

        [HttpGet("/About")]
        public IActionResult About()
        {
            return View();
        }
    }
}
