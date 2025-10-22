using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.UniversalCommands;

public class HelpCommand : UniversalCommand
{
    public static HelpCommand Instance;

    public HelpCommand()
    {
        CommandHead = "help";
        DirectCommandHead = "help";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("help", "1");
        Instance = this;
    }

    public override void Parse(string originalPlainMessage, CqMessagePostContext source)
    {
        var message = new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqTextMsg($"请访问链接以查询 {BotConfiguration.Instance.BotName} 的使用方法：https://setchin.com/lapis/docs/")
        };

        SendMessage(source, message);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage, CqMessagePostContext source)
    {
    } // 空重写以防 "help me" 导致触发帮助的情况发生

    public void ErrorHelp(CqMessagePostContext source, string errorMessage)
    {
        var message = new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(errorMessage)
        };
        SendMessage(source, message);
    }

    public void ArgumentErrorHelp(CqMessagePostContext source)
    {
        var message = new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(
                $"参数错误\n请访问链接以查询 {BotConfiguration.Instance.BotName} 的使用方法：https://setchin.com/lapis/docs/")
        };
        SendMessage(source, message);
    }

    public void UnexpectedErrorHelp(CqMessagePostContext source)
    {
        var message = new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("出现了未知的错误")
        };
        SendMessage(source, message);
    }
}