using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HuongDichVu.Entities
{
    public partial class Book
    {
        [Key]

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }

        public string? Upc { get; set; }

        public string? Title { get; set; }

        public string? Author { get; set; }

        public string? Genre { get; set; }

        public decimal? Price { get; set; }

        public string? ImgSrc { get; set; }

        public int? StarRating { get; set; }

        public string? status { get; set; }

        public int? viewCount { get; set; }

        public string? Description { get; set; }

        public DateTime? LastAccessedTime { get; set; }
    }
    public class GenreCount

    {

        public string Genre { get; set; }

        public int Count { get; set; }

    }
}
