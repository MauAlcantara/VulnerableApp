using Microsoft.AspNetCore.Mvc;
using VulnerableApp.Data;
using VulnerableApp.Models;
using System.Diagnostics;

namespace VulnerableApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SearchController> _logger;

        public SearchController(AppDbContext db, ILogger<SearchController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index(string search)
        {
            var sw = Stopwatch.StartNew();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var user = HttpContext.Session.GetString("User") ?? "Anónimo";

            _logger.LogInformation("Inicio Search.Index. Parámetro búsqueda: {SearchParam}, Usuario: {User}, IP: {IP}", search, user, ip);

            try
            {
                if (string.IsNullOrEmpty(search))
                {
                    sw.Stop();
                    _logger.LogInformation("Fin Search.Index (Búsqueda vacía). Tiempo: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
                    return View(new List<User>());
                }

                var users = _db.Users.Where(u => u.Username!.Contains(search)).ToList();

                sw.Stop();
                _logger.LogInformation("Fin Search.Index. Se encontraron {Count} resultados. Tiempo: {ElapsedMilliseconds} ms", users.Count, sw.ElapsedMilliseconds);
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Search.Index al buscar '{SearchParam}' por el Usuario: {User}", search, user);
                throw;
            }
        }
    }
}