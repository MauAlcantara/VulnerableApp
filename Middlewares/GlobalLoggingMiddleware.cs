using Serilog.Context;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VulnerableApp.Middlewares
{
    public class GlobalLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalLoggingMiddleware> _logger;

        public GlobalLoggingMiddleware(RequestDelegate next, ILogger<GlobalLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Generar e inyectar el CorrelationId en los Headers
            var correlationId = Guid.NewGuid().ToString();
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            // Inyectar el CorrelationId en el contexto de Serilog para que TODO log de esta petición lo incluya
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                // 2. Iniciar cronómetro para Request Logging
                var sw = Stopwatch.StartNew();

                try
                {
                    // Pasar al siguiente middleware o controlador
                    await _next(context);

                    sw.Stop();

                    // Registrar método HTTP, ruta, código y tiempo de ejecución
                    _logger.LogInformation(
                        "Petición completada: HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {ElapsedMilliseconds} ms",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    // 3. Exception Middleware: Registrar excepciones no controladas
                    _logger.LogError(ex, "Unhandled: Error interno crítico capturado. Método: {RequestMethod} Ruta: {RequestPath}",
                        context.Request.Method, context.Request.Path);

                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Ocurrió un error interno en el servidor (500).");
                }
            }
        }
    }
}