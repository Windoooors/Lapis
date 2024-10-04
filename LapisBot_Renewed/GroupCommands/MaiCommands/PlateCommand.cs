using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.ImageGenerators;
using Xamarin.Forms.Internals;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class PlateCommand : MaiCommand
    {

        public enum PlateCategories
        {
            ji,
            jiang,
            shen,
            wuwu,
            bazhe
        };

        public Dictionary<string, string> PlateToVersion = new Dictionary<string, string>()
        {
            { "真", "maimai PLUS" },
            { "超", "maimai GreeN" },
            { "檄", "maimai GreeN PLUS" },
            { "橙", "maimai ORANGE" },
            { "暁", "maimai ORANGE PLUS" },
            { "桃", "maimai PiNK" },
            { "櫻", "maimai PiNK PLUS" },
            { "紫", "maimai MURASAKi" },
            { "菫", "maimai MURASAKi PLUS" },
            { "白", "maimai MiLK" },
            { "雪", "MiLK PLUS" },
            { "輝", "maimai FiNALE" },
            { "舞", "maimai ALL" },
            { "熊", "maimai でらっくす" },
            { "華", "maimai でらっくす" },
            { "爽", "maimai でらっくす Splash" },
            { "煌", "maimai でらっくす Splash" },
            { "宙", "maimai でらっくす UNiVERSE" },
            { "星", "maimai でらっくす UNiVERSE" },
            { "祭", "maimai でらっくす FESTiVAL" },
            { "祝", "maimai でらっくす FESTiVAL" }
            //{ "双", "maimai でらっくす BUDDiES" }
        };

        public Dictionary<string, string> Characters = new Dictionary<string, string>()
        {
            { "晓", "暁" },
            { "樱", "櫻" },
            { "堇", "菫" },
            { "辉", "輝" },
            { "华", "華" }
        };

        public Dictionary<string, string> Categories = new Dictionary<string, string>()
        {
            { "流行&动漫", "anime" },
            { "舞萌", "maimai" },
            { "niconico & VOCALOID", "niconico" },
            { "东方Project", "touhou" },
            { "其他游戏", "game" },
            { "音击&中二节奏", "ongeki" },
            { "POPSアニメ", "anime" },
            { "maimai", "maimai" },
            { "niconicoボーカロイド", "niconico" },
            { "東方Project", "touhou" },
            { "ゲームバラエティ", "game" },
            { "オンゲキCHUNITHM", "ongeki" },
            { "宴会場", "宴会场" }
        };

        public int[] ExcludedSongs =
        [
            70, 146, 185, 189, 190, 341, 419, 451, 455, 460, 524, 687, 688, 712, 731,
            792, 853, 10146, 11213, 11253, 11267
        ];

        public int[] IncludedRemasterSongs =
        [
            834,
            22,
            227,
            365,
            799,
            803,
            812,
            825,
            833,
            61,
            70,
            143,
            198,
            204,
            299,
            301,
            496,
            589,
            820,
            23,
            24,
            255,
            295,
            741,
            756,
            777,
            830,
            838,
            58,
            62,
            66,
            71,
            81,
            100,
            107,
            200,
            226,
            247,
            265,
            310,
            312,
            759,
            763,
            793,
            809,
            816,
            818,
            17,
            80,
            145,
            256,
            282,
            296,
            414,
            513,
            532,
            806,
            65,
            266
        ];

        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^plate\s");
            DirectCommand = new Regex(@"进度$|完成表$");
            DefaultSettings.SettingsName = "牌子查询";
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

        public class UsernameDto
        {
            [JsonProperty("username")] public string Username;
        }

        public class SongToBeDisplayed
        {
            public SongDto SongDto;
            public ScoresDto.ScoreDto ScoreDto;
            public int LevelIndex;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            if (command == "真将" || command == "")
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage()
                {
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("未找到该姓名框")
                });
                return Task.CompletedTask;
            }

            try
            {

                var jiRegex = new Regex("极$");
                var jiangRegex = new Regex("将$");
                var shenRegex = new Regex("神$");
                var wuwuRegex = new Regex("舞舞$");
                var bazheRegex = new Regex("^霸者$");

                var userName = JsonConvert.DeserializeObject<BestDto>(Program.apiOperator.Post(
                    "api/maimaidxprober/query/player",
                    new { qq = source.Sender.UserId })).Username;

                var versionCharacter =
                    wuwuRegex.Replace(shenRegex.Replace(jiangRegex.Replace(jiRegex.Replace(command, ""), ""), ""), "");

                var versionCharacterInJapanese = "";

                Characters.TryGetValue(versionCharacter, out versionCharacterInJapanese);

                if (versionCharacterInJapanese != null)
                    versionCharacter = versionCharacterInJapanese;

                var singleVersion = "";

                PlateToVersion.TryGetValue(versionCharacter, out singleVersion);

                string[] version = { singleVersion };

                var plateVersionIndex = PlateToVersion.Keys.ToList().IndexOf(versionCharacter);

                if (command == "霸者" || command.StartsWith("舞"))
                {
                    version =
                    [
                        "maimai", "maimai PLUS", "maimai GreeN", "maimai GreeN PLUS", "maimai ORANGE",
                        "maimai ORANGE PLUS",
                        "maimai PiNK", "maimai PiNK PLUS", "maimai MURASAKi", "maimai MURASAKi PLUS", "maimai MiLK",
                        "maimai MiLK PLUS",
                        "maimai FiNALE"
                    ];
                    plateVersionIndex = 12;
                }
                else if (command.StartsWith("真"))
                    version =
                    [
                        "maimai", "maimai PLUS"
                    ];

                var content = "";
                ScoresDto scores;
                if (!(command == "霸者" || command.StartsWith("舞")))
                {
                    content = Program.apiOperator.Post("api/maimaidxprober/query/plate",
                        new { username = "maxscore", version });
                    scores = JsonConvert.DeserializeObject<ScoresDto>(content);
                }
                else
                {
                    var list = new List<ScoresDto.ScoreDto>();
                    foreach (SongDto song in MaiCommandCommand.Songs)
                    {
                        if (song.Id < 1000)
                        {
                            for (int i = 0; i < song.Ratings.Length; i++)
                                list.Add(new ScoresDto.ScoreDto()
                                    { Id = song.Id, LevelIndex = i });
                        }
                    }

                    scores = new ScoresDto() { ScoreDtos = list.ToArray() };
                }

                ScoresDto scoresInRealilty = JsonConvert.DeserializeObject<ScoresDto>(Program.apiOperator.Post(
                    "api/maimaidxprober/query/plate",
                    new { qq = source.Sender.UserId, version }));

                var songsToBeDisplayed = new List<SongToBeDisplayed>();

                foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                {
                    var song = MaiCommandCommand.GetSong(score.Id);
                    if (Math.Round(song.Ratings[score.LevelIndex], 1) > 13.6f)
                    {
                        var scoreDto = new ScoresDto.ScoreDto();
                        foreach (var realScore in scoresInRealilty.ScoreDtos)
                        {
                            if (score.Id == realScore.Id && score.LevelIndex == realScore.LevelIndex)
                                scoreDto = realScore;
                        }
                        
                        if (ExcludedSongs.Contains(song.Id))
                            if (!((command == "霸者" || command.StartsWith("舞")) && song.Id == 70))
                                continue;

                        if (command == "霸者" || command.StartsWith("舞"))
                        {
                            if (score.LevelIndex == 4 && IncludedRemasterSongs.Contains(song.Id))
                                songsToBeDisplayed.Add(new SongToBeDisplayed
                                    { LevelIndex = score.LevelIndex, SongDto = song, ScoreDto = scoreDto });
                            else if (score.LevelIndex != 4)
                                songsToBeDisplayed.Add(new SongToBeDisplayed
                                    { LevelIndex = score.LevelIndex, SongDto = song, ScoreDto = scoreDto });
                        }
                        else if (score.LevelIndex != 4)
                        {
                            songsToBeDisplayed.Add(new SongToBeDisplayed
                                { LevelIndex = score.LevelIndex, SongDto = song, ScoreDto = scoreDto });
                        }
                    }
                }

                var allSongs = new List<SongToBeDisplayed>();

                foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                {
                    var song = MaiCommandCommand.GetSong(score.Id);
                    var scoreDto = new ScoresDto.ScoreDto();
                    foreach (var realScore in scoresInRealilty.ScoreDtos)
                    {
                        if (score.Id == realScore.Id && score.LevelIndex == realScore.LevelIndex)
                            scoreDto = realScore;
                    }

                    if (ExcludedSongs.Contains(song.Id))
                        if (!((command == "霸者" || command.StartsWith("舞")) && song.Id == 70))
                            continue;
                    
                    if (command == "霸者" || command.StartsWith("舞"))
                    {
                        if (score.LevelIndex == 4 && IncludedRemasterSongs.Contains(song.Id))
                            allSongs.Add(new SongToBeDisplayed
                                { LevelIndex = score.LevelIndex, SongDto = song, ScoreDto = scoreDto });
                        else if (score.LevelIndex != 4)
                            allSongs.Add(new SongToBeDisplayed
                                { LevelIndex = score.LevelIndex, SongDto = song, ScoreDto = scoreDto });
                    }
                    else if (score.LevelIndex != 4)
                        allSongs.Add(new SongToBeDisplayed
                            { LevelIndex = score.LevelIndex, SongDto = song, ScoreDto = scoreDto });
                }

                var category = PlateCategories.ji;

                if (jiRegex.IsMatch(command))
                    category = PlateCategories.ji;
                if (jiangRegex.IsMatch(command))
                    category = PlateCategories.jiang;
                if (shenRegex.IsMatch(command))
                    category = PlateCategories.shen;
                if (wuwuRegex.IsMatch(command))
                    category = PlateCategories.wuwu;
                if (bazheRegex.IsMatch(command))
                    category = PlateCategories.bazhe;

                Program.settingsCommand.GetSettings(source);

                var image = new PlateImageGenerator().Generate(songsToBeDisplayed, allSongs, userName,
                    MaiCommandCommand,
                    category, source.Sender.UserId.ToString(), true, plateVersionIndex,
                    Program.settingsCommand.CurrentBotSettings.CompressedImage);

                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqImageMsg("base64://" + image)
                ]);

                return Task.CompletedTask;
            }
            catch
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("未找到该姓名框")
                ]);

                return Task.CompletedTask;
            }
        }
    }
}