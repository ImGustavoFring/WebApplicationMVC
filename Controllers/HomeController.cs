using Microsoft.AspNetCore.Mvc;
using WebApplicationMVC.Data;

namespace WebApplicationMVC.Controllers
{
    [Route("Home")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
