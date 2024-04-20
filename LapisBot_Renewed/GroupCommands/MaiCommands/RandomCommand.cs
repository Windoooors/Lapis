using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages;
using static LapisBot_Renewed.GroupCommand;
using LapisBot_Renewed.ImageGenerators;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{

    public class RandomSettings : GroupCommandSettings
    {
        public bool SongPreview { get; set; }

        public RandomSettings Clone(RandomSettings randomSettings)
        {
            return JsonConvert.DeserializeObject<RandomSettings>(JsonConvert.SerializeObject(randomSettings));
        }
    }

    public class RandomCommand : MaiCommand
    {
        public override Task GetDefaultSettings()
        {
            CurrentGroupCommandSettings = ((RandomSettings)DefaultSettings).Clone((RandomSettings)DefaultSettings);
            return Task.CompletedTask;
        }
        //public new RandomSettings _groupCommandSettings;
        //public new RandomSettings defaultSettings;
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^random\s");
            DirectCommand = new Regex(@"^random\s|^随个\s|^随个");
            DefaultSettings = new RandomSettings
            {
                Enabled = true,
                SongPreview = false,
                DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" }, { "SongPreview", "歌曲试听" } },
                SettingsName = "随机歌曲"
            };
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings");
                
            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<RandomSettings>(settingsString));
            }
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            int i;
            if (!MaiCommandCommand.LevelDictionary.ContainsKey(command))
                MessageManager.SendGroupMessageAsync(source.GroupId, "不支持的等级名称");
            else
            {
                MaiCommandCommand.LevelDictionary.TryGetValue(command, out i);
                if (i == MaiCommandCommand.LevelDictionary.Count - 1)
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId, "潘你妈");
                }
                SongDto[] songs = MaiCommandCommand.Levels[i].ToArray();

                Random random = new Random();
                int j = random.Next(0, songs.Length);

                Program.settingsCommand.GetSettings(source);
                var image = new ImageMessage
                {
                    Base64 = new InfoImageGenerator().Generate(songs[j], "随机歌曲", null, Program.settingsCommand.CurrentBotSettings.CompressedImage)
                };

                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), image });
                if (((RandomSettings)CurrentGroupCommandSettings).SongPreview)
                {
                    var voice = new VoiceMessage
                    {
                        Path = new AudioToVoiceConverter().ConvertSong(songs[j].Id)
                    };
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { voice });
                }
            }
            return Task.CompletedTask;
        }
    }
}
