using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;

namespace Lapis.Commands;

public class UniversalCommand : Command
{
    public virtual void Parse(CqMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void ParseWithArgument(string[] arguments, CqMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqMessagePostContext source)
    {
    }
}