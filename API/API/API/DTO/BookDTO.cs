using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuongDichVu.DTO
{
    public class BookDTO
    {
        public string? Upc { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public decimal? Price { get; set; }
        public string? ImgSrc { get; set; }
        public string? StarRating { get; set; }
        public string? Instock { get; set; }
        public int? NumberAvailable { get; set; }
        public string? Description { get; set; }
    }
}
