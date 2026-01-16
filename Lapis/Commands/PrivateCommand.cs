using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;

namespace Lapis.Commands;

public class PrivateCommand : Command
{
    public virtual void Parse(string originalPlainMessage, CqPrivateMessagePostContext source)
    {
        HelpCommand.Instance.Parse(originalPlainMessage, source);
    }

    public virtual void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqPrivateMessagePostContext source)
    {
        HelpCommand.Instance.Parse(originalPlainMessage, source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqPrivateMessagePostContext source)
    {
    }
}