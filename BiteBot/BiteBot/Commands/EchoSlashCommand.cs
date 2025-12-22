using Discord.Interactions;


namespace BiteBot.Commands
{
    public class EchoSlashCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("echo", "Echoes back the provided text")]
        public async Task EchoAsync([Summary("text","Text to echo")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                await RespondAsync("Usage: /echo <text>", ephemeral: true);
            else
                await RespondAsync(text);
        }
    }
}