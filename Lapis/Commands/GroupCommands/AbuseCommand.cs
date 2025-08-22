using System;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands;

public class AbuseCommand : GroupCommand
{
    private readonly string[] _dirtyWordsStrings =
    {
        "6", "杂鱼\u2661~杂鱼\u2661~", "我不骂傻逼"
    };

    public AbuseCommand()
    {
        CommandHead = "骂我|夸我";
        DirectCommandHead = "骂我|夸我";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("abuse", "1");
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        var i = new Random().Next(0, _dirtyWordsStrings.Length + 1);
        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(_dirtyWordsStrings[i])
        ]);
    }
}