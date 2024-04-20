using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class AliasAddCommand : MaiCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^alias add\s");
            DirectCommand = new Regex(@"^添加别名\s");
            DefaultSettings.SettingsName = "添加别名";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            MessageManager.SendGroupMessageAsync(source.GroupId, "🥺");
            return Task.CompletedTask;
        }
    }
}