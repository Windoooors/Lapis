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
using Manganese.Text;
using static LapisBot_Renewed.GroupCommand;
using LapisBot_Renewed.ImageGenerators;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{

    public class InfoSettings : GroupCommandSettings
    {
        public bool SongPreview { get; set; }

        public InfoSettings Clone(InfoSettings infoSettings)
        {
            return JsonConvert.DeserializeObject<InfoSettings>(JsonConvert.SerializeObject(infoSettings));
        }
    }

    public class InfoCommand : MaiCommand
    {
        //public new InfoSettings CurrentGroupCommandSettings;
        //public new InfoSettings DefaultSettings;

        public override Task GetDefaultSettings()
        {
            CurrentGroupCommandSettings = ((InfoSettings)DefaultSettings).Clone((InfoSettings)DefaultSettings);
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^info\s");
            DirectCommand = new Regex(@"^info\s");
            DefaultSettings = new InfoSettings
            {
                Enabled = true,
                SongPreview = true,
                DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" }, { "SongPreview", "歌曲试听" } },
                SettingsName = "歌曲信息"
            };
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings");
            }

            foreach (var path in Directory.GetFiles(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                                       " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<InfoSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        public enum Rate
        {
            D,
            C,
            B,
            Bb,
            Bbb,
            A,
            Aa,
            Aaa,
            S,
            Sp,
            Ss,
            Ssp,
            Sss,
            Sssp
        }

        public enum FCState
        {
            None,
            Fc,
            Fcp
        }

        public enum FSState
        {
            None,
            FS,
            FSd
        }

        public class GetScoreDto
        {
            public class Level
            {
                public Rate Rate;
                public double Achievement;
                public int LevelIndex;
                public FCState Fc;
                public FSState Fs;
            }

            public Level[] Levels;

            public void Get(string name, string version, int id)
            {
                //try
                //{
                var content = Program.apiOperator.Post("api/maimaidxprober/query/plate",
                    new { username = name, version = new string[] { version } });
                ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                List<Level> levelList = new List<Level>();
                foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                {
                    if (score.Id == id)
                    {
                        var rate = new Rate();

                        var achievement = score.Achievements;
                        if (achievement >= 100.5)
                            rate = Rate.Sssp;
                        else if (100.5 > achievement && achievement >= 100)
                            rate = Rate.Sss;
                        else if (100 > achievement && achievement >= 99.5)
                            rate = Rate.Ssp;
                        else if (99.5 > achievement && achievement >= 99)
                            rate = Rate.Ss;
                        else if (99 > achievement && achievement >= 98)
                            rate = Rate.Sp;
                        else if (98 > achievement && achievement >= 97)
                            rate = Rate.S;
                        else if (97 > achievement && achievement >= 94)
                            rate = Rate.Aaa;
                        else if (94 > achievement && achievement >= 90)
                            rate = Rate.Aa;
                        else if (90 > achievement && achievement >= 80)
                            rate = Rate.A;
                        else if (80 > achievement && achievement >= 75)
                            rate = Rate.Bbb;
                        else if (75 > achievement && achievement >= 70)
                            rate = Rate.Bb;
                        else if (70 > achievement && achievement >= 60)
                            rate = Rate.B;
                        else if (60 > achievement && achievement >= 50)
                            rate = Rate.C;
                        else if (50 > achievement)
                            rate = Rate.D;

                        var fc = new FCState();
                        var fs = new FSState();

                        if (score.Fc == "fc")
                            fc = FCState.Fc;
                        else if (score.Fc == "fcp")
                            fc = FCState.Fcp;
                        else if (score.Fc == "")
                            fc = FCState.None;
                        /*
                        if (score.Fsd == "fs")
                            fs = FSState.FS;
                        else if (score.Fsd == "fsd")
                            fs = FSState.FSd;
                        else if (score.Fsd == "")
                            fs = FSState.None;
                        */
                        levelList.Add(new Level()
                        {
                            Achievement = score.Achievements, Rate = rate, LevelIndex = score.LevelIndex, Fc = fc,
                            Fs = fs
                        });

                    }
                }

                Levels = levelList.ToArray();
                //}
                /*catch
                {
                    List<Level> levelList = new List<Level>();
                    levels = levelList.ToArray();
                }*/
            }

            public void Get(long number, string version, int id)
            {
                try
                {
                    var content = Program.apiOperator.Post("api/maimaidxprober/query/plate",
                        new { qq = number, version = new string[] { version } });
                    ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                    List<Level> levelList = new List<Level>();
                    foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                    {
                        if (score.Id == id)
                        {
                            var rate = new Rate();

                            var achievement = score.Achievements;
                            if (achievement >= 100.5)
                                rate = Rate.Sssp;
                            else if (100.5 > achievement && achievement >= 100)
                                rate = Rate.Sss;
                            else if (100 > achievement && achievement >= 99.5)
                                rate = Rate.Ssp;
                            else if (99.5 > achievement && achievement >= 99)
                                rate = Rate.Ss;
                            else if (99 > achievement && achievement >= 98)
                                rate = Rate.Sp;
                            else if (98 > achievement && achievement >= 97)
                                rate = Rate.S;
                            else if (97 > achievement && achievement >= 94)
                                rate = Rate.Aaa;
                            else if (94 > achievement && achievement >= 90)
                                rate = Rate.Aa;
                            else if (90 > achievement && achievement >= 80)
                                rate = Rate.A;
                            else if (80 > achievement && achievement >= 75)
                                rate = Rate.Bbb;
                            else if (75 > achievement && achievement >= 70)
                                rate = Rate.Bb;
                            else if (70 > achievement && achievement >= 60)
                                rate = Rate.B;
                            else if (60 > achievement && achievement >= 50)
                                rate = Rate.C;
                            else if (50 > achievement)
                                rate = Rate.D;

                            var fc = new FCState();
                            var fs = new FSState();

                            if (score.Fc == "fc")
                                fc = FCState.Fc;
                            else if (score.Fc == "fcp")
                                fc = FCState.Fcp;
                            else if (score.Fc == "")
                                fc = FCState.None;
                            /*
                            if (score.Fsd == "fs")
                                fs = FSState.FS;
                            else if (score.Fsd == "fsd")
                                fs = FSState.FSd;
                            else if (score.Fsd == "")
                                fs = FSState.None;
                            */
                            levelList.Add(new Level()
                            {
                                Achievement = score.Achievements, Rate = rate, LevelIndex = score.LevelIndex, Fc = fc,
                                Fs = fs
                            });

                        }
                    }

                    Levels = levelList.ToArray();
                }
                catch
                {
                    List<Level> _levelList = new List<Level>();
                    Levels = _levelList.ToArray();
                }
            }

            public class ScoresDto
            {
                [JsonProperty("verlist")] public ScoreDto[] ScoreDtos;

                public class ScoreDto
                {
                    [JsonProperty("achievements")] public float Achievements;

                    [JsonProperty("fc")] public string Fc;

                    [JsonProperty("fsd")] public string Fsd;

                    [JsonProperty("Id")] public int Id;

                    [JsonProperty("level_index")] public int LevelIndex;
                }
            }

            public static GetScoreDto GetScore = new GetScoreDto();
        }

        public Task ParseWithArgument(string command, GroupMessageReceiver source)
        {
            var aliases = MaiCommandCommand.GetAliasByAliasStringUsingStartsWith(command);
            if (aliases.Length != 0)
            {
                if (aliases.Length == 1)
                {
                    try
                    {
                        var i = MaiCommandCommand.GetSongIndexById(aliases[0].Id);
                        //var name = command.Replace(MaiCommandCommand.GetAliasStringUsingStartsWith(command) + " ", string.Empty);
                        GetScoreDto.GetScore.Get(
                            command.Replace(MaiCommandCommand.GetAliasStringUsingStartsWith(command) + " ", string.Empty),
                            Songs[i].BasicInfo.Version, Songs[i].Id);
                        Program.settingsCommand.GetSettings(source);
                        var image = new ImageMessage
                        {
                            Base64 = InfoImageGenerator.Generate(i, Songs, "歌曲信息", GetScoreDto.GetScore.Levels,
                                Program.settingsCommand.CurrentBotSettings.CompressedImage)
                        };
                        MessageManager.SendGroupMessageAsync(source.GroupId,
                            new MessageChain() { new AtMessage(source.Sender.Id), image });

                        if (((InfoSettings)CurrentGroupCommandSettings).SongPreview)
                        {
                            var voice = new VoiceMessage
                            {
                                Path = AudioToVoiceConverter.ConvertSong(aliases[0].Id)
                            };
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { voice });
                        }
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId,
                            new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该玩家") });
                    }
                }
                else
                {
                    string ids = string.Empty;
                    List<int> idsList = new List<int>();
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        if (idsList.Contains(aliases[i].Id))
                            continue;
                        int subIndex = MaiCommandCommand.GetSongIndexById(aliases[i].Id);
                        ids += "ID " + aliases[i].Id + " - " + MaiCommandCommand.Songs[subIndex].Title + " [" +
                               MaiCommandCommand.Songs[subIndex].Type + "]";
                        idsList.Add(aliases[i].Id);
                        if (i != aliases.Length - 1)
                            ids += "\n";
                    }

                    if (idsList.Count == 1)
                    {
                        Parse(
                            "ID " + idsList[0] + command.Replace(MaiCommandCommand.GetAliasStringUsingStartsWith(command),
                                string.Empty), source);
                        return Task.CompletedTask;
                    }

                    int index = MaiCommandCommand.GetSongIndexById(idsList[0]);
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain()
                    {
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(" 该别称有多首歌曲匹配：\n" + ids + "\n*使用 \"lps mai info ID " + idsList[0] + " " +
                                         command.Replace(MaiCommandCommand.GetAliasStringUsingStartsWith(command) + " ",
                                             string.Empty) + "\" 指令即可查询歌曲 " + MaiCommandCommand.Songs[index].Title + " [" +
                                         MaiCommandCommand.Songs[index].Type + "] 的相关信息")
                    });
                }
                //MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
            }
            else
            {
                var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
                var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
                if (idRegex.IsMatch(command))
                {
                    try
                    {
                        var id = idHeadRegex.Replace(command, string.Empty);
                        int index = MaiCommandCommand.GetSongIndexByIdUsingStartsWith(id);
                        if (index != -1)
                        {
                            try
                            {
                                GetScoreDto.GetScore.Get(idRegex.Replace(command, "").TrimStart(),
                                    Songs[index].BasicInfo.Version, Songs[index].Id);
                                Program.settingsCommand.GetSettings(source);
                                var image = new ImageMessage
                                {
                                    Base64 = InfoImageGenerator.Generate(index, Songs, "歌曲信息", GetScoreDto.GetScore.Levels,
                                        Program.settingsCommand.CurrentBotSettings.CompressedImage)
                                };

                                MessageManager.SendGroupMessageAsync(source.GroupId,
                                    new MessageChain() { new AtMessage(source.Sender.Id), image });
                                if (((InfoSettings)CurrentGroupCommandSettings).SongPreview)
                                {
                                    var voice = new VoiceMessage
                                    {
                                        Path = AudioToVoiceConverter.ConvertSong(Songs[index].Id)
                                    };
                                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { voice });
                                }
                            }
                            catch
                            {
                                MessageManager.SendGroupMessageAsync(source.GroupId,
                                    new MessageChain()
                                        { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该玩家") });
                            }
                        }
                        else
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain()
                            {
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲")
                            });
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain()
                        {
                            new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲")
                        });
                    }

                }
                else
                {
                    int index = MaiCommandCommand.GetSongIndexByTitleUsingStartsWith(command);
                    if (index != -1)
                    {
                        try
                        {
                            GetScoreDto.GetScore.Get(command.Replace(Songs[index].Title + " ", string.Empty),
                                Songs[index].BasicInfo.Version, Songs[index].Id);
                            Program.settingsCommand.GetSettings(source);
                            var image = new ImageMessage
                            {
                                Base64 = InfoImageGenerator.Generate(index, Songs, "歌曲信息", GetScoreDto.GetScore.Levels,
                                    Program.settingsCommand.CurrentBotSettings.CompressedImage)
                            };
                            MessageManager.SendGroupMessageAsync(source.GroupId,
                                new MessageChain() { new AtMessage(source.Sender.Id), image });

                            if (((InfoSettings)CurrentGroupCommandSettings).SongPreview)
                            {
                                var voice = new VoiceMessage
                                {
                                    Path = AudioToVoiceConverter.ConvertSong(Songs[index].Id)
                                };
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { voice });
                            }
                        }
                        catch
                        {
                            MessageManager.SendGroupMessageAsync(source.GroupId,
                                new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该玩家") });
                        }
                    }
                    else
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain()
                        {
                            new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲")
                        });
                }
                //MessageManager.SendGroupMessageAsync(source.GroupId, " ");
            }

            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            var aliases = MaiCommandCommand.GetAliasByAliasString(command);
            if (aliases.Length != 0)
            {
                if (aliases.Length == 1)
                {
                    try
                    {
                        var i = MaiCommandCommand.GetSongIndexById(aliases[0].Id);
                        GetScoreDto.GetScore.Get(source.Sender.Id.ToInt64(), Songs[i].BasicInfo.Version, Songs[i].Id);
                        Program.settingsCommand.GetSettings(source);
                        var image = new ImageMessage
                        {
                            Base64 = InfoImageGenerator.Generate(i, Songs, "歌曲信息", GetScoreDto.GetScore.Levels,
                                Program.settingsCommand.CurrentBotSettings.CompressedImage)
                        };
                        MessageManager.SendGroupMessageAsync(source.GroupId,
                            new MessageChain() { new AtMessage(source.Sender.Id), image });

                        if (((InfoSettings)CurrentGroupCommandSettings).SongPreview)
                        {
                            var voice = new VoiceMessage
                            {
                                Path = AudioToVoiceConverter.ConvertSong(aliases[0].Id)
                            };
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { voice });
                        }
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId,
                            new MessageChain()
                            {
                                new AtMessage(source.Sender.Id),
                                new PlainMessage(
                                    " 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定")
                            });
                    }
                }
                else
                {
                    string ids = string.Empty;
                    List<int> idsList = new List<int>();
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        if (idsList.Contains(aliases[i].Id))
                            continue;
                        int subIndex = MaiCommandCommand.GetSongIndexById(aliases[i].Id);
                        ids += "ID " + aliases[i].Id + " - " + MaiCommandCommand.Songs[subIndex].Title + " [" +
                               MaiCommandCommand.Songs[subIndex].Type + "]";
                        idsList.Add(aliases[i].Id);
                        if (i != aliases.Length - 1)
                            ids += "\n";
                    }

                    if (idsList.Count == 1)
                    {
                        Parse(
                            "ID " + idsList[0] + command.Replace(MaiCommandCommand.GetAliasStringUsingStartsWith(command),
                                string.Empty), source);
                        return Task.CompletedTask;
                    }

                    int index = MaiCommandCommand.GetSongIndexById(idsList[0]);
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain()
                    {
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(" 该别称有多首歌曲匹配：\n" + ids + "\n*使用 \"lps mai info ID " + idsList[0] +
                                         "\" 指令即可查询歌曲 " + MaiCommandCommand.Songs[index].Title + " [" +
                                         MaiCommandCommand.Songs[index].Type + "] 的相关信息")
                    });
                }
            }
            else
            {
                var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
                var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
                if (idRegex.IsMatch(command))
                {
                    try
                    {
                        var id = idHeadRegex.Replace(command, string.Empty).ToInt32();
                        int index = MaiCommandCommand.GetSongIndexById(id);
                        if (index != -1)
                        {
                            //try
                            //{
                            GetScoreDto.GetScore.Get(source.Sender.Id.ToInt64(), Songs[index].BasicInfo.Version,
                                Songs[index].Id);
                            Program.settingsCommand.GetSettings(source);
                            var image = new ImageMessage
                            {
                                Base64 = InfoImageGenerator.Generate(index, Songs, "歌曲信息", GetScoreDto.GetScore.Levels,
                                    Program.settingsCommand.CurrentBotSettings.CompressedImage)
                            };

                            MessageManager.SendGroupMessageAsync(source.GroupId,
                                new MessageChain() { new AtMessage(source.Sender.Id), image });

                            if (((InfoSettings)CurrentGroupCommandSettings).SongPreview)
                            {
                                var voice = new VoiceMessage
                                {
                                    Path = AudioToVoiceConverter.ConvertSong(id)
                                };
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { voice });
                            }
                            /*}
                            catch
                            {
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
                            }*/
                        }
                        else
                            ParseWithArgument(command, source);
                    }
                    catch
                    {
                        ParseWithArgument(command, source);
                    }

                }
                else
                {
                    int index = MaiCommandCommand.GetSongIndexByTitle(command);
                    if (index != -1)
                    {
                        try
                        {
                            GetScoreDto.GetScore.Get(source.Sender.Id.ToInt64(), Songs[index].BasicInfo.Version,
                                Songs[index].Id);
                            Program.settingsCommand.GetSettings(source);
                            var _image = new ImageMessage
                            {
                                Base64 = InfoImageGenerator.Generate(index, Songs, "歌曲信息", GetScoreDto.GetScore.Levels,
                                    Program.settingsCommand.CurrentBotSettings.CompressedImage)
                            };
                            MessageManager.SendGroupMessageAsync(source.GroupId,
                                new MessageChain() { new AtMessage(source.Sender.Id), _image });

                            if (((InfoSettings)CurrentGroupCommandSettings).SongPreview)
                            {
                                var _voice = new VoiceMessage
                                {
                                    Path = AudioToVoiceConverter.ConvertSong(Songs[index].Id)
                                };
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { _voice });
                            }
                        }
                        catch
                        {
                            MessageManager.SendGroupMessageAsync(source.GroupId,
                                new MessageChain()
                                {
                                    new AtMessage(source.Sender.Id),
                                    new PlainMessage(
                                        " 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定")
                                });
                        }
                    }
                    else
                        ParseWithArgument(command, source);
                }
                //MessageManager.SendGroupMessageAsync(source.GroupId, " ");
            }

            return Task.CompletedTask;
        }
    }
}
