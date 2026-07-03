using Microsoft.AspNetCore.Mvc;
using VulnerableApp.Data;
using System.Diagnostics;

namespace VulnerableApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ApiController> _logger;

        public ApiController(AppDbContext db, ILogger<ApiController> logger) 
        { 
            _db = db; 
            _logger = logger;
        }

        [HttpGet("user/{id}")]
        public IActionResult GetUser(int id) 
        {
            var sw = Stopwatch.StartNew();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var sessionUser = HttpContext.Session.GetString("User") ?? "Anónimo";

            _logger.LogInformation("Inicio Api.GetUser. Parámetro ID: {IdRequested}, Usuario solicitante: {User}, IP: {IP}", id, sessionUser, ip);

            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (!currentUserId.HasValue)
                {
                    _logger.LogWarning("Api.GetUser: Acceso denegado (No autenticado) desde IP: {IP}", ip);
                    return StatusCode(401, "No autenticado.");
                }

                if (id != currentUserId.Value)
                {
                    _logger.LogWarning("Api.GetUser: Acceso denegado (IDOR prevenido). Usuario: {User} intentó acceder al ID {IdRequested}", sessionUser, id);
                    return StatusCode(403, "Acceso denegado.");
                }

                var user = _db.Users.Find(id); 
                if (user == null) 
                {
                    _logger.LogWarning("Api.GetUser: Usuario con ID {IdRequested} no encontrado", id);
                    return NotFound(); 
                }

                sw.Stop();
                _logger.LogInformation("Fin Api.GetUser exitoso. Tiempo: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
                return Ok(new { user.Id, user.Username, user.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Api.GetUser procesando ID {IdRequested}", id);
                throw;
            }
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var sw = Stopwatch.StartNew();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var sessionUser = HttpContext.Session.GetString("User") ?? "Anónimo";

            _logger.LogInformation("Inicio Api.GetAllUsers. Usuario: {User}, IP: {IP}", sessionUser, ip);

            try
            {
                var users = _db.Users.ToList();
                sw.Stop();
                _logger.LogInformation("Fin Api.GetAllUsers. Registros devueltos: {Count}. Tiempo: {ElapsedMilliseconds} ms", users.Count, sw.ElapsedMilliseconds);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Api.GetAllUsers solicitado por {User}", sessionUser);
                throw;
            }
        }
    }
}