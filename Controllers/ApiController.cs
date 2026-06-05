using Microsoft.AspNetCore.Mvc;
using VulnerableApp.Data;

namespace VulnerableApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ApiController(AppDbContext db) { _db = db; }

        [HttpGet("user/{id}")]
        public IActionResult GetUser(int id) 
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            // Corrección: Usamos StatusCode manual en lugar de Unauthorized/Forbid
            if (!currentUserId.HasValue)
                return StatusCode(401, "No autenticado. Por favor inicia sesión.");

            if (id != currentUserId.Value)
                return StatusCode(403, "Acceso denegado. No puedes ver datos de otros usuarios.");

            var user = _db.Users.Find(id); 
            if (user == null) return NotFound(); 

            return Ok(new { user.Id, user.Username, user.Email });
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _db.Users.ToList();
            return Ok(users);
        }
    }
}