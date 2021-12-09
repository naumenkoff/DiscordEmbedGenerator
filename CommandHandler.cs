using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;

        public CommandHandler(DiscordSocketClient client, CommandService command)
        {
            _command = command;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            await _command.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            _client.Log += Log;
            _client.MessageReceived += HandleCommandAsync;
            _command.CommandExecuted += OnCommandExecutedAsync;
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            try
            {
                if (messageParam is not SocketUserMessage message) return;

                var argPos = 0;

                if (!(message.HasCharPrefix('!', ref argPos) ||
                      message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                    || message.Author.IsBot)
                    return;

                var context = new SocketCommandContext(_client, message);
                await _command.ExecuteAsync(context, argPos, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static Task Log(LogMessage arg)
        {
            Console.ForegroundColor = arg.Severity switch
            {
                LogSeverity.Critical => ConsoleColor.Red,
                LogSeverity.Error => ConsoleColor.DarkRed,
                LogSeverity.Info => ConsoleColor.Green,
                LogSeverity.Warning => ConsoleColor.Yellow,
                _ => Console.ForegroundColor
            };
            Console.WriteLine($"[{DateTime.Now}]\t" + arg.Message);
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            var commandName = command.IsSpecified ? command.Value.Name : "A command";
            await Log(new LogMessage(LogSeverity.Info, "CommandExecution",
                $"Command !{commandName} was executed by {context.User}"));
        }

        public class MainModule : ModuleBase<SocketCommandContext>
        {
            [Command("embed", RunMode = RunMode.Async)]
            public async Task GenerateEmbed(CustomInput.Embed userInput)
            {
                var embed = EmbedGenerator.Generate(userInput);
                //await Context.Channel.DeleteMessageAsync(Context.Message);
                await Context.Channel.SendMessageAsync(string.Empty, false, embed);
            }

            [Group("admin")]
            [RequireUserPermission(GuildPermission.Administrator)]
            public class AdminModule : ModuleBase<SocketCommandContext>
            {
                [Command("clearchat", RunMode = RunMode.Async)]
                public async Task ClearMessagesAsync(int num)
                {
                    var startTime = DateTime.Now;
                    var messages = await Context.Channel.GetMessagesAsync(num + 1).FlattenAsync();
                    var lastMessages = messages as IMessage[] ?? messages.ToArray();
                    var twoWeeksMessage = 0;
                    for (var index = 0; index < lastMessages.Length; index++)
                    {
                        var message = lastMessages[index];
                        var creationTime = DateTime.Now - message.CreatedAt;
                        if (!(Math.Abs(creationTime.TotalHours) > 336)) continue;
                        twoWeeksMessage = index;
                        break;
                    }

                    if (twoWeeksMessage == 0)
                    {
                        await ((SocketTextChannel)Context.Channel).DeleteMessagesAsync(lastMessages);
                    }
                    else
                    {
                        await ((SocketTextChannel)Context.Channel).DeleteMessagesAsync(
                            lastMessages.Take(twoWeeksMessage));
                        messages = await Context.Channel.GetMessagesAsync(num + 1 - twoWeeksMessage).FlattenAsync();
                        foreach (var msg in messages)
                            if (msg != null)
                                await msg.DeleteAsync();
                    }

                    var consumed = DateTime.Now - startTime;
                    await ReplyAsync($"Успешно удалено {num} сообщений за {consumed.Minutes} м {consumed.Seconds} с.");
                }

                [Command("roles")]
                public async Task ShowAdministatorRoles()
                {
                    var sb = new StringBuilder();
                    sb.Append("Роли у которых есть права администратора: ");
                    foreach (var x in Context.Client.Guilds)
                    {
                        foreach (var i in x.Roles)
                        {
                            if (i.Permissions.Administrator) sb.Append(i.Name + ", ");
                        }
                    }

                    var result = sb.ToString();
                    await Context.Channel.SendMessageAsync(result.Remove(result.Length - 2));
                }
            }
        }
    }
}