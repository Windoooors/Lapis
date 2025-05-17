using EleCho.GoCqHttpSdk.Post;
using LapisBot.UniversalCommands;

namespace LapisBot;

public class GroupCommand : Command
{
    public virtual void Parse(CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
    }
}