using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using API.DTO;
using HuongDichVu.DTO;
using HuongDichVu.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly web_dataContext web_dataContext;

        public BookController(web_dataContext web_dataContext)
        {
            this.web_dataContext = web_dataContext;
        }


        [HttpGet("GetAllBooks")]

        public async Task<ActionResult<List<Book>>> Get()

        {

            var List = await web_dataContext.Books.Select(

              s => new Book

              {

                  Id = s.Id,

                  Upc = s.Upc,

                  Title = s.Title,

                  Author = s.Author,

                  Genre = s.Genre,

                  Price = s.Price,

                  ImgSrc = s.ImgSrc,

                  StarRating = s.StarRating,

                  status = s.status,

                  viewCount = s.viewCount,

                  Description = s.Description

              }

            ).ToListAsync();



            if (List.Count < 0)

            {

                return NotFound();

            }

            else

            {

                return List;

            }

        }

        [HttpGet("GetBookById")]

        public async Task<ActionResult<BookDTO>> GetBookById(int id)

        {

            // Lấy thông tin sách

            var book = await web_dataContext.Books.FindAsync(id);

            if (book == null)

            {

                return NotFound();

            }

            book.viewCount = (book.viewCount ?? 0) + 1;

            var today = DateTime.UtcNow.Date;

            var dailyView = await web_dataContext.DailyViews

              .FirstOrDefaultAsync(v => v.BookId == id && v.ViewDate == today);



            if (dailyView != null)

            {

                dailyView.ViewCountDay += 1; // Tăng số lượt xem trong ngày

            }

            else

            {

                // Thêm mới nếu chưa có

                web_dataContext.DailyViews.Add(new DailyViews

                {

                    BookId = id,

                    ViewDate = today,

                    ViewCountDay = 1

                });

            }

            // Lưu thay đổi vào cơ sở dữ liệu

            await web_dataContext.SaveChangesAsync();

            // Chuyển đổi dữ liệu sang BookDTO

            var bookDTO = new BookDTO

            {

                Id = book.Id,

                Upc = book.Upc,

                Title = book.Title,

                Author = book.Author,

                Genre = book.Genre,

                Price = book.Price,

                ImgSrc = book.ImgSrc,

                StarRating = book.StarRating,

                Status = book.status,

                ViewCount = book.viewCount, // Trả về tổng lượt xem

                Description = book.Description

            };

            return Ok(bookDTO);

        }

        [HttpPost("InsertBook")]

        public async Task<IActionResult> InsertUser(BookDTO book)

        {

            var entity = new Book()

            {

                Upc = book.Upc,

                Title = book.Title,

                Author = book.Author,

                Genre = book.Genre,

                Price = book.Price,

                ImgSrc = book.ImgSrc,

                StarRating = book.StarRating,

                status = book.Status,

                viewCount = 0,

                Description = book.Description

            };



            web_dataContext.Books.Add(entity);

            await web_dataContext.SaveChangesAsync();



            // Giả sử bạn muốn trả về ID của cuốn sách đã tạo

            return CreatedAtAction(nameof(GetBookById), new { id = entity.Id }, entity);

        }



        [HttpPut("UpdateBook/{id}")]

        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookDTO book)

        {

            if (id <= 0)

            {

                return BadRequest("Id không hợp lệ.");

            }

            // Tìm sách với Id được cung cấp

            var entity = await web_dataContext.Books.FindAsync(id);

            if (entity == null)

            {

                return NotFound("Không tìm thấy sách với Id được cung cấp.");

            }

            // Cập nhật các trường

            entity.Upc = book.Upc;

            entity.Title = book.Title;

            entity.Author = book.Author;

            entity.Genre = book.Genre;

            entity.Price = book.Price;

            entity.StarRating = book.StarRating;

            entity.status = book.Status;

            entity.Description = book.Description;

            await web_dataContext.SaveChangesAsync();

            return Ok("Cập nhật sách thành công.");

        }



        [HttpDelete("DeleteBook/{id}")]

        public async Task<IActionResult> DeleteBook(int id)

        {

            var entity = await web_dataContext.Books.FirstOrDefaultAsync(s => s.Id == id);



            if (entity == null)

            {

                return NotFound();

            }

            // Xóa sách

            web_dataContext.Books.Attach(entity);

            web_dataContext.Books.Remove(entity);

            await web_dataContext.SaveChangesAsync();



            return NoContent();

        }

        [HttpGet("QuickSearch")]

        public async Task<ActionResult<List<BookDTO>>> QuickSearch(string keyword, string? genre)

        {

            if (string.IsNullOrEmpty(keyword))

            {

                return BadRequest("Vui lòng nhập từ khóa tìm kiếm.");

            }

            var query = web_dataContext.Books.AsQueryable();

            query = query.Where(book => book.Title.Contains(keyword));



            // Nếu có thể loại (genre), thêm điều kiện tìm kiếm theo thể loại

            if (!string.IsNullOrEmpty(genre))

            {

                query = query.Where(book => book.Genre.Contains(genre));

            }



            var results = await query

              .Where(book => book.Title.Contains(keyword)) // Tìm kiếm theo tên sách

              .Select(book => new Book

              {

                  Id = book.Id,

                  Upc = book.Upc,

                  Title = book.Title,

                  Author = book.Author,

                  Genre = book.Genre,

                  Price = book.Price,

                  ImgSrc = book.ImgSrc,

                  StarRating = book.StarRating,

                  status = book.status,

                  viewCount = book.viewCount,

                  Description = book.Description

              })

              .Take(10) // Giới hạn số lượng kết quả trả về

              .ToListAsync();



            if (results.Count == 0)

            {

                return NotFound("Không tìm thấy sách phù hợp với từ khóa.");

            }



            return Ok(results);

        }
        [HttpGet("GetGenres")]
        public async Task<ActionResult<List<string>>> GetGenres()
        {
            var genres = await web_dataContext.Books
                .Select(book => book.Genre)
                .Distinct()
                .ToListAsync();

            if (genres.Count == 0)
            {
                return NotFound("Không có thể loại sách nào.");
            }

            return Ok(genres);
        }

        [HttpGet("ColumnChart_CountByGenre")]

        public IActionResult GetBooksCountByGenre()

        {

            var genreCounts = web_dataContext.Books

              .GroupBy(b => b.Genre)

              .Select(g => new GenreCount

              {

                  Genre = g.Key,

                  Count = g.Count()

              })

              .ToList();



            return Ok(genreCounts);

        }

        [HttpGet("LineChart_ViewCountperDay")]
        public async Task<ActionResult<IEnumerable<ViewIncrementDTO>>> GetDailyViewIncrements()

        {

            var dailyViewData = await web_dataContext.DailyViews

              .GroupBy(v => v.ViewDate)

              .Select(g => new ViewIncrementDTO

              {

                  Date = g.Key,

                  TotalViewIncrement = g.Sum(v => v.ViewCountDay)

              }).OrderBy(v => v.Date)

              .ToListAsync();



            return Ok(dailyViewData);

        }
    }
}
