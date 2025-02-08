using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using LapisBot_Renewed.GroupCommands.StickerCommands;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class StickerCommand : GroupCommand
    {

        public override Task Initialize()
        {
            SubCommands.Clear();
            HeadCommand = new Regex(@"^sticker\s");
            DefaultSettings.SettingsName = "表情包";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings");

            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                                       " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            //MessageManager.SendGroupMessageAsync(source.GroupId, "傻逼");
            SubCommands.Add(new FortuneCommand());

            foreach (StickerCommand stickerCommand in SubCommands)
            {
                stickerCommand.Initialize();
                stickerCommand.ParentCommand = this;
            }

            return Task.CompletedTask;
        }
    }
}