using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;

namespace Lapis.Commands;

public class GroupCommand : Command
{
    public virtual void Parse(CqGroupMessagePostContext source, long[] mentionedUserIds)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void ParseWithArgument(string command, CqGroupMessagePostContext source, long[] mentionedUserIds)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source,
        long[] mentionedUserIds)
    {
    }
}