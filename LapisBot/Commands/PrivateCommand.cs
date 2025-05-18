using EleCho.GoCqHttpSdk.Post;

namespace LapisBot.Commands;

public class PrivateCommand : Command
{
    public virtual void Parse(CqPrivateMessagePostContext source)
    {
    }

    public virtual void ParseWithArgument(string command, CqPrivateMessagePostContext source)
    {
    }

    public virtual void RespondWithoutParsingCommand(string command, CqPrivateMessagePostContext source)
    {
    }
}