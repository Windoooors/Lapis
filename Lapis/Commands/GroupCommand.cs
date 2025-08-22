using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;

namespace Lapis.Commands;

public class GroupCommand : Command
{
    public virtual void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(originalPlainMessage, source);
    }

    public virtual void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(originalPlainMessage, source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
    }
}