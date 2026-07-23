using Microsoft.EntityFrameworkCore;
using VulnerableApp.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRA TODOS TUS SERVICIOS AQUÍ
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSession();

// 2. CONFIGURA SERILOG
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// 3. CONFIGURA EL PIPELINE HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<VulnerableApp.Middlewares.GlobalLoggingMiddleware>();

// =========================================================================
// MIDDLEWARE DE CABECERAS DE SEGURIDAD (CORRECCIÓN DAST OWASP ZAP)
// =========================================================================
app.Use(async (context, next) =>
{
    // 1. CSP (CWE-693 / Plugin 10038)
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:;");

    // 2. Anti-Clickjacking (CWE-1021 / Plugin 10020)[cite: 4]
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // 3. Anti MIME-Sniffing (CWE-693 / Plugin 10021)[cite: 4]
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    await next();
});

app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

try
{
    Log.Information("Iniciando VulnerableApp");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Fallo crítico al iniciar la aplicación");
}
finally
{
    Log.CloseAndFlush();
}