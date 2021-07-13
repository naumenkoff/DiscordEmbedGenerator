using Discord.Commands;

namespace DiscordEmbedGenerator
{
    public class CustomExecutionResult : RuntimeResult
    {
        public CustomExecutionResult(CommandError? error, string reason) : base(error, reason)
        {
        }

        public static CustomExecutionResult FromError(string reason)
        {
            return new(CommandError.Unsuccessful, reason);
        }

        public static CustomExecutionResult FromSuccess(string reason = null)
        {
            return new(null, reason);
        }
    }
}