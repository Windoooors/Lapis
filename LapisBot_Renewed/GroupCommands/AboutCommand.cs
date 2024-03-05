using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Sessions.Http.Managers;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using ImageMagick;
using System.Reflection;

namespace LapisBot_Renewed.GroupCommands
{
    public class AboutCommand : GroupCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^about$|^关于$");
            DirectCommand = new Regex(@"^about$|^关于$");
            DefaultSettings.SettingsName = "关于";
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

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            var image = new MagickImage(Environment.CurrentDirectory + @"/resource/about.png");
            new Drawables()
    .Font(Environment.CurrentDirectory + @"/resource/font.otf")
    .FontPointSize(22f)
    .FillColor(new MagickColor(65535, 65535, 65535, 65535))
    .Text(128.56, 202.23, RuntimeInformation.OSDescription)
    .Draw(image);
                new Drawables()
    .Font(Environment.CurrentDirectory + @"/resource/font.otf")
    .FontPointSize(22f)
    .FillColor(new MagickColor(65535, 65535, 65535, 65535))
    .Text(128.56, 231.67, RuntimeInformation.FrameworkDescription)
    .Draw(image);
                    new Drawables()
    .Font(Environment.CurrentDirectory + @"/resource/font.otf")
    .FontPointSize(22f)
    .FillColor(new MagickColor(65535, 65535, 65535, 65535))
    .Text(128.56, 262.48, RuntimeInformation.OSArchitecture.ToString())
    .Draw(image);
                        new Drawables()
    .Font(Environment.CurrentDirectory + @"/resource/font.otf")
    .FontPointSize(18f)
    .FillColor(new MagickColor(65535, 65535, 65535, 65535))
    .Text(20.49, 325, "Version " + Assembly.GetAssembly(GetType()).GetName().Version.ToString())
    .Draw(image);
            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new ImageMessage() { Base64 = image.ToBase64() } });
            return Task.CompletedTask;
        }
    }
}
