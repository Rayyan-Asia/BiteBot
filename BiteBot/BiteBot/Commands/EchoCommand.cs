using Discord.Commands;

namespace BiteBot.Commands;

public class EchoCommand : ModuleBase<SocketCommandContext>
{
    [Command("echo")]
    [Summary("Echoes back what was said")]
    public async Task ExecuteAsync([Remainder] [Summary("The message to echo")] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            await ReplyAsync("Usage: /echo <phrase>");
            return;
        }
        await ReplyAsync(message);
    }
}