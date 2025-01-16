using Microsoft.EntityFrameworkCore;
using RestoreApiV2.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});


var app = builder.Build();

//Middleware
// Configure the HTTP request pipeline.

//app.UseAuthorization();

app.MapControllers();

DbInitializer.InitDb(app);

app.Run();
