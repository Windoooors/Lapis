using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;

namespace Lapis.Commands;

public class UniversalCommand : Command
{
    public virtual void Parse(string originalPlainMessage, CqMessagePostContext source)
    {
        HelpCommand.Instance.Parse(originalPlainMessage, source);
    }

    public virtual void ParseWithArgument(string[] arguments, string originalPlainMessage, CqMessagePostContext source)
    {
        HelpCommand.Instance.Parse(originalPlainMessage, source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqMessagePostContext source)
    {
    }
}