using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AppBlog.Models;

public partial class BlogContext : DbContext
{
    public BlogContext()
    {
    }

    public BlogContext(DbContextOptions<BlogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-FC9EO7E;Initial Catalog=Blog;User ID=hoangtung;Password=Atng1234567890;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Salt)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Accounts_Roles");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CatId).HasName("PK_Catogories");

            entity.Property(e => e.CatId).HasColumnName("CatID");
            entity.Property(e => e.Alias).HasMaxLength(255);
            entity.Property(e => e.CatName).HasMaxLength(255);
            entity.Property(e => e.Cover).HasMaxLength(255);
            entity.Property(e => e.Icon).HasMaxLength(255);
            entity.Property(e => e.MetaDesc).HasMaxLength(255);
            entity.Property(e => e.MetaKey).HasMaxLength(255);
            entity.Property(e => e.Thumb).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Alias).HasMaxLength(255);
            entity.Property(e => e.Author).HasMaxLength(255);
            entity.Property(e => e.CatId).HasColumnName("CatID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.IsHot).HasColumnName("isHot");
            entity.Property(e => e.IsNewfeed).HasColumnName("isNewfeed");
            entity.Property(e => e.Scontents)
                .HasMaxLength(255)
                .HasColumnName("SContents");
            entity.Property(e => e.Thumb).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Account).WithMany(p => p.Posts)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_Posts_Accounts");

            entity.HasOne(d => d.Cat).WithMany(p => p.Posts)
                .HasForeignKey(d => d.CatId)
                .HasConstraintName("FK_Posts_Categories");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleDescription).HasMaxLength(50);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
