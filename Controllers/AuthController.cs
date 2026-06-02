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
            // Vulnerabilidad 1: Credenciales predeterminadas quemadas en código
            if (username == "admin" && password == "admin")
            {
                HttpContext.Session.SetString("User", username);
                HttpContext.Session.SetInt32("UserId", 1);
                return RedirectToAction("Dashboard");
            }

            // Vulnerabilidad 2: Concatenación directa (SQL Injection)
            string query = "SELECT * FROM Users WHERE Username = '" + username + "' AND Password = '" + password + "'";
            var user = _db.Users.FromSqlRaw(query).FirstOrDefault();

            if (user != null)
            {
                HttpContext.Session.SetString("User", user.Username);
                HttpContext.Session.SetInt32("UserId", user.Id);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Usuario/contraseña inválido";
            return View();
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