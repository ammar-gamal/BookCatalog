using BookCatalog.API.Exceptions;
using BookCatalog.API.Persistence;
using BookCatalog.API.Repositories.EFCore;
using BookCatalog.API.Repositories.InMemory;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services;
using BookCatalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BookCatalog.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
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

            builder.Services.AddSingleton(TimeProvider.System);
            //builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
            //builder.Services.AddSingleton(typeof(IBaseRepository<>), typeof(InMemoryBaseRepository<>));
            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(EFCoreBaseRepository<>));
            builder.Services.AddScoped<IBookRepository, EFCoreBookRepository>();
            builder.Services.AddScoped<IBookCopyRepository, EFCoreBookCopyRepository>();
            builder.Services.AddScoped<ILoanRepository, EFCoreLoanRepository>();
            builder.Services.AddScoped<IUserRepository, EFCoreUserRepository>();
            builder.Services.AddScoped<IAuthorService, AuthorService>();
            builder.Services.AddScoped<IBookService, BookService>();
            builder.Services.AddScoped<IBookCopyService, BookCopyService>();
            builder.Services.AddScoped<ILoanService, LoanService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers()
                            .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                            });
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
                if(builder.Environment.IsDevelopment())
                    options.EnableSensitiveDataLogging();
            });
            builder.Services.AddOpenApi();
            var app = builder.Build();
            app.UseExceptionHandler();
            if(!app.Environment.IsEnvironment("Testing"))
            {
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Database migrated successfully");
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
            // Configure the HTTP request pipeline.
            if(app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();

            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
