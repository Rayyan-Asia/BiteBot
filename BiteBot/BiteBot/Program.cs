using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BiteBot.Data;

namespace BiteBot;

internal abstract class Program
{
    private static void Main(string[] args) =>
        MainAsync(args).GetAwaiter().GetResult();

    private static async Task MainAsync(string[] _)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();
        
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            })
            .AddSingleton<IConfiguration>(configuration)
            // Configure EF Core DbContext with PostgreSQL. Expects a connection string named "DefaultConnection" in configuration (e.g., user secrets).
            .AddDbContext<AppDbContext>((provider, options) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var conn = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(conn))
                {
                    // Fallback: try configuration key directly
                    conn = config["DefaultConnection"];
                }
                options.UseNpgsql(conn);
            })
            .AddScoped<IBot, Bot>()
            .BuildServiceProvider();


        try
        {
            var bot = serviceProvider.GetRequiredService<IBot>();
            await bot.StartAsync(serviceProvider);
            // Ensure database is created/migrated on startup (simple approach). This will create the DB schema if missing.
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
            do
            {
                var keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.Q)
                {
                    await bot.StopAsync();
                    return;
                }
            } while (true);
        }catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Environment.Exit(-1);
        }
    }
}