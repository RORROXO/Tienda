using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using tienda.src.Middleware; // 👈 cambia el namespace según dónde pusiste el middleware
using tienda.src.Exceptions;  // 👈 cambia el namespace según dónde pusiste las excepciones
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 👇 Configuración de Serilog (puedes comentarlo si aún no lo activas)
//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .WriteTo.Console()
//    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

builder.Host.UseSerilog();

// 👇 Reemplaza AddOpenApi por AddControllers (si quieres OpenAPI, luego agregas Swagger manualmente)
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

// 👇 Activar FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tienda API", Version = "v1" });
});

var app = builder.Build();

// Middlewares globales
app.UseMiddleware<CorrelationMidware>();
app.UseMiddleware<ErrorHandlerMiddleware>();

// 👇 Si quieres dejar OpenAPI, activa Swagger aquí
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers(); // 👈 Necesario para que tus endpoints funcionen

app.Run();

//sssss
//aaa

