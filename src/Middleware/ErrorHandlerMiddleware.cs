using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using tienda.src.Exceptions; // <-- Agrega esto si tus excepciones están en este namespace

namespace tienda.src.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
        {
            var (status, code, message, errors) = MapException(ex);

            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId} Path: {Path}", ctx.TraceIdentifier, ctx.Request.Path);

            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = (int)status;

            var payload = new
            {
                status = ctx.Response.StatusCode,
                code,
                message,
                traceId = ctx.TraceIdentifier,
                path = ctx.Request.Path.Value,
                method = ctx.Request.Method,
                timestamp = DateTime.UtcNow,
                errors
            };

            await ctx.Response.WriteAsJsonAsync(payload);
        }
        private static (HttpStatusCode, string, string, object?) MapException(Exception ex)
        {
            return ex switch
            {
                NotFoundException nf => (HttpStatusCode.NotFound, nf.ErrorCode, nf.Message, null),
                ConflictException cf => (HttpStatusCode.Conflict, cf.ErrorCode, cf.Message, null),
                ForbiddenException fb => (HttpStatusCode.Forbidden, fb.ErrorCode, fb.Message, null),
                UnauthorizedAppException ua => (HttpStatusCode.Unauthorized, ua.ErrorCode, ua.Message, null),
                BadRequestAppException br => (HttpStatusCode.BadRequest, br.ErrorCode, br.Message, br.Errors),
                ValidationException fv => (
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR",
                    "One or more validation errors occurred.",
                    fv.Errors.GroupBy(e => e.PropertyName)
                             .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                ),
                _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An internal server error occurred.", null)
            };
        }
    }
}