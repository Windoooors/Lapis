using EleCho.GoCqHttpSdk.Post;
using LapisBot.UniversalCommands;

namespace LapisBot;

public class UniversalCommand : Command
{
    public virtual void Parse(CqMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void ParseWithArgument(string command, CqMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqMessagePostContext source)
    {
    }
}