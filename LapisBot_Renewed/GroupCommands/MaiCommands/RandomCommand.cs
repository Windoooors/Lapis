using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
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
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");

            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<RandomSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            int i;
            if (!Instance.LevelDictionary.ContainsKey(command))
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqTextMsg("不支持的等级名称") });
            else
            {
                Instance.LevelDictionary.TryGetValue(command, out i);
                SongDto[] songs = Instance.Levels[i].ToArray();
                if (songs.Length == 0)
                {

                    Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                        { new CqTextMsg("不支持的等级名称") });
                    return Task.CompletedTask;
                }

                Random random = new Random();
                int j = random.Next(0, songs.Length);

                Program.SettingsCommand.GetSettings(source);
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqImageMsg("base64://" + new InfoImageGenerator().Generate(songs[j], "随机歌曲", null,
                            Program.SettingsCommand.CurrentBotSettings.CompressedImage))
                    });
                if (((RandomSettings)CurrentGroupCommandSettings).SongPreview)
                {
                    var voice = new CqRecordMsg("file:///" + new AudioToVoiceConverter().GetSongPath(songs[j].Id));
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        new CqMessage
                        {
                            voice
                        });
                }
            }

            return Task.CompletedTask;
        }
    }
}
