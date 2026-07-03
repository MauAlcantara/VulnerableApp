using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace VulnerableApp.Controllers
{
    public class CommentController : Controller
    {
        private static List<string> _comments = new();
        private readonly ILogger<CommentController> _logger;

        public CommentController(ILogger<CommentController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Inicio Comment.Index.");

            try
            {
                sw.Stop();
                _logger.LogInformation("Fin Comment.Index. Tiempo: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
                return View(_comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Comment.Index");
                throw;
            }
        }

        [HttpPost]
        public IActionResult AddComment(string comment)
        {
            var sw = Stopwatch.StartNew();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var user = HttpContext.Session.GetString("User") ?? "Anónimo";

            _logger.LogInformation("Inicio Comment.AddComment. Parámetro comentario: {CommentParam}, Usuario: {User}, IP: {IP}", comment, user, ip);

            try
            {
                if (!string.IsNullOrEmpty(comment))
                {
                    _comments.Add(comment);
                    _logger.LogInformation("Comentario agregado exitosamente por el Usuario: {User}", user);
                }
                else
                {
                    _logger.LogWarning("Intento de enviar comentario vacío por el Usuario: {User}", user);
                }

                sw.Stop();
                _logger.LogInformation("Fin Comment.AddComment. Tiempo: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Comment.AddComment al agregar el comentario '{CommentParam}'", comment);
                throw;
            }
        }
    }
}