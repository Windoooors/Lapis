using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands;

public class RepeatCommand : GroupCommand
{
    public RepeatCommand()
    {
        CommandHead = "repeat";
        DirectCommandHead = "repeat";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("repeat", "1");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source, long[] mentionedUserIds)
    {
        SendMessage(source, new CqMessage
            { command });
    }
}