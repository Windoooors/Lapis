using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Sessions.Http.Managers;
using System.IO;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class GoMadCommand : GroupCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^色色\s");
            DirectCommand = new Regex(@"^色色\s");
            DefaultSettings.SettingsName = "色色";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");

            }

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
            var text = command.Replace("！", "\u2661").Replace("!", "\u2661").Replace("，", "……")
                .Replace(",", "……").Replace("；", "……").Replace(";", "……");
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new PlainMessage(text) });
          return Task.CompletedTask;
        }
    }
}
