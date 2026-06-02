using Microsoft.EntityFrameworkCore;
using VulnerableApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRA TODOS TUS SERVICIOS AQUÍ (Antes del builder.Build)
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CONSTRUYE LA APLICACIÓN
builder.Services.AddSession();
var app = builder.Build();

// 3. CONFIGURA EL PIPELINE HTTP (Middlewares)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();