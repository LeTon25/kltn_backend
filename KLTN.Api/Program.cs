using FluentValidation;
using FluentValidation.AspNetCore;
using KLTN.Api.Extensions;
using KLTN.Api.Middleware;
using KLTN.Application.DTOs.Users;
using KLTN.Infrastructure.Data;
using KLTN.Infrastructure.Seeders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddAppConfigurations();
Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.Console())
    .CreateLogger();
builder.Services.AddControllers();
//Add Identity 
builder.Services.ConfigureIdentity();
// Add Authentication


builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddAuthorization();
//Add Aws Service
builder.Services.AddAwsStorageService(builder.Configuration);
//Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8,0,21)));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(
    options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("Authorization");
        });
    }
);
builder.Services.AddMyService();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<CreateUserRequestDtoValidator>();
//Add Mailing
builder.Services.ConfigureEmailSettings(builder.Configuration);
//
builder.Services.ConfigureAuthentication(builder.Configuration);

//Add HttpService
builder.Services.AddCustomHttpServices();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    option =>
    {
        option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new List<string>{}
        }
    });
    }
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ErrorWrappingMiddleware>();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseDefaultFiles();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Log.Information("Tạo dữ liệu mẫu");
        var dbInitializer = services.GetService<DbInitializer>();
        dbInitializer.Seed().Wait();
        Log.Information("Tạo dữ liệu mẫu thành công");

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Có lỗi khi chèn dữ liêu vào database.");
    }
}
app.Run();
