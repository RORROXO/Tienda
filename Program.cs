using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using tienda.src.Exceptions;
using tienda.src.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configura Serilog usando appsettings.json y los servicios
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// Servicios principales
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var http = context.HttpContext;
            var errors = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var problem = new
            {
                status = StatusCodes.Status400BadRequest,
                code = "VALIDATION_ERROR",
                message = "One or more validation errors occurred.",
                traceId = http.TraceIdentifier,
                path = http.Request.Path.Value,
                method = http.Request.Method,
                timestamp = DateTime.UtcNow,
                errors
            };

            return new BadRequestObjectResult(problem);
        };
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tienda API", Version = "v1" });
});

var app = builder.Build();

// Middleware para enriquecer logs con TraceId, Path y Method
app.Use(async (context, next) =>
{
    Serilog.Context.LogContext.PushProperty("TraceId", context.TraceIdentifier);
    Serilog.Context.LogContext.PushProperty("RequestPath", context.Request.Path);
    Serilog.Context.LogContext.PushProperty("RequestMethod", context.Request.Method);

    await next();
});

// Middlewares globales
app.UseMiddleware<CorrelationMidware>();
app.UseMiddleware<ErrorHandlerMiddleware>();

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapControllers();

app.Run();