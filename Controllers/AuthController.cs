using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VulnerableApp.Data;

namespace VulnerableApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Búsqueda segura con LINQ
            var user = _db.Users.FirstOrDefault(u => u.Username == username);

            // Validación doble (hash o texto plano para los usuarios semilla)
            bool isPasswordValid = user != null && (user.Password == password || BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));

            if (user == null || !isPasswordValid)
            {
                ViewBag.Error = "Credenciales inválidas";
                return View();
            }

            HttpContext.Session.SetString("User", user.Username);
            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("Dashboard");
        }

        public ActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login");

            var user = _db.Users.Find(userId.Value);
            return View(user);
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}