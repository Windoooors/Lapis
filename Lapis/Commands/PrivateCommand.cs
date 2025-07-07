using EleCho.GoCqHttpSdk.Post;

namespace Lapis.Commands;

public class PrivateCommand : Command
{
    public virtual void Parse(CqPrivateMessagePostContext source)
    {
    }

    public virtual void ParseWithArgument(string[] arguments, CqPrivateMessagePostContext source)
    {
    }

    public virtual void RespondWithoutParsingCommand(string command, CqPrivateMessagePostContext source)
    {
    }
}