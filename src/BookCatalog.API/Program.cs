using BookCatalog.API.Exceptions;
using BookCatalog.API.Logging;
using BookCatalog.API.Options;
using BookCatalog.API.Options.Validators;
using BookCatalog.API.Persistence;
using BookCatalog.API.Repositories;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services;
using BookCatalog.API.Services.Interfaces;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text.Json.Serialization;

namespace BookCatalog.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Bootstrap Logger to capture startup failures
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // -------------------------------------------------------------
            // Logging Configuration (Serilog)
            // -------------------------------------------------------------
            builder.Host.UseSerilog((context, services, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration);
                config.Enrich.With<TraceIdEnricher>();
            });

            // -------------------------------------------------------------
            // Strongly-Typed Configuration Validation (Fail-Fast on Start)
            // -------------------------------------------------------------
            builder.Services.AddOptions<DatabaseOptions>()
                .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateOnStart();
            builder.Services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidations>();

            // -------------------------------------------------------------
            // Centralized Error Handling & ProblemDetails
            // -------------------------------------------------------------
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                    var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                    context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
                };
            });
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            // -------------------------------------------------------------
            // Database Resilience Configuration 
            // -------------------------------------------------------------
            builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                options.UseSqlServer(dbOptions.ConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 6,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
            });

            // -------------------------------------------------------------
            // Graceful Shutdown Configuration
            // -------------------------------------------------------------
            builder.Services.Configure<HostOptions>(options =>
            {
                options.ShutdownTimeout = TimeSpan.FromSeconds(20);
            });

            // -------------------------------------------------------------
            // Health Checks (Liveness & Readiness)
            // -------------------------------------------------------------
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>(
                    name: "dbcontext",
                    tags: ["ready"]
                );

            // -------------------------------------------------------------
            // Dependency Injection (Repositories & Services)
            // -------------------------------------------------------------
            builder.Services.AddSingleton(TimeProvider.System);

            // Repositories
            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            builder.Services.AddScoped<IBookRepository, BookRepository>();
            builder.Services.AddScoped<IBookCopyRepository, BookCopyRepository>();
            builder.Services.AddScoped<ILoanRepository, LoanRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // Services
            builder.Services.AddScoped<IAuthorService, AuthorService>();
            builder.Services.AddScoped<IBookService, BookService>();
            builder.Services.AddScoped<IBookCopyService, BookCopyService>();
            builder.Services.AddScoped<ILoanService, LoanService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // -------------------------------------------------------------
            // Controllers & API Documentation
            // -------------------------------------------------------------
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddSwaggerGen();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // -------------------------------------------------------------
            // Middleware Pipeline
            // -------------------------------------------------------------
            app.UseExceptionHandler();
            app.UseSerilogRequestLogging();

            if(app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
                await ApplyMigrationsAsync(app);
            }

            // Health check endpoints
            app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecks("/healthz/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });


            app.MapControllers();

            await app.RunAsync();
        }
        catch(Exception ex) when(ex is not HostAbortedException)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task ApplyMigrationsAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrated successfully.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}