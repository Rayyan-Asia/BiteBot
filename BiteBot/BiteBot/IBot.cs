namespace BiteBot;

public interface IBot
{
    Task StartAsync(IServiceProvider serviceProvider);
    Task StopAsync();
}