using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordEmbedGenerator
{
    public class Program
    {
        private const string Token = "Enter your token here";
        private DiscordSocketClient _discordSocketClient;

        private static void Main()
        {
            new Program().OnBotStartup().GetAwaiter().GetResult();
        }

        private async Task OnBotStartup()
        {
            _discordSocketClient = new DiscordSocketClient();
            var config = new CommandServiceConfig {DefaultRunMode = RunMode.Async};
            var commandService = new CommandService(config);
            await _discordSocketClient.LoginAsync(TokenType.Bot, Token);
            await _discordSocketClient.StartAsync();
            await new CommandHandler(_discordSocketClient, commandService).InstallCommandsAsync();
            Console.ReadLine();
        }
    }
}