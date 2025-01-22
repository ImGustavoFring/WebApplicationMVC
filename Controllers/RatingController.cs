using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    [Route("Rating")]
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(int articleId, int value)
        {
            if (value < 1 || value > 10)
            {
                return RedirectToAction("Details", "Article", new { id = articleId });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.Articleid == articleId && r.Userid == userId);

            if (existingRating != null)
            {
                return RedirectToAction("Details", "Article", new { id = articleId });
            }

            var rating = new Rating
            {
                Articleid = articleId,
                Userid = userId,
                Value = value
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Article", new { id = articleId });
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update(int articleId, int value, int? userId)
        {
            if (userId == null)
            {
                return Unauthorized();
            }

            if (value < 1 || value > 10)
            {
                return RedirectToAction("Details", "Article", new { id = articleId });
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.Articleid == articleId && (r.Userid == userId || User.IsInRole("Admin")));

            if (existingRating == null)
            {
                return RedirectToAction("Details", "Article", new { id = articleId });
            }

            if (existingRating.Userid != userId && !User.IsInRole("Admin"))
            {
                return Unauthorized();
            }

            existingRating.Value = value;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Article", new { id = articleId });
        }


        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(int articleId, int? userId)
        {
            if (userId == null)
            {
                return Unauthorized();
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.Articleid == articleId && (r.Userid == userId || User.IsInRole("Admin")));

            if (existingRating == null)
            {
                return RedirectToAction("Details", "Article", new { id = articleId });
            }

            _context.Ratings.Remove(existingRating);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Article", new { id = articleId });
        }
    }
}
