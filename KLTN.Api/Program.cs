using KLTN.Api.Extensions;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8,0,21)));
});
//Add Identity 
builder.Services.AddIdentity<User,IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddCors();
builder.Services.AddMyService();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
