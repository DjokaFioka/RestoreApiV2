using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestoreApiV2.Data;
using RestoreApiV2.Entities;
using RestoreApiV2.Middleware;
using RestoreApiV2.RequestHelpers;
using RestoreApiV2.Services;

var builder = WebApplication.CreateBuilder(args);

// Optional: Customize logging
builder.Logging.ClearProviders();            // Clears default providers (optional)
builder.Logging.AddConsole();                // Adds console output
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddTransient<ExceptionMiddleware>();
builder.Services.AddScoped<PaymentsService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddIdentityApiEndpoints<User>(opt =>
{
    opt.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StoreContext>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("?? Program starting up...");

//Middleware
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("https://localhost:3080");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("api").MapIdentityApi<User>();
app.MapFallbackToController("Index", "Fallback");

await DbInitializer.InitDb(app);

app.Run();
