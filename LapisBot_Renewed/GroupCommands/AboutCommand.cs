using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Reflection;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Operations.ImageOperation;

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

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            var image = new Image(Environment.CurrentDirectory + @"/resource/about.png");
            image.DrawText(RuntimeInformation.OSDescription, Color.White, 22, FontWeight.Regular, 128.56f, 202.23f);
            image.DrawText(RuntimeInformation.FrameworkDescription, Color.White, 22, FontWeight.Regular, 128.56f, 231.67f);
            image.DrawText(RuntimeInformation.OSArchitecture.ToString(), Color.White, 22, FontWeight.Regular, 128.56f, 262.48f);
            image.DrawText("Version " + Assembly.GetAssembly(GetType()).GetName().Version, Color.White, 22, FontWeight.Regular, 20.49f, 325f);
            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqImageMsg("base64://" + image.ToBase64())
            });
            image.Dispose();
            return Task.CompletedTask;
        }
    }
}