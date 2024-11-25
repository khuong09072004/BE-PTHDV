using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        [HttpGet("GetBooks")]
        public async Task<ActionResult<List<Book>>> Get()
        {
            var List = await web_dataContext.Books.Select(
                s => new Book
                {
                    Id=s.Id,
                    Upc = s.Upc,
                    Title = s.Title,
                    Genre = s.Genre,
                    Price = s.Price,
                    ImgSrc = s.ImgSrc,
                    StarRating=s.StarRating,
                    Instock=s.Instock,
                    NumberAvailable=s.NumberAvailable,
                    Description=s.Description
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
            var book = await web_dataContext.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            var bookDTO = new Book
            {
                Id=book.Id,
                Upc = book.Upc,
                Title = book.Title,
                Genre = book.Genre,
                Price = book.Price,
                ImgSrc = book.ImgSrc,
                StarRating = book.StarRating,
                Instock = book.Instock,
                NumberAvailable = book.NumberAvailable,
                Description = book.Description
            };
                    return Ok(bookDTO); // Trả về 200 OK với thông tin sách
        }
        [HttpPost("InsertBook")]
        public async Task<IActionResult> InsertUser(BookDTO book)
        {
            var entity = new Book()
            {
                Upc = book.Upc,
                Title = book.Title,
                Genre = book.Genre,
                Price = book.Price,
                ImgSrc = book.ImgSrc,
                StarRating = book.StarRating,
                Instock = book.Instock,
                NumberAvailable = book.NumberAvailable,
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
            entity.Genre = book.Genre;
            entity.Price = book.Price;
            entity.StarRating = book.StarRating;
            entity.Instock = book.Instock;
            entity.NumberAvailable = book.NumberAvailable;
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

            return NoContent(); // Trả về 204 No Content khi xóa thành công
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
                    Genre = book.Genre,
                    Price = book.Price,
                    ImgSrc = book.ImgSrc,
                    StarRating = book.StarRating,
                    Instock = book.Instock,
                    NumberAvailable = book.NumberAvailable,
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
    }
}
