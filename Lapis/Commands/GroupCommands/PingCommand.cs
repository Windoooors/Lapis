using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands;

public class PingCommand : GroupCommand
{
    public PingCommand()
    {
        CommandHead = "ping";
        DirectCommandHead = "ping";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("ping", "1");
        IntendedArgumentCount = 1;
    }

    public override void Parse(string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        SendMessage(source, [new CqReplyMsg(source.MessageId), $"Pong! {BotConfiguration.Instance.BotName} 喜欢你们喵！"]);
    }
}