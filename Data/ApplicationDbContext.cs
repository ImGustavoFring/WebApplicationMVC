using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
   
    }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<View> Views { get; set; }

    public virtual DbSet<Visibility> Visibilities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=coursework;Username=postgres;Password=14105");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("articles_pkey");

            entity.ToTable("articles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Previewurl).HasColumnName("previewurl");
            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .HasColumnName("title");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Visibilityid).HasColumnName("visibilityid");

            entity.HasOne(d => d.User).WithMany(p => p.Articles)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("articles_userid_fkey");

            entity.HasOne(d => d.Visibility).WithMany(p => p.Articles)
                .HasForeignKey(d => d.Visibilityid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("articles_visibilityid_fkey");

            entity.HasMany(d => d.Tags).WithMany(p => p.Articles)
                .UsingEntity<Dictionary<string, object>>(
                    "Articletag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("Tagid")
                        .HasConstraintName("articletags_tagid_fkey"),
                    l => l.HasOne<Article>().WithMany()
                        .HasForeignKey("Articleid")
                        .HasConstraintName("articletags_articleid_fkey"),
                    j =>
                    {
                        j.HasKey("Articleid", "Tagid").HasName("articletags_pkey");
                        j.ToTable("articletags");
                        j.IndexerProperty<int>("Articleid").HasColumnName("articleid");
                        j.IndexerProperty<int>("Tagid").HasColumnName("tagid");
                    });
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => new { e.Articleid, e.Userid }).HasName("comments_pkey");

            entity.ToTable("comments");

            entity.Property(e => e.Articleid).HasColumnName("articleid");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Article).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Articleid)
                .HasConstraintName("comments_articleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("comments_userid_fkey");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => new { e.Articleid, e.Userid }).HasName("ratings_pkey");

            entity.ToTable("ratings");

            entity.Property(e => e.Articleid).HasColumnName("articleid");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Article).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.Articleid)
                .HasConstraintName("ratings_articleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("ratings_userid_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => new { e.Subscriberid, e.Authorid }).HasName("subscriptions_pkey");

            entity.ToTable("subscriptions");

            entity.Property(e => e.Subscriberid).HasColumnName("subscriberid");
            entity.Property(e => e.Authorid).HasColumnName("authorid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");

            entity.HasOne(d => d.Author).WithMany(p => p.SubscriptionAuthors)
                .HasForeignKey(d => d.Authorid)
                .HasConstraintName("subscriptions_authorid_fkey");

            entity.HasOne(d => d.Subscriber).WithMany(p => p.SubscriptionSubscribers)
                .HasForeignKey(d => d.Subscriberid)
                .HasConstraintName("subscriptions_subscriberid_fkey");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.ToTable("tags");

            entity.HasIndex(e => e.Name, "tags_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Avatarurl).HasColumnName("avatarurl");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Contactinfo).HasColumnName("contactinfo");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_roleid_fkey");
        });

        modelBuilder.Entity<View>(entity =>
        {
            entity.HasKey(e => new { e.Articleid, e.Userid }).HasName("views_pkey");

            entity.ToTable("views");

            entity.Property(e => e.Articleid).HasColumnName("articleid");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");

            entity.HasOne(d => d.Article).WithMany(p => p.Views)
                .HasForeignKey(d => d.Articleid)
                .HasConstraintName("views_articleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Views)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("views_userid_fkey");
        });

        modelBuilder.Entity<Visibility>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("visibility_pkey");

            entity.ToTable("visibility");

            entity.HasIndex(e => e.Name, "visibility_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
