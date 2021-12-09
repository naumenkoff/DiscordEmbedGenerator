using Discord.Commands;

namespace DiscordEmbedGenerator
{
    public class MyCustomResult : RuntimeResult
    {
        public MyCustomResult(CommandError? error, string reason) : base(error, reason)
        {
        }

        public static MyCustomResult FromError(string reason)
        {
            return new MyCustomResult(CommandError.Unsuccessful, reason);
        }

        public static MyCustomResult FromSuccess(string reason = null)
        {
            return new MyCustomResult(null, reason);
        }
    }
}