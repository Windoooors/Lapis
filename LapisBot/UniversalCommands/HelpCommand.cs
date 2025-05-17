using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Settings;

namespace LapisBot.UniversalCommands;

public class HelpCommand : UniversalCommand
{
    public static HelpCommand Instance;

    public HelpCommand()
    {
        CommandHead = new Regex("^help");
        DirectCommandHead = new Regex("^help");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("help", "1");
        Instance = this;
    }

    public override void Parse(CqMessagePostContext source)
    {
        var message = new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("请访问链接以查询 Lapis 的使用方法：https://www.setchin.com/lapis_docs.html")
        };

        SendMessage(source, message);
    }

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
            new CqTextMsg("参数错误\n请访问链接以查询 Lapis 的使用方法：https://www.setchin.com/lapis_docs.html")
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