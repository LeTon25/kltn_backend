using KLTN.BackgroundJobs.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowKLTNApi",
        policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigin").Value ?? throw new ArgumentNullException("Missing AllowedOrigin Configuration"))
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.ConfigureEmailSettings(builder.Configuration);
builder.Services.AddHangfireService(builder.Configuration);
builder.Services.AddCustomService();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowKLTNApi");
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCustomHangfireDashboard(builder.Configuration);
app.MapDefaultControllerRoute();

app.Run();
