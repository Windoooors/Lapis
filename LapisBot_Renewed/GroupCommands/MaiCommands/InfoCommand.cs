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
using Newtonsoft.Json.Linq;
using static LapisBot_Renewed.MaiCommand;
using System.Reflection;
using static LapisBot_Renewed.GroupCommand;

namespace LapisBot_Renewed
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
        //public new InfoSettings _groupCommandSettings;
        //public new InfoSettings defaultSettings;

        public override Task GetDefaultSettings()
        {
            _groupCommandSettings = ((InfoSettings)defaultSettings).Clone((InfoSettings)defaultSettings);
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            headCommand = new Regex(@"^info\s");
            directCommand = new Regex(@"^info\s");
            defaultSettings = new InfoSettings
            {
                Enabled = true,
                SongPreview = true,
                DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" }, { "SongPreview", "歌曲试听" } },
                SettingsName = "歌曲信息"
            };
            _groupCommandSettings = defaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings");

            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<InfoSettings>(settingsString));
            }
            return Task.CompletedTask;
        }

        public enum Rate { D, C, B, BB, BBB, A, AA, AAA, S, Sp, SS, SSp, SSS, SSSp }

        public enum FCState { None, FC, FCp }
        public enum FSState { None, FS, FSd }

        public class GetScore
        {
            public class Level
            {
                public Rate rate;
                public double achievement;
                public int levelIndex;
                public FCState fc;
                public FSState fs;
            }

            public Level[] levels;

            public void Get(string name, string version, int id)
            {
                try
                {
                    var content = Program.apiOperator.Post("api/maimaidxprober/query/plate", new { username = name, version = new string[] { version } });
                    ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                    List<Level> levelList = new List<Level>();
                    foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                    {
                        if (score.Id == id)
                        {
                            var rate = new Rate();

                            var achievement = score.Achievements;
                            if (achievement >= 100.5)
                                rate = Rate.SSSp;
                            else if (100.5 > achievement && achievement >= 100)
                                rate = Rate.SSS;
                            else if (100 > achievement && achievement >= 99.5)
                                rate = Rate.SSp;
                            else if (99.5 > achievement && achievement >= 99)
                                rate = Rate.SS;
                            else if (99 > achievement && achievement >= 98)
                                rate = Rate.Sp;
                            else if (98 > achievement && achievement >= 97)
                                rate = Rate.S;
                            else if (97 > achievement && achievement >= 94)
                                rate = Rate.AAA;
                            else if (94 > achievement && achievement >= 90)
                                rate = Rate.AA;
                            else if (90 > achievement && achievement >= 80)
                                rate = Rate.A;
                            else if (80 > achievement && achievement >= 75)
                                rate = Rate.BBB;
                            else if (75 > achievement && achievement >= 70)
                                rate = Rate.BB;
                            else if (70 > achievement && achievement >= 60)
                                rate = Rate.B;
                            else if (60 > achievement && achievement >= 50)
                                rate = Rate.C;
                            else if (50 > achievement)
                                rate = Rate.D;

                            var fc = new FCState();
                            var fs = new FSState();

                            if (score.Fc == "fc")
                                fc = FCState.FC;
                            else if (score.Fc == "fcp")
                                fc = FCState.FCp;
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
                            levelList.Add(new Level() { achievement = score.Achievements, rate = rate, levelIndex = score.LevelIndex, fc = fc, fs = fs });

                        }
                    }
                    levels = levelList.ToArray();
                }
                catch
                {
                    List<Level> levelList = new List<Level>();
                    levels = levelList.ToArray();
                }
            }

            public void Get(long number, string version, int id)
            {
                try
                {
                    var content = Program.apiOperator.Post("api/maimaidxprober/query/plate", new { qq = number, version = new string[] { version } });
                    ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                    List<Level> levelList = new List<Level>();
                    foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                    {
                        if (score.Id == id)
                        {
                            var rate = new Rate();

                            var achievement = score.Achievements;
                            if (achievement >= 100.5)
                                rate = Rate.SSSp;
                            else if (100.5 > achievement && achievement >= 100)
                                rate = Rate.SSS;
                            else if (100 > achievement && achievement >= 99.5)
                                rate = Rate.SSp;
                            else if (99.5 > achievement && achievement >= 99)
                                rate = Rate.SS;
                            else if (99 > achievement && achievement >= 98)
                                rate = Rate.Sp;
                            else if (98 > achievement && achievement >= 97)
                                rate = Rate.S;
                            else if (97 > achievement && achievement >= 94)
                                rate = Rate.AAA;
                            else if (94 > achievement && achievement >= 90)
                                rate = Rate.AA;
                            else if (90 > achievement && achievement >= 80)
                                rate = Rate.A;
                            else if (80 > achievement && achievement >= 75)
                                rate = Rate.BBB;
                            else if (75 > achievement && achievement >= 70)
                                rate = Rate.BB;
                            else if (70 > achievement && achievement >= 60)
                                rate = Rate.B;
                            else if (60 > achievement && achievement >= 50)
                                rate = Rate.C;
                            else if (50 > achievement)
                                rate = Rate.D;

                            var fc = new FCState();
                            var fs = new FSState();

                            if (score.Fc == "fc")
                                fc = FCState.FC;
                            else if (score.Fc == "fcp")
                                fc = FCState.FCp;
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
                            levelList.Add(new Level() { achievement = score.Achievements, rate = rate, levelIndex = score.LevelIndex, fc = fc, fs = fs });

                        }
                    }
                    levels = levelList.ToArray();
                }
                catch
                {
                    List<Level> levelList = new List<Level>();
                    levels = levelList.ToArray();
                }
            }

            public class ScoresDto
            {
                [JsonProperty("verlist")]
                public ScoreDto[] ScoreDtos;

                public class ScoreDto
                {
                    [JsonProperty("achievements")]
                    public float Achievements;

                    [JsonProperty("fc")]
                    public string Fc;

                    [JsonProperty("fsd")]
                    public string Fsd;

                    [JsonProperty("Id")]
                    public int Id;

                    [JsonProperty("level_index")]
                    public int LevelIndex;
                }
            }

            public static GetScore getScore = new GetScore();
        }

        public Task ParseWithArgument(string command, GroupMessageReceiver source)
        {
            var aliases = maiCommand.GetAliasByAliasStringUsingStartsWith(command);
            if (aliases.Length != 0)
            {
                if (aliases.Length == 1)
                {
                    try
                    {
                        var i = maiCommand.GetSongIndexById(aliases[0].id);
                        var name = command.Replace(maiCommand.GetAliasStringUsingStartsWith(command) + " ", string.Empty);
                        GetScore.getScore.Get(command.Replace(maiCommand.GetAliasStringUsingStartsWith(command) + " ", string.Empty), songs[i].BasicInfo.Version, songs[i].Id);
                        var _image = new ImageMessage
                        {
                            Base64 = InfoImageGenerator.Generate(i, songs, "歌曲信息", GetScore.getScore.levels)
                        };
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });

                        if (((InfoSettings)_groupCommandSettings).SongPreview)
                        {
                            var _voice = new VoiceMessage
                            {
                                Path = SongToVoiceConverter.Convert(aliases[0].id)
                            };
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { _voice });
                        }
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该玩家") });
                    }
                }
                else
                {
                    string ids = string.Empty;
                    List<int> idsList = new List<int>();
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        if (idsList.Contains(aliases[i].id))
                            continue;
                        int _index = maiCommand.GetSongIndexById(aliases[i].id);
                        ids += "ID " + aliases[i].id + " - " + maiCommand.songs[_index].Title + " [" + maiCommand.songs[_index].Type + "]";
                        idsList.Add(aliases[i].id);
                        if (i != aliases.Length - 1)
                            ids += "\n";
                    }
                    if (idsList.Count == 1)
                    {
                        Parse("ID " + idsList[0] + command.Replace(maiCommand.GetAliasStringUsingStartsWith(command), string.Empty), source);
                        return Task.CompletedTask;
                    }
                    int index = maiCommand.GetSongIndexById(idsList[0]);
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(" 该别称有多首歌曲匹配：\n" + ids + "\n*使用 \"lps mai info ID " + idsList[0] + " " + command.Replace(maiCommand.GetAliasStringUsingStartsWith(command) + " ", string.Empty) + "\" 指令即可查询歌曲 " + maiCommand.songs[index].Title + " [" + maiCommand.songs[index].Type + "] 的相关信息")});
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
                        int index = maiCommand.GetSongIndexByIdUsingStartsWith(id);
                        if (index != -1)
                        {
                            try
                            {
                                GetScore.getScore.Get(idRegex.Replace(command, "").TrimStart(), songs[index].BasicInfo.Version, songs[index].Id);
                                var _image = new ImageMessage
                                {
                                    Base64 = InfoImageGenerator.Generate(index, songs, "歌曲信息", GetScore.getScore.levels)
                                };

                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
                                if (((InfoSettings)_groupCommandSettings).SongPreview)
                                {
                                    var _voice = new VoiceMessage
                                    {
                                        Path = SongToVoiceConverter.Convert(songs[index].Id)
                                    };
                                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { _voice });
                                }
                            }
                            catch
                            {
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该玩家") });
                            }
                        }
                        else
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                    }

                }
                else
                {
                    int index = maiCommand.GetSongIndexByTitleUsingStartsWith(command);
                    if (index != -1)
                    {
                        try
                        {
                            GetScore.getScore.Get(command.Replace(songs[index].Title + " ", string.Empty), songs[index].BasicInfo.Version, songs[index].Id);
                            var _image = new ImageMessage
                            {
                                Base64 = InfoImageGenerator.Generate(index, songs, "歌曲信息", GetScore.getScore.levels)
                            };
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });

                            if (((InfoSettings)_groupCommandSettings).SongPreview)
                            {
                                var _voice = new VoiceMessage
                                {
                                    Path = SongToVoiceConverter.Convert(songs[index].Id)
                                };
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { _voice });
                            }
                        }
                        catch
                        {
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该玩家") });
                        }
                    }
                    else
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                }
                //MessageManager.SendGroupMessageAsync(source.GroupId, " ");
            }
            return Task.CompletedTask;
        }
        public override Task Parse(string command, GroupMessageReceiver source)
        {
            var aliases = maiCommand.GetAliasByAliasString(command);
            if (aliases.Length != 0)
            {
                if (aliases.Length == 1)
                {
                    try
                    {
                        var i = maiCommand.GetSongIndexById(aliases[0].id);
                        GetScore.getScore.Get(source.Sender.Id.ToInt64(), songs[i].BasicInfo.Version, songs[i].Id);
                        var _image = new ImageMessage
                        {
                            Base64 = InfoImageGenerator.Generate(i, songs, "歌曲信息", GetScore.getScore.levels)
                        };
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });

                        if (((InfoSettings)_groupCommandSettings).SongPreview)
                        {
                            var _voice = new VoiceMessage
                            {
                                Path = SongToVoiceConverter.Convert(aliases[0].id)
                            };
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { _voice });
                        }
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
                    }
                }
                else
                {
                    string ids = string.Empty;
                    List<int> idsList = new List<int>();
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        if (idsList.Contains(aliases[i].id))
                            continue;
                        int _index = maiCommand.GetSongIndexById(aliases[i].id);
                        ids += "ID " + aliases[i].id + " - " + maiCommand.songs[_index].Title + " [" + maiCommand.songs[_index].Type + "]";
                        idsList.Add(aliases[i].id);
                        if (i != aliases.Length - 1)
                            ids += "\n";
                    }
                    if (idsList.Count == 1)
                    {
                        Parse("ID " + idsList[0] + command.Replace(maiCommand.GetAliasStringUsingStartsWith(command), string.Empty), source);
                        return Task.CompletedTask;
                    }
                    int index = maiCommand.GetSongIndexById(idsList[0]);
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(" 该别称有多首歌曲匹配：\n" + ids + "\n*使用 \"lps mai info ID " + idsList[0] + "\" 指令即可查询歌曲 " + maiCommand.songs[index].Title + " [" + maiCommand.songs[index].Type + "] 的相关信息")});
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
                        int index = maiCommand.GetSongIndexById(id);
                        if (index != -1)
                        {
                            try
                            {
                                GetScore.getScore.Get(source.Sender.Id.ToInt64(), songs[index].BasicInfo.Version, songs[index].Id);
                                var _image = new ImageMessage
                                {
                                    Base64 = InfoImageGenerator.Generate(index, songs, "歌曲信息", GetScore.getScore.levels)
                                };

                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });

                                if (((InfoSettings)_groupCommandSettings).SongPreview)
                                {
                                    var _voice = new VoiceMessage
                                    {
                                        Path = SongToVoiceConverter.Convert(id)
                                    };
                                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { _voice });
                                }
                            }
                            catch
                            {
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
                            }
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
                    int index = maiCommand.GetSongIndexByTitle(command);
                    if (index != -1)
                    {
                        try
                        {
                            GetScore.getScore.Get(source.Sender.Id.ToInt64(), songs[index].BasicInfo.Version, songs[index].Id);
                            var _image = new ImageMessage
                            {
                                Base64 = InfoImageGenerator.Generate(index, songs, "歌曲信息", GetScore.getScore.levels)
                            };
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });

                            if (((InfoSettings)_groupCommandSettings).SongPreview)
                            {
                                var _voice = new VoiceMessage
                                {
                                    Path = SongToVoiceConverter.Convert(songs[index].Id)
                                };
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { _voice });
                            }
                        }
                        catch
                        {
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
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
