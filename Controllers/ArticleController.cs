using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;

namespace WebApplicationMVC.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    [Route("Article")]
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _context.Articles
                .Include(a => a.User)            // Автор статьи
                .Include(a => a.Comments)        // Комментарии
                .Include(a => a.Tags)            // Теги
                .Include(a => a.Views)           // Просмотры
                .Include(a => a.Visibility)      // Видимость
                .Include(a => a.Ratings)         // Оценки
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            // Инициализируем пустые коллекции, если они null
            article.Comments = article.Comments ?? new List<Comment>();
            article.Tags = article.Tags ?? new List<Tag>();
            article.Ratings = article.Ratings ?? new List<Rating>();
            article.Views = article.Views ?? new List<View>();

            // Проверяем, был ли уже просмотр этой статьи этим пользователем
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingView = article.Views.FirstOrDefault(v => v.Userid == userId);

            if (existingView == null)
            {
                // Если такого просмотра нет, добавляем новый
                article.Views.Add(new View { Articleid = article.Id, Userid = userId });
                await _context.SaveChangesAsync();
            }

            return View(article);
        }

        [HttpPost("Details/{id}")]
        public async Task<IActionResult> AddComment(int id, string commentContent)
        {
            var article = await _context.Articles
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Articleid = article.Id,
                Userid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Content = commentContent,
                Createdat = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = article.Id });
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Tags)
                .Include(a => a.Visibility)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            if (!UserCanEdit(article))
            {
                return Forbid();
            }

            ViewBag.VisibilityOptions = await _context.Visibilities.ToListAsync();
            return View(article);
        }

        [HttpPost("Edit/{id}")] //проклятый эндпоинт
        public async Task<IActionResult> Edit(int id, Article article, string[] tagNames)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            var existingArticle = await _context.Articles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingArticle == null)
            {
                return NotFound();
            }

            if (!UserCanEdit(existingArticle))
            {
                return Forbid();
            }

            existingArticle.Title = article.Title;
            existingArticle.Content = article.Content;
            existingArticle.Visibilityid = article.Visibilityid;

            existingArticle.Tags.Clear();
            var tags = await GetOrCreateTagsAsync(tagNames);
            foreach (var tag in tags)
            {
                existingArticle.Tags.Add(tag);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = existingArticle.Id });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.VisibilityOptions = await _context.Visibilities.Select(v => v.Name).ToListAsync();
            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Article article, string[] tagNames)
        {
            if (ModelState.IsValid)
            {
                article.Userid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var tags = await GetOrCreateTagsAsync(tagNames);
                article.Tags = tags;

                _context.Articles.Add(article);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = article.Id });
            }

            ViewBag.VisibilityOptions = await _context.Visibilities.ToListAsync();
            return View(article);
        }

        private bool UserCanEdit(Article article)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            return article.Userid == userId || userRole == "Admin";
        }

        private async Task<List<Tag>> GetOrCreateTagsAsync(string[] tagNames)
        {
            var tags = new List<Tag>();
            foreach (var tagName in tagNames.Distinct())
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    _context.Tags.Add(tag);
                }
                tags.Add(tag);
            }
            await _context.SaveChangesAsync();
            return tags;
        }
    }
}

