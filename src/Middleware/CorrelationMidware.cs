using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace tienda.src.Middleware
{
    public class CorrelationMidware
    {
        private const string HeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationMidware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        // Cambiado a Invoke para compatibilidad .NET 9
        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out var cid) || string.IsNullOrWhiteSpace(cid))
            {
                cid = Guid.NewGuid().ToString("N");
            }

            context.Response.Headers[HeaderName] = cid;
            context.TraceIdentifier = cid;

            await _next(context);
        }
    }
}
