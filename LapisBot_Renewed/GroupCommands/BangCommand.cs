using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class BangCommand : GroupCommand
    {
        
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^bang$");
            DirectCommand = new Regex(@"^bang$");
            DefaultSettings.SettingsName = "钢管落地";
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

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            //var path = AudioToVoiceConverter.ConvertAudio(AppContext.BaseDirectory + "resource/bang.mp3");
            
            //CqGroupMessagePostContext.SendGroupMessageAsync(source.GroupId, new MessageChain() { new VoiceMessage(){ Path = AppContext.BaseDirectory + "resource/bang.silk" } });
            return Task.CompletedTask;
        }
    }
}