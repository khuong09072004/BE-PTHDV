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

                optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=Duong1997@;database=web_data");

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



                entity.Property(e => e.status)

                  .HasMaxLength(255)

                  .HasColumnName("status");



                entity.Property(e => e.viewCount).HasColumnName("viewCount");



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

                entity.Property(e => e.Author)

                  .HasMaxLength(255)

                  .HasColumnName("author");

            });



            OnModelCreatingPartial(modelBuilder);

        }



        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
