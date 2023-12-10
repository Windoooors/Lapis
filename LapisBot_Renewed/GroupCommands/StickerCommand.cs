using System;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LapisBot_Renewed
{
    public class StickerCommand : GroupCommand
    {

        public override Task Initialize()
        {
            subCommands.Clear();
            headCommand = new Regex(@"^sticker\s");
            defaultSettings.SettingsName = "表情包";
            _groupCommandSettings = defaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings");
                
            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }
            //MessageManager.SendGroupMessageAsync(source.GroupId, "傻逼");
            subCommands.Add(new FortuneCommand());

            foreach (StickerCommand stickerCommand in subCommands)
            {
                stickerCommand.Initialize();
                stickerCommand.parentCommand = this;
            }
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            foreach (StickerCommand subCommand in subCommands)
            {
                if (subCommand.headCommand.IsMatch(command) && subCommand.headCommand.Replace(command, "") != string.Empty)
                {
                    command = subCommand.headCommand.Replace(command, "");
                    subCommand.PreParse(command, source);
                    return Task.CompletedTask;
                }
            }
            Program.helpCommand.Parse("", source);
            return Task.CompletedTask;
        }
    }
}
