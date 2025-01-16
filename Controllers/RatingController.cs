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
                return BadRequest("Оценка должна быть в диапазоне от 1 до 10.");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.Articleid == articleId && r.Userid == userId);

            if (existingRating != null)
            {
                return BadRequest("Вы уже поставили оценку этой статье.");
            }

            var rating = new Rating
            {
                Articleid = articleId,
                Userid = userId,
                Value = value
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return Ok("Оценка успешно добавлена.");
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update(int articleId, int value)
        {
            if (value < 1 || value > 10)
            {
                return BadRequest("Оценка должна быть в диапазоне от 1 до 10.");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.Articleid == articleId && r.Userid == userId);

            if (existingRating == null)
            {
                return NotFound("Оценка не найдена.");
            }

            existingRating.Value = value;
            await _context.SaveChangesAsync();

            return Ok("Оценка успешно обновлена.");
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(int articleId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.Articleid == articleId && r.Userid == userId);

            if (existingRating == null)
            {
                return NotFound("Оценка не найдена.");
            }

            _context.Ratings.Remove(existingRating);
            await _context.SaveChangesAsync();

            return Ok("Оценка успешно удалена.");
        }
    }
}
