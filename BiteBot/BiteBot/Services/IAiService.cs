namespace BiteBot.Services;

public interface IAiService
{
    Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
}

