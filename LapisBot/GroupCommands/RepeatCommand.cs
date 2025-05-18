using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Settings;

namespace LapisBot.GroupCommands;

public class RepeatCommand : GroupCommand
{
    public RepeatCommand()
    {
        CommandHead = new Regex("^repeat");
        DirectCommandHead = new Regex("^repeat");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("repeat", "1");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        SendMessage(source, new CqMessage
            { command });
    }
}