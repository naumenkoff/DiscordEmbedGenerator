using System;
using System.Collections.Generic;
using Discord;

namespace DiscordEmbedGenerator
{
    public class EmbedGenerator
    {
        public static Embed Generate(CustomInput.Embed embeds)
        {
            var embed = new EmbedBuilder();
            if (!string.IsNullOrEmpty(embeds.Color))
            {
                var colorStr = embeds.Color.Split(';', ',', ' ');
                var colors = new List<int>();
                foreach (var clr in colorStr)
                {
                    var parsed = int.TryParse(clr, out var color);
                    if (parsed && color is >= 0 and <= 255) colors.Add(color);
                }

                if (colors.Count == 3) embed.WithColor(colors[0], colors[1], colors[2]);
                //else
                //{
                //    await Context.Channel.SendMessageAsync(
                //        $"{Context.User.Mention}, вы ввели неверные аргументы для параметра \"color\".\nУбедитесь, правильно ли вы указали цвет в форме RGB(\"R;G;B\", 0 ≤ n ≤ 255) и попробуйте снова.");
                //    return;
                //}
            }

            embed.WithAuthor(embeds.Author);
            embed.WithThumbnailUrl(embeds.ThumbnailUrl);
            embed.WithTitle(embeds.Title);
            embed.WithUrl(embeds.TitleUrl);
            embed.WithDescription(embeds.Description);
            embed.WithImageUrl(embeds.ImageUrl);
            embed.WithFooter(embeds.Footer);
            embed.WithTimestamp(DateTime.Now);
            embed.AddField(embeds.UpperTheme, embeds.UpperMessage);

            return embed.Build();
        }
    }
}