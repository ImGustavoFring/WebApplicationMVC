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
                    Username = "admin",
                    Email = "admin@example.com",
                    Passwordhash = BCrypt.Net.BCrypt.HashPassword("adminpassword"),
                    Roleid = adminRole.Id,
                    Fullname = "Admin User",
                    Avatarurl = "https://example.com/admin-avatar.jpg",
                    Bio = "Admin bio",
                    Contactinfo = "admin-contact",
                    Createdat = localNow 
                },
                new User
                {
                    Username = "user",
                    Email = "user@example.com",
                    Passwordhash = BCrypt.Net.BCrypt.HashPassword("userpassword"),
                    Roleid = userRole.Id,
                    Fullname = "Regular User",
                    Avatarurl = "https://example.com/user-avatar.jpg",
                    Bio = "User bio",
                    Contactinfo = "user-contact",
                    Createdat = localNow  
                }
            );
            context.SaveChanges();
        }

        // Tags
        if (!context.Tags.Any())
        {
            context.Tags.AddRange(
                new Tag { Name = "Tech" },
                new Tag { Name = "Programming" },
                new Tag { Name = "C#" }
            );
            context.SaveChanges();
        }

        // Articles
        if (!context.Articles.Any())
        {
            var admin = context.Users.First(u => u.Username == "admin");
            var visibility = context.Visibilities.First(v => v.Name == "Public");

            var article1 = new Article
            {
                Title = "First Article",
                Content = "This is the content of the first article.",
                Userid = admin.Id,
                Visibilityid = visibility.Id,
                Createdat = localNow,
                Tags = new List<Tag> {
                    context.Tags.First(t => t.Name == "Tech"),
                    context.Tags.First(t => t.Name == "Programming")
                }
            };

            var article2 = new Article
            {
                Title = "Second Article",
                Content = "This is the content of the second article.",
                Userid = admin.Id,
                Visibilityid = visibility.Id,
                Createdat = localNow,  
                Tags = new List<Tag> {
                    context.Tags.First(t => t.Name == "C#")
                }
            };

            context.Articles.AddRange(article1, article2);
            context.SaveChanges();
        }

        // Comments
        if (!context.Comments.Any())
        {
            var user = context.Users.First(u => u.Username == "user");

            context.Comments.AddRange(
                new Comment
                {
                    Articleid = context.Articles.First(a => a.Title == "First Article").Id,
                    Userid = user.Id,
                    Content = "Great article!",
                    Createdat = localNow 
                }
            );
            context.SaveChanges();
        }

        // Ratings
        if (!context.Ratings.Any())
        {
            var user = context.Users.First(u => u.Username == "user");

            context.Ratings.AddRange(
                new Rating
                {
                    Articleid = context.Articles.First(a => a.Title == "First Article").Id,
                    Userid = user.Id,
                    Value = 5,
                    Createdat = localNow 
                }
            );
            context.SaveChanges();
        }

        // Subscriptions
        if (!context.Subscriptions.Any())
        {
            var subscriber = context.Users.First(u => u.Username == "user");
            var author = context.Users.First(u => u.Username == "admin");

            context.Subscriptions.AddRange(
                new Subscription
                {
                    Subscriberid = subscriber.Id,
                    Authorid = author.Id,
                    Createdat = localNow 
                }
            );
            context.SaveChanges();
        }

        // Views
        if (!context.Views.Any())
        {
            var user = context.Users.First(u => u.Username == "user");

            context.Views.AddRange(
                new View
                {
                    Articleid = context.Articles.First(a => a.Title == "First Article").Id,
                    Userid = user.Id,
                    Createdat = localNow 
                }
            );
            context.SaveChanges();
        }
    }
}
