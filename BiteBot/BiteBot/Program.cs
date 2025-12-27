using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BiteBot.Data;
using BiteBot.Repositories;
using BiteBot.Services;

namespace BiteBot;

internal abstract class Program
{
    private static void Main(string[] args) =>
        MainAsync(args).GetAwaiter().GetResult();

    private static async Task MainAsync(string[] _)
    {
        try
        {
            var configuration = BuildConfiguration();
            var serviceProvider = ConfigureServices(configuration);
            
            var bot = serviceProvider.GetRequiredService<IBot>();
            await bot.StartAsync(serviceProvider);
            
            await MigrateDatabaseAsync(serviceProvider);
            
            await RunApplicationAsync(bot);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Environment.Exit(-1);
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();
    }

    private static ServiceProvider ConfigureServices(IConfiguration configuration)
    {
        return new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            })
            .AddSingleton(configuration)
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
            .AddHttpClient()
            .AddScoped<IRestaurantRepository, RestaurantRepository>()
            .AddScoped<IRestaurantService, RestaurantService>()
            .AddScoped<IAuditService, AuditService>()
            .AddScoped<IAiService, OllamaAiService>()
            .AddScoped<IBot, Bot>()
            .BuildServiceProvider();
    }

    private static async Task MigrateDatabaseAsync(ServiceProvider serviceProvider)
    {
        // Ensure database is created/migrated on startup (simple approach). This will create the DB schema if missing.
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    private static async Task RunApplicationAsync(IBot bot)
    {
        // Check if running in an interactive console (local development) or non-interactive (Docker)
        if (Console.IsInputRedirected || !Environment.UserInteractive)
        {
            await RunNonInteractiveModeAsync();
        }
        else
        {
            await RunInteractiveModeAsync(bot);
        }
    }

    private static async Task RunNonInteractiveModeAsync()
    {
        // Running in Docker or non-interactive mode - wait indefinitely
        Console.WriteLine("Running in non-interactive mode. Press Ctrl+C to stop.");
        await Task.Delay(-1);
    }

    private static async Task RunInteractiveModeAsync(IBot bot)
    {
        // Running locally with interactive console
        Console.WriteLine("Press ESC or Q to quit.");
        do
        {
            var keyInfo = Console.ReadKey();
            if (keyInfo.Key is ConsoleKey.Escape or ConsoleKey.Q)
            {
                await bot.StopAsync();
                return;
            }
        } while (true);
    }
}