using System;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ImageOperation;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.StickerCommands;

public class ObituaryCommand : StickerCommandBase
{
    private readonly Regex _fontSizeCommand = new(@"^-s\s(([0-7][0-2]\s)|([0-9]\s)|([0-6][0-9]\s))");
    private readonly Regex _topCommand = new(@"^-t\s(([0-2][0-9][0-9]\s)|([0-9][0-9]\s)|([0-9]\s))");

    public ObituaryCommand()
    {
        CommandHead = new Regex("^悲报");
        DirectCommandHead = new Regex("^悲报");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("obituary", "1");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        if (command != string.Empty)
        {
            var image = new Image(Environment.CurrentDirectory + "/resource/stickers/beibao.png");
            var fontSize = 36;
            var top = 200;

            if (_fontSizeCommand.IsMatch(command))
            {
                fontSize = int.Parse(_fontSizeCommand.Match(command).ToString().Substring(3));
                command = _fontSizeCommand.Replace(command, string.Empty, 1);
                if (_topCommand.IsMatch(command))
                {
                    top = int.Parse(_topCommand.Match(command).ToString().Substring(3));
                    command = _topCommand.Replace(command, string.Empty, 1);
                }
            }
            else
            {
                if (_topCommand.IsMatch(command))
                {
                    top = int.Parse(_topCommand.Match(command).ToString().Substring(3));
                    command = _topCommand.Replace(command, string.Empty, 1);
                }
            }

            if (_topCommand.IsMatch(command))
            {
                top = int.Parse(_topCommand.Match(command).ToString().Substring(3));
                command = _topCommand.Replace(command, string.Empty, 1);
                if (_fontSizeCommand.IsMatch(command))
                {
                    fontSize = int.Parse(_fontSizeCommand.Match(command).ToString().Substring(3));
                    command = _fontSizeCommand.Replace(command, string.Empty, 1);
                }
            }
            else
            {
                if (_fontSizeCommand.IsMatch(command))
                {
                    fontSize = int.Parse(_fontSizeCommand.Match(command).ToString().Substring(3));
                    command = _fontSizeCommand.Replace(command, string.Empty, 1);
                }
            }

            image.DrawText(command, new Color(0.3f, 0.3f, 0.3f, 1), fontSize, FontWeight.Regular,
                HorizontalAlignment.Center, 233, top);

            SendMessage(source, new CqMessage
                { new CqImageMsg("base64://" + image.ToBase64()) });
            image.Dispose();
            return;
        }

        HelpCommand.Instance.ArgumentErrorHelp(source);
    }
}