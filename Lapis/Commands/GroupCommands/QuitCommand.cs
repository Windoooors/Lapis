using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands;

public class QuitCommand : GroupCommand
{
    public QuitCommand()
    {
        CommandHead = "quit";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("quit", "1");
        IntendedArgumentCount = 1;
    }

    public override void Parse(string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (source.Sender.Role is not (CqRole.Admin or CqRole.Owner) &&
            source.Sender.UserId != BotConfiguration.Instance.AdministratorQqNumber)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "您无权执行该命令"]);
            return;
        }

        SendMessage(source, [new CqReplyMsg(source.MessageId), $"{BotConfiguration.Instance.BotName} 将退出群聊，感谢大家的使用！"]);
        Program.Session.LeaveGroup(source.GroupId);
    }
}