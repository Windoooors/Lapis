using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class RepeatCommand : GroupCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^repeat\s");
            DirectCommand = new Regex(@"^repeat\s");
            DefaultSettings.SettingsName = "重复";
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
            Program.Session.SendGroupMessageAsync(source.GroupId, [command]);
            return Task.CompletedTask;
        }
    }
}
