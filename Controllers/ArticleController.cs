﻿using Microsoft.AspNetCore.Authorization;
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
    [Route("Article")]
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.VisibilityOptions = await _context.Visibilities.ToListAsync();
            return View(new Article());
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(string title, string content, int visibilityId, string tagNames)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var visibility = await _context.Visibilities
                                               .FirstOrDefaultAsync(v => v.Id == visibilityId);

                if (visibility == null)
                {
                    ModelState.AddModelError("Visibilityid", "Выбранная видимость не существует.");
                    ViewBag.VisibilityOptions = await _context.Visibilities.ToListAsync();
                    return View();
                }

                var tagArray = tagNames.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(t => t.Trim())
                                       .ToArray();

                var tags = await GetOrCreateTagsAsync(tagArray);

                var article = new Article
                {
                    Title = title,
                    Content = content,
                    Visibilityid = visibilityId,
                    Userid = userId,
                    Tags = tags
                };

                _context.Articles.Add(article);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = article.Id });
            }

            ViewBag.VisibilityOptions = await _context.Visibilities.ToListAsync();
            return View();
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _context.Articles
                .Include(a => a.User)
                .Include(a => a.Comments)
                .Include(a => a.Tags)
                .Include(a => a.Views)
                .Include(a => a.Visibility)
                .Include(a => a.Ratings)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingView = article.Views.FirstOrDefault(v => v.Userid == userId);

            if (existingView == null)
            {
                article.Views.Add(new View { Articleid = article.Id, Userid = userId });
                await _context.SaveChangesAsync();
            }

            return View(article);
        }

        [HttpGet("Articles/{id?}")]
        public async Task<IActionResult> Articles(int? id)
        {
            if (!id.HasValue)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null || !int.TryParse(userId, out int parsedId))
                {
                    return Unauthorized();
                }
                id = parsedId;
            }

            var articles = await _context.Articles
                .Where(a => a.Userid == id)
                .ToListAsync();

            return View(articles);
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

            ViewBag.VisibilityOptions = await _context.Visibilities
                .Select(v => v.Name)
                .ToListAsync();

            return View(article);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, string title, string content, string visibilityName, string[] tagNames)
        {
            var existingArticle = await _context.Articles
                .Include(a => a.Tags)
                .Include(a => a.Visibility)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingArticle == null)
            {
                return NotFound();
            }

            if (!UserCanEdit(existingArticle))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(visibilityName))
            {
                ModelState.AddModelError("Visibility", "Видимость не может быть пустой.");
                ViewBag.VisibilityOptions = await _context.Visibilities
                    .Select(v => v.Name)
                    .ToListAsync();
                return View(existingArticle);
            }

            var visibility = await _context.Visibilities
                .FirstOrDefaultAsync(v => v.Name == visibilityName);

            if (visibility == null)
            {
                ModelState.AddModelError("Visibility", "Выбранная видимость не существует.");
                ViewBag.VisibilityOptions = await _context.Visibilities
                    .Select(v => v.Name)
                    .ToListAsync();
                return View(existingArticle);
            }

            existingArticle.Title = title;
            existingArticle.Content = content;
            existingArticle.Visibilityid = visibility.Id;

            var tags = await GetOrCreateTagsAsync(tagNames);

            existingArticle.Tags.Clear();
            foreach (var tag in tags)
            {
                existingArticle.Tags.Add(tag);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = existingArticle.Id });
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
