using System;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Settings;

namespace LapisBot.Commands.GroupCommands;

public class AbuseCommand : GroupCommand
{
    private readonly string[] _dirtyWordsStrings =
    {
        "6", "杂鱼\u2661~杂鱼\u2661~", "你妈炸了你个傻逼", "我不骂傻逼", "操你妈了个逼，操你老婆逼，操你女儿逼，你全家 2025 年死光光，操你妈了个逼的", "就你这粉丝量想跟我撞？",
        "严重的怀疑你自以为是，不是所谓的新版你吃不吃史？", "人生自古谁无死？不幸地，Index已在于上浮的搏斗中去世，让我们永远缅怀"
    };

    public AbuseCommand()
    {
        CommandHead = new Regex("^骂我|^夸我");
        DirectCommandHead = new Regex("^骂我|^夸我");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("abuse", "1");
    }

    public override void Parse(CqGroupMessagePostContext source)
    {
        if (source.GroupId == 1078224429)
        {
            var i = new Random().Next(0, _dirtyWordsStrings.Length);
            SendMessage(source, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(_dirtyWordsStrings[i])
            });
        }
        else
        {
            var i = new Random().Next(0, 5);
            SendMessage(source, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(_dirtyWordsStrings[i])
            });
        }
    }
}