using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.ImageGenerators;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands;

public class SettingsCommand : GroupCommand
{
    public SettingsCommand()
    {
        CommandHead = "settings";
        DirectCommandHead = "settings";
        IntendedArgumentCount = 2;
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + new BotSettingsImageGenerator().Generate(source.GroupId,
                SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId)))
        ]);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (!(source.Sender.Role is CqRole.Admin or CqRole.Owner ||
              source.Sender.UserId == BotConfiguration.Instance.AdministratorQqNumber))
        {
            SendMessage(source,
                [new CqReplyMsg(source.MessageId), new CqTextMsg("您无权执行该指令")]);
            return;
        }

        if (arguments.Length < IntendedArgumentCount)
        {
            HelpCommand.Instance.ArgumentErrorHelp(source);
            return;
        }

        var operationRegex = new Regex("true$|false$");

        if (!operationRegex.IsMatch(arguments[1]))
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId), new CqTextMsg("参数输入有误")
            ]);
            return;
        }

        var identifiers = arguments[0].Split(".");

        var identifierPair = new SettingsIdentifierPair(identifiers[0], identifiers[1]);

        var valueString = arguments[1];
        var value = valueString.Equals("true");

        if (SettingsPool.SetValue(identifierPair, source.GroupId, value))
            SendMessage(source,
                [new CqReplyMsg(source.MessageId), new CqTextMsg("设置变更成功！")]);
        else
            SendMessage(source,
                [new CqReplyMsg(source.MessageId), new CqTextMsg("未找到设置项！")]);
    }
}