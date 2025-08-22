using EleCho.GoCqHttpSdk.Post;

namespace Lapis.Commands;

public class PrivateCommand : Command
{
    public virtual void Parse(string originalPlainMessage, CqPrivateMessagePostContext source)
    {
    }

    public virtual void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqPrivateMessagePostContext source)
    {
    }

    public virtual void RespondWithoutParsingCommand(string command, CqPrivateMessagePostContext source)
    {
    }
}