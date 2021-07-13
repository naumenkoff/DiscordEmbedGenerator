using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordEmbedGenerator
{
    public class CommandHandler
    {
        private const string EmbedKeyWords =
            "color: author: thumbnailUrl: title: titleUrl: description: fieldName: fieldValue: imageUrl: footer: timestamp:";
        private static readonly string[] SplittedEmbedKeyWords = EmbedKeyWords.Split(' ').ToArray();
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
            // TODO 1. Все методы someEmbed.With*<param>(args); имеют несколько перегрузок. Сделать возможным заполнение всех перегрузок,
            // TODO отловить эксепшны, сделать кастомные эксепшены что бы не пользоваться ретюрном, -_-.
            // TODO 2. Сделать возможным генерацию эмбеда с цветом как RGB, так и HEX.
            if (command.Value.Name is "embed")
                try
                {
                    if (context.Message.Content.Trim(' ') is "!embed")
                    {
                        await context.Channel.SendMessageAsync(
                            "Подробная работа бота описана на моем github: https://github.com/naumenkoff?tab=repositories");
                        return;
                    }

                    var messageContent = context.Message.Content;
                    var keyWordMatches = SplittedEmbedKeyWords.Count(keyWord => messageContent.Contains(keyWord));
                    var dictionary = new Dictionary<string, string>();
                    var matchesKeyWordIndexes = new List<int>();
                    matchesKeyWordIndexes.AddRange(
                        from keyWord in SplittedEmbedKeyWords
                        where messageContent.Contains(keyWord)
                        select messageContent.IndexOf(keyWord, StringComparison.Ordinal));
                    matchesKeyWordIndexes.Sort();
                    if (keyWordMatches > 0)
                    {
                        for (var i = 0; i < matchesKeyWordIndexes.Count; i++)
                        {
                            var message = i == matchesKeyWordIndexes.Count - 1
                                ? messageContent.Substring(matchesKeyWordIndexes[i])
                                : messageContent.Substring(matchesKeyWordIndexes[i],
                                    matchesKeyWordIndexes[i + 1] - matchesKeyWordIndexes[i]);
                            foreach (var keyWord in SplittedEmbedKeyWords)
                            {
                                if (!message.StartsWith(keyWord)) continue;
                                message = message.Remove(0, keyWord.Length).TrimStart().TrimEnd();
                                if (message.StartsWith('"')) message = message.Substring(1);
                                if (message.EndsWith('"')) message = message.Remove(message.Length - 1);
                                dictionary.Add(keyWord, message);
                            }
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(
                            "Подробная работа бота описана на моем github: https://github.com/naumenkoff?tab=repositories");
                        return;
                    }

                    var embed = new EmbedBuilder();

                    // color
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[0]))
                    {
                        var colorStr = dictionary[SplittedEmbedKeyWords[0]].Split(';', ',', ' ');
                        var colors = new List<int>();
                        foreach (var clr in colorStr)
                        {
                            var parsed = int.TryParse(clr, out var color);
                            if (parsed && color is >= 0 and <= 255) colors.Add(color);
                        }

                        if (colors.Count == 3)
                        {
                            embed.WithColor(colors[0], colors[1], colors[2]);
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync(
                                $"{context.User.Mention}, вы ввели неверные аргументы для параметра \"color\".\nУбедитесь, правильно ли вы указали цвет в форме RGB(\"R;G;B\", 0 ≤ n ≤ 255) и попробуйте снова.");
                            return;
                        }
                    }

                    // author
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[1]))
                        embed.WithAuthor(dictionary[SplittedEmbedKeyWords[1]]);

                    // thumbnailUrl
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[2]))
                        embed.WithThumbnailUrl(dictionary[SplittedEmbedKeyWords[2]]);

                    // title
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[3]))
                        embed.WithTitle(dictionary[SplittedEmbedKeyWords[3]]);

                    // titleUrl
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[4]))
                        embed.WithUrl(dictionary[SplittedEmbedKeyWords[4]]);

                    // description
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[5]))
                        embed.WithDescription(dictionary[SplittedEmbedKeyWords[5]]);

                    // fieldName
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[6]) &&
                        dictionary.ContainsKey(SplittedEmbedKeyWords[7]))
                        embed.AddField(dictionary[SplittedEmbedKeyWords[6]], dictionary[SplittedEmbedKeyWords[7]]);

                    // imageUrl
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[8]))
                        embed.WithImageUrl(dictionary[SplittedEmbedKeyWords[8]]);

                    // footer
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[9]))
                        embed.WithFooter(dictionary[SplittedEmbedKeyWords[9]]);
                    // timestamp
                    if (dictionary.ContainsKey(SplittedEmbedKeyWords[10])) embed.WithTimestamp(DateTime.Now);

                    //await context.Channel.DeleteMessageAsync(context.Message);
                    await context.Channel.SendMessageAsync(string.Empty, false, embed.Build());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            var commandName = command.IsSpecified ? command.Value.Name : "A command";
            await Log(new LogMessage(LogSeverity.Info,
                "CommandExecution", $"Command !{commandName} was executed by {context.User}"));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public class MainModule : ModuleBase<SocketCommandContext>
        {
            [Command("embed")]
            public async Task GenerateEmbed()
            {
                await Task.Run(() => { });
            }

            [Command("clearchat", RunMode = RunMode.Async)]
            [RequireUserPermission(GuildPermission.Administrator)]
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
                    await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(lastMessages);
                }
                else
                {
                    await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(
                        lastMessages.Take(twoWeeksMessage));
                    messages = await Context.Channel.GetMessagesAsync(num + 1 - twoWeeksMessage).FlattenAsync();
                    foreach (var msg in messages)
                        if (msg != null)
                            await msg.DeleteAsync();
                }

                var consumed = DateTime.Now - startTime;
                await ReplyAsync($"Успешно удалено {num} сообщений за {consumed.Minutes} м {consumed.Seconds} с.");
            }
        }
    }
}