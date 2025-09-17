using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configura Serilog usando appsettings.json y los servicios
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    Serilog.Context.LogContext.PushProperty("TraceId", context.TraceIdentifier);
    Serilog.Context.LogContext.PushProperty("RequestPath", context.Request.Path);
    Serilog.Context.LogContext.PushProperty("RequestMethod", context.Request.Method);

    await next();
});

app.Run();

//aaa