using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationMVC.Data;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    [Route("User")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Загружаем список ролей в виде строк (названий)
            var roles = await _context.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = roles;

            return View(user);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, string RoleName, [Bind("Id,Username,Email,Fullname,Bio,Contactinfo,Avatarurl")] User user)
        {
            // Очистим любые старые ошибки в ModelState
            ModelState.Clear();

            if (!ModelState.IsValid)
            {
                // Загружаем список ролей заново, если форма не прошла валидацию
                var roles = await _context.Roles.Select(r => r.Name).ToListAsync();
                ViewBag.Roles = roles;
                return View(user);
            }

            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            // Обновляем данные пользователя
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Fullname = user.Fullname;
            existingUser.Bio = user.Bio;
            existingUser.Contactinfo = user.Contactinfo;

            // Проверяем роль по имени
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleName);
            if (role == null)
            {
                // Если роль не найдена, возвращаем ошибку
                ModelState.AddModelError("RoleName", "Невалидная роль.");
                var roles = await _context.Roles.Select(r => r.Name).ToListAsync();
                ViewBag.Roles = roles;
                return View(user);
            }

            existingUser.Roleid = role.Id; // Устанавливаем ID роли
            existingUser.Avatarurl = user.Avatarurl;

            // Сохраняем изменения
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = user.Id });
        }
    }
}
