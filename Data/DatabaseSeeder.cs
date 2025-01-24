using Microsoft.EntityFrameworkCore;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Data;

public class Seeder
{
    public static void Seed(ApplicationDbContext context)
    {
        DateTime localNow = DateTime.UtcNow.ToLocalTime();

        // Roles
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "User" }
            );
            context.SaveChanges();
        }

        // Visibility
        if (!context.Visibilities.Any())
        {
            context.Visibilities.AddRange(
                new Visibility { Name = "Public" },
                new Visibility { Name = "Private" }
            );
            context.SaveChanges();
        }

        // Users
        if (!context.Users.Any())
        {
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            var userRole = context.Roles.First(r => r.Name == "User");

            context.Users.AddRange(
                new User
                {
                    Username = "Admin",
                    Email = "admin@example.com",
                    Passwordhash = BCrypt.Net.BCrypt.HashPassword("adminpassword"),
                    Roleid = adminRole.Id,
                    Fullname = "Администратор сайта",
                    Avatarurl = "https://example.com/admin-avatar.jpg",
                    Bio = "Является администратором сайта и отвечает за управление контентом.",
                    Contactinfo = "admin@example.com",
                    Createdat = localNow
                },
                new User
                {
                    Username = "User1",
                    Email = "user1@example.com",
                    Passwordhash = BCrypt.Net.BCrypt.HashPassword("user1password"),
                    Roleid = userRole.Id,
                    Fullname = "Пользователь Иванов",
                    Avatarurl = "https://example.com/user1-avatar.jpg",
                    Bio = "Любитель технологий и IT, пишет статьи об алгоритмах и программировании.",
                    Contactinfo = "user1-contact@example.com",
                    Createdat = localNow
                },
                new User
                {
                    Username = "User2",
                    Email = "user2@example.com",
                    Passwordhash = BCrypt.Net.BCrypt.HashPassword("user2password"),
                    Roleid = userRole.Id,
                    Fullname = "Пользователь Петров",
                    Avatarurl = "https://example.com/user2-avatar.jpg",
                    Bio = "Инженер-системщик с опытом работы в сетях и базах данных.",
                    Contactinfo = "user2-contact@example.com",
                    Createdat = localNow
                }
            );
            context.SaveChanges();
        }

        // Tags
        if (!context.Tags.Any())
        {
            context.Tags.AddRange(
                new Tag { Name = "Технологии" },
                new Tag { Name = "Программирование" },
                new Tag { Name = "C#" },
                new Tag { Name = "Администрирование" },
                new Tag { Name = "Алгоритмы и структуры данных" },
                new Tag { Name = "Оптимизация" },
                new Tag { Name = "Сборки компьютера" },
                new Tag { Name = "Смартфоны" },
                new Tag { Name = "Сети" },
                new Tag { Name = "Базы Данных" },
                new Tag { Name = "ИИ" },
                new Tag { Name = "Безопасность" }
            );
            context.SaveChanges();
        }

        // Articles
        if (!context.Articles.Any())
        {
            var admin = context.Users.First(u => u.Username == "Admin");
            var user1 = context.Users.First(u => u.Username == "User1");
            var user2 = context.Users.First(u => u.Username == "User2");
            var publicVisibility = context.Visibilities.First(v => v.Name == "Public");

            context.Articles.AddRange(
                new Article
                {
                    Title = "Технологии будущего",
                    Content = "В этой статье мы рассмотрим, как современные технологии влияют на будущее человечества.",
                    Userid = admin.Id,
                    Visibilityid = publicVisibility.Id,
                    Createdat = localNow,
                    Tags = new List<Tag>
                    {
                        context.Tags.First(t => t.Name == "Технологии"),
                        context.Tags.First(t => t.Name == "ИИ")
                    }
                },
                new Article
                {
                    Title = "Секреты программирования на C#",
                    Content = "Глубокий обзор особенностей и приемов программирования на языке C#.",
                    Userid = user1.Id,
                    Visibilityid = publicVisibility.Id,
                    Createdat = localNow,
                    Tags = new List<Tag>
                    {
                        context.Tags.First(t => t.Name == "Программирование"),
                        context.Tags.First(t => t.Name == "C#")
                    }
                },
                new Article
                {
                    Title = "Оптимизация алгоритмов",
                    Content = "Как улучшить производительность алгоритмов и структур данных в реальных проектах.",
                    Userid = user2.Id,
                    Visibilityid = publicVisibility.Id,
                    Createdat = localNow,
                    Tags = new List<Tag>
                    {
                        context.Tags.First(t => t.Name == "Алгоритмы и структуры данных"),
                        context.Tags.First(t => t.Name == "Оптимизация")
                    }
                },
                new Article
                {
                    Title = "Смартфоны: от выбора до оптимизации",
                    Content = "Руководство по выбору и оптимальному использованию современных смартфонов.",
                    Userid = user1.Id,
                    Visibilityid = publicVisibility.Id,
                    Createdat = localNow,
                    Tags = new List<Tag>
                    {
                        context.Tags.First(t => t.Name == "Смартфоны"),
                        context.Tags.First(t => t.Name == "Технологии")
                    }
                },
                new Article
                {
                    Title = "Секреты баз данных",
                    Content = "Основы и углубленные темы работы с современными базами данных.",
                    Userid = user2.Id,
                    Visibilityid = publicVisibility.Id,
                    Createdat = localNow,
                    Tags = new List<Tag>
                    {
                        context.Tags.First(t => t.Name == "Базы Данных"),
                        context.Tags.First(t => t.Name == "Администрирование")
                    }
                }
            );
            context.SaveChanges();
        }

        // Comments
        if (!context.Comments.Any())
        {
            var user1 = context.Users.First(u => u.Username == "User1");
            var user2 = context.Users.First(u => u.Username == "User2");

            context.Comments.AddRange(
                new Comment
                {
                    Articleid = context.Articles.First(a => a.Title == "Технологии будущего").Id,
                    Userid = user1.Id,
                    Content = "Очень интересная статья! Спасибо за полезную информацию.",
                    Createdat = localNow
                },
                new Comment
                {
                    Articleid = context.Articles.First(a => a.Title == "Секреты программирования на C#").Id,
                    Userid = user2.Id,
                    Content = "Отличный обзор возможностей языка.",
                    Createdat = localNow
                }
            );
            context.SaveChanges();
        }

        // Ratings
        if (!context.Ratings.Any())
        {
            var user1 = context.Users.First(u => u.Username == "User1");
            var user2 = context.Users.First(u => u.Username == "User2");

            context.Ratings.AddRange(
                new Rating
                {
                    Articleid = context.Articles.First(a => a.Title == "Технологии будущего").Id,
                    Userid = user1.Id,
                    Value = 5,
                    Createdat = localNow
                },
                new Rating
                {
                    Articleid = context.Articles.First(a => a.Title == "Секреты программирования на C#").Id,
                    Userid = user2.Id,
                    Value = 4,
                    Createdat = localNow
                }
            );
            context.SaveChanges();
        }

        // Subscriptions
        if (!context.Subscriptions.Any())
        {
            var user1 = context.Users.First(u => u.Username == "User1");
            var user2 = context.Users.First(u => u.Username == "User2");
            var admin = context.Users.First(u => u.Username == "Admin");

            context.Subscriptions.AddRange(
                new Subscription
                {
                    Subscriberid = user1.Id,
                    Authorid = admin.Id,
                    Createdat = localNow
                },
                new Subscription
                {
                    Subscriberid = user2.Id,
                    Authorid = user1.Id,
                    Createdat = localNow
                },
                new Subscription
                {
                    Subscriberid = admin.Id,
                    Authorid = user2.Id,
                    Createdat = localNow
                }
            );
            context.SaveChanges();
        }
    }
}
