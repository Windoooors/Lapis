using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;
using LapisBot_Renewed.Operations.ImageOperation;

namespace LapisBot_Renewed.GroupCommands.StickerCommands
{
    public class ObituaryCommand : StickerCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^悲报\s");
            DirectCommand = new Regex(@"^悲报\s");
            DefaultSettings.SettingsName = "悲报";
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
                var image = new Image(Environment.CurrentDirectory + @"/resource/stickers/beibao.png");
                var fontSize = 36;
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

                image.DrawText(command, new Color(0.3f, 0.3f, 0.3f, 1), fontSize, FontWeight.Regular,
                    HorizontalAlignment.Center, 233, top);

                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqImageMsg("base64://" + image.ToBase64()) });
                image.Dispose();
                return Task.CompletedTask;
            }
            else
            {
                Program.HelpCommand.Parse(command, source);
                return Task.CompletedTask;
            }
        }
    }
}