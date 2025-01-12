using System;
using System.Collections.Generic;

namespace WebApplicationMVC.Models;

public partial class User
{
    public int Id { get; set; }

    public int Roleid { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string? Fullname { get; set; }

    public string? Avatarurl { get; set; }

    public string? Bio { get; set; }

    public string? Contactinfo { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Subscription> SubscriptionAuthors { get; set; } = new List<Subscription>();

    public virtual ICollection<Subscription> SubscriptionSubscribers { get; set; } = new List<Subscription>();

    public virtual ICollection<View> Views { get; set; } = new List<View>();
}
