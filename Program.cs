using Microsoft.EntityFrameworkCore;
using Serilog;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
try
{
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddDbContext<StargateContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.SQLite(
            Path.Combine(AppContext.BaseDirectory, "logs.db"),
            tableName: "Logs"
        )
        .CreateBootstrapLogger(); // Use CreateBootstrapLogger instead of CreateLogger

    // CORS for Vite dev server
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", p => p
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader());
    });

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.SQLite(
            Path.Combine(AppContext.BaseDirectory, "logs.db"),
            tableName: "Logs"
        ));

    builder.Services.AddMediatR(cfg =>
    {
        cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
        cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowFrontend");

    app.UseAuthorization();

    app.MapControllers();

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting application");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


