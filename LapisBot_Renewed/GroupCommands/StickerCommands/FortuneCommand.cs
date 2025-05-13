using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Operations.ImageOperation;
using LapisBot_Renewed.Settings;

namespace LapisBot_Renewed.GroupCommands.StickerCommands;

public class FortuneCommand : StickerCommandBase
{
    public FortuneCommand()
    {
        CommandHead = new Regex("^喜报");
        DirectCommandHead = new Regex("^喜报");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("fortune", "1");
    }

    private readonly Regex _fontSizeCommand = new (@"^-s\s(([0-7][0-2]\s)|([0-9]\s)|([0-6][0-9]\s))");
    private readonly Regex _topCommand = new (@"^-t\s(([0-2][0-9][0-9]\s)|([0-9][0-9]\s)|([0-9]\s))");

    public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        if (command != string.Empty)
        {
            var image = new Image(Environment.CurrentDirectory + @"/resource/stickers/xibao.png");
            var fontSize = 36;
            var top = 200;

            if (_fontSizeCommand.IsMatch(command))
            {
                fontSize = int.Parse(_fontSizeCommand.Match(command).ToString().Substring(3));
                command = _fontSizeCommand.Replace(command, string.Empty);
                if (_topCommand.IsMatch(command))
                {
                    top = int.Parse(_topCommand.Match(command).ToString().Substring(3));
                    command = _topCommand.Replace(command, string.Empty);
                }
            }
            else
            {
                if (_topCommand.IsMatch(command))
                {
                    top = int.Parse(_topCommand.Match(command).ToString().Substring(3));
                    command = _topCommand.Replace(command, string.Empty);
                }
            }

            if (_topCommand.IsMatch(command))
            {
                top = int.Parse(_topCommand.Match(command).ToString().Substring(3));
                command = _topCommand.Replace(command, string.Empty);
                if (_fontSizeCommand.IsMatch(command))
                {
                    fontSize = int.Parse(_fontSizeCommand.Match(command).ToString().Substring(3));
                    command = _fontSizeCommand.Replace(command, string.Empty);
                }
            }
            else
            {
                if (_fontSizeCommand.IsMatch(command))
                {
                    fontSize = int.Parse(_fontSizeCommand.Match(command).ToString().Substring(3));
                    command = _fontSizeCommand.Replace(command, string.Empty);
                }
            }

            image.DrawText(command, new Color(0.6f, 0, 0, 1), fontSize, FontWeight.Regular,
                HorizontalAlignment.Center, 233, top);

            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                { new CqImageMsg("base64://" + image.ToBase64()) });
            image.Dispose();
            return Task.CompletedTask;
        }

        HelpCommand.Instance.ArgumentErrorHelp(source);
        return Task.CompletedTask;

    }
}
