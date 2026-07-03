using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VulnerableApp.Data;
using System.Diagnostics;

namespace VulnerableApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext db, ILogger<AuthController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var sw = Stopwatch.StartNew();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            _logger.LogInformation("Inicio Auth.Login. Intento de acceso para Usuario: {User} desde IP: {IP}", username, ip);

            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                bool isPasswordValid = user != null && (user.Password == password || BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));

                if (user == null || !isPasswordValid)
                {
                    _logger.LogWarning("Evento de Autenticación: Credenciales inválidas para Usuario: {User} desde IP: {IP}", username, ip);
                    ViewBag.Error = "Credenciales inválidas";
                    sw.Stop();
                    _logger.LogInformation("Fin Auth.Login (Fallido). Tiempo: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
                    return View();
                }

                HttpContext.Session.SetString("User", user.Username ?? "UsuarioDesconocido");
                HttpContext.Session.SetInt32("UserId", user.Id);

                sw.Stop();
                _logger.LogInformation("Evento de Autenticación: Login exitoso para Usuario: {User}. Fin Auth.Login. Tiempo: {ElapsedMilliseconds} ms", username, sw.ElapsedMilliseconds);
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Auth.Login intentando autenticar al Usuario: {User}", username);
                throw;
            }
        }

        public ActionResult Dashboard()
        {
            var sw = Stopwatch.StartNew();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var sessionUser = HttpContext.Session.GetString("User") ?? "Anónimo";

            _logger.LogInformation("Inicio Auth.Dashboard. Usuario: {User}, IP: {IP}", sessionUser, ip);

            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    _logger.LogWarning("Intento de acceso a Dashboard sin sesión. Redirigiendo a Login. IP: {IP}", ip);
                    return RedirectToAction("Login");
                }

                var user = _db.Users.Find(userId.Value);
                sw.Stop();
                _logger.LogInformation("Fin Auth.Dashboard. Tiempo: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Auth.Dashboard para el Usuario: {User}", sessionUser);
                throw;
            }
        }

        public ActionResult Logout()
        {
            var sessionUser = HttpContext.Session.GetString("User") ?? "Anónimo";
            _logger.LogInformation("Inicio Auth.Logout. Cerrando sesión para Usuario: {User}", sessionUser);

            HttpContext.Session.Clear();

            _logger.LogInformation("Fin Auth.Logout.");
            return RedirectToAction("Index", "Home");
        }
    }
}