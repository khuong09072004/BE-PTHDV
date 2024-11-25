using System.Threading.Tasks;
using HuongDichVu.DTO;
using HuongDichVu.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using API.DTO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


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
            user.Email = auth.Email;
            // Không cập nhật mật khẩu trực tiếp nếu không có yêu cầu, hoặc cần thêm logic mã hóa
            if (!string.IsNullOrEmpty(auth.Password))
            {
                context.Entry(user).Property(u => u.Password).IsModified = true;
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



        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Kiểm tra xem người dùng có tồn tại không
            var user = await context.Auths.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("Email không tồn tại");
            }

            // Tạo mã token (ví dụ: sử dụng GUID)
            string resetToken = Guid.NewGuid().ToString();

            // Lưu token vào cơ sở dữ liệu (ví dụ: thêm một cột ResetToken vào bảng Auths)
            user.ResetToken = resetToken;
            await context.SaveChangesAsync();

            // Gửi email chứa liên kết đặt lại mật khẩu
            string resetLink = $"{resetToken}"; 
            await SendResetPasswordEmailAsync(user.Email, resetLink);

            return Ok("Yêu cầu đặt lại mật khẩu đã được gửi đến email của bạn");
        }

        // Hàm gửi email
        private async Task SendResetPasswordEmailAsync(string toEmail, string resetLink)
        {

            UserCredential credential;

            var fromAddress = new MailAddress("duydaokhuong9@gmail.com","duydaokhuong9"); // Thay thế bằng email của bạn
            var toAddress = new MailAddress(toEmail);
            const string fromPassword = "zqgerndhjrlzdjlb"; // Thay thế bằng mật khẩu email của bạn
            const string subject = "Đặt lại mật khẩu";
            string body = $"Sử dụng OTP Token sau để cấp lại mật khẩu của bạn: {resetLink}";
            //zqge rndh jrlz djlb
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // Thay thế bằng máy chủ SMTP của bạn
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address,fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                await smtp.SendMailAsync(message);

            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            // Kiểm tra token
            var user = await context.Auths.FirstOrDefaultAsync(u => u.ResetToken == request.Token);
            if (user == null)
            {
                return BadRequest("Token không hợp lệ");
            }

            // Cập nhật mật khẩu
            user.Password = request.NewPassword;
            user.ResetToken = null; // Xóa token sau khi sử dụng
            await context.SaveChangesAsync();

            return Ok("Đặt lại mật khẩu thành công");
        }

    }
}