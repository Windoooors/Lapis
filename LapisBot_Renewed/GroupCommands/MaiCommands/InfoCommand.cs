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
            DirectCommand = new Regex(@"^info\s|是什么歌$|\s是什么歌$");
            DefaultSettings = new InfoSettings
            {
                Enabled = true,
                SongPreview = false,
                DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" }, { "SongPreview", "歌曲试听" } },
                SettingsName = "歌曲信息"
            };
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");
            }

            foreach (var path in Directory.GetFiles(AppContext.BaseDirectory +
                                                    CurrentGroupCommandSettings.SettingsName +
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

        public class GetScoreDto
        {
            public class Level
            {
                public Rate Rate;
                public double Achievement;
                public int LevelIndex;
                public string Fc;
                public string Fs;
            }

            public Level[] Levels;

            public bool userExists;

            public void Get(string name, SongDto song)
            {
                try
                {
                    var content = Program.ApiOperator.Post("api/maimaidxprober/query/plate",
                        new { username = name, version = new string[] { song.BasicInfo.Version } }, true);
                    ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                    List<Level> levelList = new List<Level>();
                    foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                    {
                        if (score.Id == song.Id)
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
                                Achievement = score.Achievements, Rate = rate, LevelIndex = score.LevelIndex,
                                Fc = score.Fc,
                                Fs = score.Fs
                            });

                        }
                    }

                    Levels = levelList.ToArray();
                    if (Levels.Length > 0)
                        userExists = true;
                    else
                        userExists = false;
                }
                catch
                {
                    Levels = new Level[0];
                    userExists = false;
                }
            }

            public void Get(long number, SongDto song)
            {
                try
                {
                    var content = Program.ApiOperator.Post("api/maimaidxprober/query/plate",
                        new { qq = number, version = new string[] { song.BasicInfo.Version } }, true);
                    ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                    List<Level> levelList = new List<Level>();
                    foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                    {
                        if (score.Id == song.Id)
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

                            levelList.Add(new Level()
                            {
                                Achievement = score.Achievements, Rate = rate, LevelIndex = score.LevelIndex,
                                Fc = score.Fc,
                                Fs = score.Fs
                            });

                        }
                    }

                    Levels = levelList.ToArray();
                    if (Levels.Length > 0)
                        userExists = true;
                    else
                        userExists = false;
                }
                catch
                {
                    Levels = new Level[0];
                    userExists = false;
                }
            }
            
            public static GetScoreDto GetScore = new GetScoreDto();
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            var songs = MaiCommandCommand.GetSongsUsingStartsWith(command);

            if (songs == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该歌曲") });
                return Task.CompletedTask;
            }

            var indicatorString = MaiCommandCommand.GetSongIndicatorString(command);

            if (songs.Length != 1)
            {
                string ids = string.Empty;
                List<int> idsList = new List<int>();
                for (int i = 0; i < songs.Length; i++)
                {
                    ids += "ID " + songs[i].Id + " - " + songs[i].Title + " [" + songs[i].Type + "]";
                    if (i != songs.Length - 1)
                        ids += "\n";
                    idsList.Add(songs[i].Id);
                }

                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                {
                    new CqReplyMsg(source.MessageId), new CqTextMsg(
                        " 该别称有多首歌曲匹配：\n" + ids + "\n*发送 \"lps mai info ID " + idsList[0] + "\" 指令即可查询歌曲 " +
                        songs[0].Title + " [" + songs[0].Type +
                        "] 的信息")
                });
                
                return Task.CompletedTask;
            }

            var indicatorRegex = new Regex(indicatorString);
            var userName = indicatorRegex.Replace(command.ToLower(), "", 1);
            if (userName != string.Empty)
            {
                GetScoreDto.GetScore.Get(userName.Substring(1, userName.Length - 1), songs[0]);
                if (!GetScoreDto.GetScore.userExists)
                {
                    try
                    {
                        GetScoreDto.GetScore.Get(Int64.Parse(userName.Substring(1, userName.Length - 1)), songs[0]);
                    }
                    catch
                    {
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                            new CqMessage
                                { new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该玩家") });
                        return Task.CompletedTask;
                    }

                    if (!GetScoreDto.GetScore.userExists)
                    {
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                            new CqMessage
                                { new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该玩家") });
                        return Task.CompletedTask;
                    }
                }
            }
            else
            {
                var id = source.Sender.UserId;
                GetScoreDto.GetScore.Get(id, songs[0]);
            }

            var generator = new InfoImageGenerator();
            
            Program.SettingsCommand.GetSettings(source);
            var image = new CqImageMsg("base64://" + generator.Generate(songs[0], "歌曲信息",
                GetScoreDto.GetScore.Levels,
                Program.SettingsCommand.CurrentBotSettings.CompressedImage));

            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                { new CqReplyMsg(source.MessageId), image });

            if (((InfoSettings)CurrentGroupCommandSettings).SongPreview)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                        { new CqRecordMsg("file:///" + new AudioToVoiceConverter().GetSongPath(songs[0].Id)) });
            }

            return Task.CompletedTask;
        }
    }
}
