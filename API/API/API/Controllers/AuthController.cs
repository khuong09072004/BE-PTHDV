using System.Threading.Tasks;
using HuongDichVu.DTO;
using HuongDichVu.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public AuthController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // API Đăng ký
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Register auth)
        {
            // Kiểm tra trùng lặp Username
            if (await context.Auths.AnyAsync(u => u.Username == auth.Username))
                return BadRequest("Username đã tồn tại");

            // Mặc định vai trò là "User"
            var user = new Auth
            {
                Username = auth.Username,
                Password = auth.Password,
                Email = auth.Email,
                Role = "User"
            };

            context.Auths.Add(user);
            await context.SaveChangesAsync();

            return Ok("Đăng ký thành công");
        }

        // API Đăng nhập
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login loginUser)
        {
            var user = await context.Auths
                .FirstOrDefaultAsync(u => u.Username == loginUser.Username && u.Password == loginUser.Password);

            if (user == null)
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu");

            // Trả về thông tin vai trò của người dùng
            return Ok(new { message = "Đăng nhập thành công", role = user.Role });
        }
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAuths()
        {
            var users = await context.Auths.ToListAsync();
            return Ok(users);
        }

        // API Lấy người dùng theo ID
        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> GetAuthById(int id)
        {
            var user = await context.Auths.FindAsync(id);
            if (user == null)
                return NotFound("Người dùng không tồn tại");

            return Ok(user);
        }

        // API Cập nhật thông tin người dùng
        [HttpPut("UpdateAccount/{id}")]
        public async Task<IActionResult> UpdateAuth(int id, [FromBody] Register auth)
        {
            var user = await context.Auths.FindAsync(id);
            if (user == null)
                return NotFound("Người dùng không tồn tại");

            // Cập nhật thông tin
            user.Username = auth.Username;
            // Không cập nhật mật khẩu trực tiếp nếu không có yêu cầu, hoặc cần thêm logic mã hóa
            if (!string.IsNullOrEmpty(auth.Password))
            {
                user.Password = auth.Password; // Nên mã hóa mật khẩu trước khi lưu
            }

            await context.SaveChangesAsync();
            return Ok("Cập nhật thông tin thành công");
        }

        // API Xóa người dùng
        [HttpDelete("DeleteAccount/{id}")]
        public async Task<IActionResult> DeleteAuth(int id)
        {
            var user = await context.Auths.FindAsync(id);
            if (user == null)
                return NotFound("Người dùng không tồn tại");

            context.Auths.Remove(user);
            await context.SaveChangesAsync();

            return Ok("Xóa người dùng thành công");
        }

    }
}