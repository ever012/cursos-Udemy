namespace BibliotecaAPI
{
    public class LogueaPeticionMiddleware
    {
        private readonly RequestDelegate next;

        public LogueaPeticionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            //viene la peticion
            var logger = contexto.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation($"Peticion: {contexto.Request.Method} {contexto.Request.Path}");

            await next.Invoke(contexto);

            //se va la respuesta
            logger.LogInformation($"Respuesta: {contexto.Response.StatusCode}");

        }
    }

    public static class LogueaPeticionMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogueaPeticion(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogueaPeticionMiddleware>();
        }
    }
}
