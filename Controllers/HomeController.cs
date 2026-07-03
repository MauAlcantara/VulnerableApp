using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VulnerableApp.Models;

namespace VulnerableApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var sw = Stopwatch.StartNew();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            _logger.LogInformation("Inicio Home.Index desde IP: {IP}", ip);

            sw.Stop();
            _logger.LogInformation("Fin Home.Index. Tiempo: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
            return View();
        }

        public IActionResult Privacy()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Inicio Home.Privacy");

            sw.Stop();
            _logger.LogInformation("Fin Home.Privacy. Tiempo: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogWarning("Acceso a Home.Error. Ocurrió una excepción no controlada en la aplicación.");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}