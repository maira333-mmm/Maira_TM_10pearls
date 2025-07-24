using Backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ✅ Add Controllers
builder.Services.AddControllers();

// ✅ Build the app
var app = builder.Build();

// ✅ Use Middleware (IMPORTANT ORDER)
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthorization(); // (If using [Authorize], otherwise optional)

// ✅ Map Routes
app.MapControllers();

app.Run();
