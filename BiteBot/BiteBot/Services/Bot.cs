using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BiteBot;

public class Bot : IBot
{
    private IServiceProvider? _serviceProvider;
    private readonly ILogger<Bot> _logger;
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    
    public Bot(ILogger<Bot> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        DiscordSocketConfig config = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        _client = new DiscordSocketClient(config);
        _interactions = new InteractionService(_client);
    }
    
    public async Task StartAsync(IServiceProvider serviceProvider)
    {
        var discordToken = _configuration["DiscordToken"] ?? throw new Exception("Discord token not found in configuration.");
        
        _logger.LogInformation("Starting bot...");
        _serviceProvider = serviceProvider;
        
        // Load interaction/slash command modules
        await _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);
        
        // Login and start the client
        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();
        
        // Wait for the Discord client to be Ready before registering commands
        var readyTcs = new TaskCompletionSource<bool>();
        Task ReadyHandler()
        {
            _logger.LogInformation("Discord client is ready. Registering slash commands...");
            readyTcs.TrySetResult(true);
            return Task.CompletedTask;
        }

        _client.Ready += ReadyHandler;

        // Wait until Ready
        await readyTcs.Task;

        // Remove handler to avoid memory leaks
        _client.Ready -= ReadyHandler;

        // Register slash commands to guild for instant registration (or globally if no guild ID)
        var guildIdStr = _configuration["GuildID"];
        if (!string.IsNullOrEmpty(guildIdStr) && ulong.TryParse(guildIdStr, out var guildId))
        {
            await _interactions.RegisterCommandsToGuildAsync(guildId);
            _logger.LogInformation("Slash commands registered to guild {GuildId} (instant).", guildId);
        }
        else
        {
            await _interactions.RegisterCommandsGloballyAsync();
            _logger.LogInformation("Slash commands registered globally (takes up to 1 hour to propagate).");
        }

        // Hook up interaction handler for slash commands
        _client.InteractionCreated += async (interaction) =>
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(ctx, _serviceProvider);
        };
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping bot...");
        await _client.LogoutAsync();
        await _client.StopAsync();
    }
}