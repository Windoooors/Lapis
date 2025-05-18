using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Operations.ImageOperation;
using LapisBot.Settings;

namespace LapisBot.Commands.GroupCommands;

public class AboutCommand : GroupCommand
{
    public AboutCommand()
    {
        CommandHead = new Regex("^about|^关于");
        DirectCommandHead = new Regex("^about|^关于");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("about", "1");
    }

    public override void Parse(CqGroupMessagePostContext source)
    {
        var image = new Image(Environment.CurrentDirectory + "/resource/about.png");
        image.DrawText(RuntimeInformation.OSDescription, Color.White, 22, FontWeight.Regular, 128.56f, 202.23f);
        image.DrawText(RuntimeInformation.FrameworkDescription, Color.White, 22, FontWeight.Regular, 128.56f, 231.67f);
        image.DrawText(RuntimeInformation.OSArchitecture.ToString(), Color.White, 22, FontWeight.Regular, 128.56f,
            262.48f);
        image.DrawText("Version " + Assembly.GetAssembly(GetType()).GetName().Version, Color.White, 22,
            FontWeight.Regular, 20.49f, 325f);
        SendMessage(source, new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + image.ToBase64())
        });
        image.Dispose();
    }
}