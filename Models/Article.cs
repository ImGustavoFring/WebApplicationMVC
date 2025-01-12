using System;
using System.Collections.Generic;

namespace WebApplicationMVC.Models;

public partial class Article
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Visibilityid { get; set; }

    public string Title { get; set; } = null!;

    public string? Previewurl { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<View> Views { get; set; } = new List<View>();

    public virtual Visibility Visibility { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
