using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HuongDichVu.Entities
{
    public partial class web_dataContext : DbContext
    {
        public web_dataContext()
        {
        }

        public web_dataContext(DbContextOptions<web_dataContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Book> Books { get; set; } = null!;
        public DbSet<DailyViews> DailyViews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=Daokhuongduy@1;database= data");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auth>(entity =>
            {
                entity.ToTable("auths");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasColumnName("password");

                entity.Property(e => e.Role)
                    .HasMaxLength(255)
                    .HasColumnName("role");

                entity.Property(e => e.Username)
                    .HasMaxLength(255)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("books");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.Genre)
                    .HasMaxLength(255)
                    .HasColumnName("genre");

                entity.Property(e => e.ImgSrc)
                    .HasMaxLength(255)
                    .HasColumnName("img_src");

                entity.Property(e => e.Instock)
                    .HasMaxLength(255)
                    .HasColumnName("instock");

                entity.Property(e => e.NumberAvailable).HasColumnName("number_available");

                entity.Property(e => e.Price)
                    .HasPrecision(10)
                    .HasColumnName("price");

                entity.Property(e => e.StarRating)
                    .HasMaxLength(20)
                    .HasColumnName("star_rating");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.Property(e => e.Upc)
                    .HasMaxLength(255)
                    .HasColumnName("upc");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
