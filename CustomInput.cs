using Discord.Commands;

namespace DiscordEmbedGenerator
{
    public class CustomInput
    {
        [NamedArgumentType]
        public class Embed
        {
            public string Color { get; set; }
            public string Author { get; set; }
            public string ThumbnailUrl { get; set; }
            public string Title { get; set; }
            public string TitleUrl { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string Footer { get; set; }
            public string Timestamp { get; set; }
            public string UpperTheme { get; set; }
            public string UpperMessage { get; set; }
        }
    }
}