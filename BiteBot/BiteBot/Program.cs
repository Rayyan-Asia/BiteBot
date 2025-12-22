using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            .AddScoped<IBot, Bot>()
            .BuildServiceProvider();


        try
        {
            var bot = serviceProvider.GetRequiredService<IBot>();
            await bot.StartAsync(serviceProvider);
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