using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
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

    public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            { command });
        return Task.CompletedTask;
    }
}