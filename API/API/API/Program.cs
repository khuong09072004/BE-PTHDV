using HuongDichVu.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.AllowAnyOrigin() // Địa chỉ frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
// Add services to the container.
builder.Services.AddEntityFrameworkMySQL()
    .AddDbContext<web_dataContext>(options =>
    {
        options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");// dùng CORS
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Kiểm tra xem admin đã tồn tại chưa
    if (!context.Auths.Any(u => u.Username == "admin"))
    {
        var admin = new Auth
        {

            Email = "adminsever@gmail.com",
            Username = "admin",
            Password = "Admin123",
            Role = "Admin"
        };

        context.Auths.Add(admin);
        context.SaveChanges();
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
