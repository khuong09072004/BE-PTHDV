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
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Asn1.Ocsp;


namespace WebService.Controllers
{
    
    public static class EncryptionHelper
    {
        public static string EncryptString(string plainText, string key)
        {
            byte[] iv = new byte[16]; // Initialization vector (should be random)
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new
         MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);

                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);

        }

        // Hàm giải mã chuỗi
        public static string DecryptString(string cipherText, string key)
        {
            byte[] iv = new byte[16]; // Use the same IV as for encryption
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new
         MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();

                        }
                    }
                }
            }
        }
    }

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
            if (await context.Auths.AnyAsync(u => u.Email == EncryptionHelper.EncryptString(auth.Email, "1234567890123456")))
                return BadRequest("Email đã tồn tại");

            // Mặc định vai trò là "User"
            var user = new Auth
            {
                Username = auth.Username,
                Password = EncryptionHelper.EncryptString(auth.Password, "1234567890123456"), // Mã hóa mật khẩu
                Email = EncryptionHelper.EncryptString(auth.Email, "1234567890123456"),           // Mã hóa email
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
            .FirstOrDefaultAsync(u =>
                u.Username == loginUser.Username &&
                u.Password == EncryptionHelper.EncryptString(loginUser.Password, "1234567890123456") // So sánh mật khẩu đã mã hóa
            );

            if (user == null)
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu");

            // Trả về thông tin vai trò của người dùng
            return Ok(new { message = "Đăng nhập thành công",username= user.Username, role = user.Role });
        }
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAuths()
        {
            var users = await context.Auths.ToListAsync();

            // Giải mã email cho mỗi người dùng
            foreach (var user in users)
            {
                user.Email = EncryptionHelper.DecryptString(user.Email, "1234567890123456");
                user.Password = EncryptionHelper.DecryptString(user.Password, "1234567890123456");
            }

            return Ok(users);
        }

        // API Lấy người dùng theo ID
        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> GetAuthById(int id)
        {
            var user = await context.Auths.FindAsync(id);
            if (user == null)
                return NotFound("Người dùng không tồn tại");

            user.Email = EncryptionHelper.DecryptString(user.Email, "1234567890123456");
            user.Password = EncryptionHelper.DecryptString(user.Password, "1234567890123456");
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
            user.Email = EncryptionHelper.EncryptString(auth.Email, "1234567890123456");
            // Không cập nhật mật khẩu trực tiếp nếu không có yêu cầu, hoặc cần thêm logic mã hóa
            if (!string.IsNullOrEmpty(EncryptionHelper.EncryptString(auth.Email, "1234567890123456")))
            {
                context.Entry(user).Property(u => u.Password).IsModified = true;
                user.Password = EncryptionHelper.EncryptString(auth.Password, "1234567890123456"); 
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
            var user = await context.Auths.FirstOrDefaultAsync(u => (u.Email == EncryptionHelper.EncryptString(request.Email, "1234567890123456")));
            if (user == null)
            {
                return NotFound("Tài khoản không tồn tại");
            }

            // Tạo mã token (ví dụ: sử dụng GUID)
            string resetToken = Guid.NewGuid().ToString();

            // Lưu token vào cơ sở dữ liệu (ví dụ: thêm một cột ResetToken vào bảng Auths)
            user.ResetToken = resetToken;
            await context.SaveChangesAsync();

            // Gửi email chứa liên kết đặt lại mật khẩu
            string resetLink = $"{resetToken}"; 
            await SendResetPasswordEmailAsync(EncryptionHelper.DecryptString(user.Email, "1234567890123456"), resetLink);

            return Ok("Yêu cầu đặt lại mật khẩu đã được gửi đến email của bạn");
        }

        // Hàm gửi email
        private async Task SendResetPasswordEmailAsync(string toEmail, string resetLink)
        {

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
        [HttpPost("ConfirmToken")]
        public async Task<IActionResult> ConfirmToken([FromBody] string token)
        {
            var user = await context.Auths.FirstOrDefaultAsync(u => u.ResetToken == token);
            if (user == null)
            {
                return BadRequest("Token không hợp lệ");
            }
            user.ResetToken = "acessToChangePassword";
            await context.SaveChangesAsync();
            return Ok("Token hop le!");
        }
        
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            // Kiểm tra token
            if (request == null)
            {
                return BadRequest("Hay nhap mat khau");
            }
            var user = await context.Auths.FirstOrDefaultAsync(u => u.ResetToken == "acessToChangePassword");
           
            if (!request.ConfirmPassword.Equals( request.NewPassword) )
            {
                return BadRequest("Mật khẩu không khớp!");
            }
            // Cập nhật mật khẩu
            user.Password =  EncryptionHelper.EncryptString(request.NewPassword, "1234567890123456");
            user.ResetToken = null; // Xóa token sau khi sử dụng
            await context.SaveChangesAsync();

            return Ok("Đặt lại mật khẩu thành công");
        }

    }
}