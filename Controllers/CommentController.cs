using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    [Route("Comment")]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Add/{articleId}")]
        public async Task<IActionResult> Add(int articleId, string content)
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == articleId);

            if (article == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var comment = new Comment
            {
                Articleid = articleId,
                Userid = userId,
                Content = content
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Article", new { id = articleId });
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int articleId, string newContent)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Articleid == articleId && c.Userid == userId);

            if (comment == null)
            {
                return NotFound();
            }

            comment.Content = newContent;
            comment.Updatedat = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Article", new { id = articleId });
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int articleId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Articleid == articleId && c.Userid == userId);

            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Article", new { id = articleId });
        }
    }
}
