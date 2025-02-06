using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageMagick;
using System.IO;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using ImageMagick.Drawing;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands.StickerCommands
{
    public class FortuneCommand : StickerCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^喜报\s");
            DirectCommand = new Regex(@"^喜报\s");
            DefaultSettings.SettingsName = "喜报";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings");
            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }
            return Task.CompletedTask;
        }

        private Regex fontSizeCommand = new Regex(@"^-s\s(([0-7][0-2]\s)|([0-9]\s)|([0-6][0-9]\s))");
        private Regex topCommand = new Regex(@"^-t\s(([0-2][0-9][0-9]\s)|([0-9][0-9]\s)|([0-9]\s))");

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            if (command != string.Empty)
            {
                var image = new MagickImage(Environment.CurrentDirectory + @"/resource/stickers/xibao.png");
                var fontSize = 72;
                var top = 200;

                if (fontSizeCommand.IsMatch(command))
                {
                    fontSize = int.Parse(fontSizeCommand.Match(command).ToString().Substring(3));
                    command = fontSizeCommand.Replace(command, string.Empty);
                    if (topCommand.IsMatch(command))
                    {
                        top = int.Parse(topCommand.Match(command).ToString().Substring(3));
                        command = topCommand.Replace(command, string.Empty);
                    }
                }
                else
                {
                    if (topCommand.IsMatch(command))
                    {
                        top = int.Parse(topCommand.Match(command).ToString().Substring(3));
                        command = topCommand.Replace(command, string.Empty);
                    }
                }
                if (topCommand.IsMatch(command))
                {
                    top = int.Parse(topCommand.Match(command).ToString().Substring(3));
                    command = topCommand.Replace(command, string.Empty);
                    if (fontSizeCommand.IsMatch(command))
                    {
                        fontSize = int.Parse(fontSizeCommand.Match(command).ToString().Substring(3));
                        command = fontSizeCommand.Replace(command, string.Empty);
                    }
                }
                else
                {
                    if (fontSizeCommand.IsMatch(command))
                    {
                        fontSize = int.Parse(fontSizeCommand.Match(command).ToString().Substring(3));
                        command = fontSizeCommand.Replace(command, string.Empty);
                    }
                }
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                    //.Font(Environment.CurrentDirectory + @"/resources/emoji.ttc")
                    .TextAlignment(TextAlignment.Center)
                    .FontPointSize(fontSize)
                    .FillColor(new MagickColor(65535, 0, 0, 65535))
                    .Text(233, top, command)
                    .Draw(image);
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqImageMsg("base64://" + image.ToBase64()) });
                image.Dispose();
                return Task.CompletedTask;
            }
            else
            {
                Program.helpCommand.Parse(command, source);
                return Task.CompletedTask;
            }
        }
    }
}