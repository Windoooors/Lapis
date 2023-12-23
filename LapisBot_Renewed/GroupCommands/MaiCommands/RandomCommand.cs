using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using ImageMagick;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages;
using Manganese.Text;
using static LapisBot_Renewed.GroupCommand;

namespace LapisBot_Renewed
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
            _groupCommandSettings = ((RandomSettings)defaultSettings).Clone((RandomSettings)defaultSettings);
            return Task.CompletedTask;
        }
        //public new RandomSettings _groupCommandSettings;
        //public new RandomSettings defaultSettings;
        public override Task Initialize()
        {
            headCommand = new Regex(@"^random\s");
            directCommand = new Regex(@"^random\s|^随个\s|^随个");
            defaultSettings = new RandomSettings
            {
                Enabled = true,
                SongPreview = true,
                DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" }, { "SongPreview", "歌曲试听" } },
                SettingsName = "随机歌曲"
            };
            _groupCommandSettings = defaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings");
                
            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<RandomSettings>(settingsString));
            }
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            int i;
            if (!levelDictionary.ContainsKey(command))
                MessageManager.SendGroupMessageAsync(source.GroupId, "你随牛魔酬宾");
            else
            {
                levelDictionary.TryGetValue(command, out i);
                if (i == levelDictionary.Count - 1)
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId, "潘你妈");
                }
                if (i == 6)
                {
                    return Task.CompletedTask;
                }
                SongDto[] _songs = levels[i].ToArray();

                Random random = new Random();
                int j = random.Next(0, _songs.Length);

                Program.settingsCommand.GetSettings(source);
                var _image = new ImageMessage
                {
                    Base64 = InfoImageGenerator.Generate(j, _songs, "随机歌曲", null, Program.settingsCommand.CurrentBotSettings.CompressedImage)
                };

                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
                if (((RandomSettings)_groupCommandSettings).SongPreview)
                {
                    var _voice = new VoiceMessage
                    {
                        Path = SongToVoiceConverter.Convert(_songs[j].Id)
                    };
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { _voice });
                }
            }
            return Task.CompletedTask;
        }
    }
}
