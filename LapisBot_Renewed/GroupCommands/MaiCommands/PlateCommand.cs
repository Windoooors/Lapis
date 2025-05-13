using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.ImageGenerators;
using LapisBot_Renewed.Settings;
using LapisBot_Renewed.Operations.ApiOperation;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class PlateCommand : MaiCommandBase
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
            { "祝", "maimai でらっくす FESTiVAL" },
            { "双", "maimai でらっくす BUDDiES" }
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

        private readonly int[] _excludedSongs =
        {
            70, 146, 185, 189, 190, 341, 419, 451, 455, 460, 524, 687, 688, 712, 731,
            792, 853, 10146, 11213, 11253, 11267
        };

        private readonly int[] _includedRemasterSongs =
        {
            834, 22, 227, 365, 799, 803, 812, 825, 833, 61,
            70, 143, 198, 204, 299, 301, 496, 589, 820, 23, 
            24, 255, 295, 741, 756, 777, 830, 838, 58, 62,
            66, 71, 81, 100, 107, 200, 226, 247, 265, 310, 312,
            759, 763, 793, 809, 816, 818, 17, 80, 145, 256, 282,
            296, 414, 513, 532, 806, 65, 266
        };

        public PlateCommand()
        {
            CommandHead = new Regex("^plate");
            DirectCommandHead = new Regex("进度|完成表");
            ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("plate", "1");
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

        public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
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

                var userName = JsonConvert.DeserializeObject<BestDto>(ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/player",
                    new { qq = source.Sender.UserId })).Username;

                var versionCharacter =
                    wuwuRegex.Replace(shenRegex.Replace(jiangRegex.Replace(jiRegex.Replace(command, ""), ""), ""), "");

                Characters.TryGetValue(versionCharacter, out string versionCharacterInJapanese);

                if (versionCharacterInJapanese != null)
                    versionCharacter = versionCharacterInJapanese;

                PlateToVersion.TryGetValue(versionCharacter, out string singleVersion);

                string[] version = { singleVersion };

                var plateVersionIndex = PlateToVersion.Keys.ToList().IndexOf(versionCharacter);

                if (command == "霸者" || command.StartsWith("舞"))
                {
                    version = new []
                    {
                        "maimai", "maimai PLUS", "maimai GreeN", "maimai GreeN PLUS", "maimai ORANGE",
                        "maimai ORANGE PLUS",
                        "maimai PiNK", "maimai PiNK PLUS", "maimai MURASAKi", "maimai MURASAKi PLUS", "maimai MiLK",
                        "maimai MiLK PLUS",
                        "maimai FiNALE"
                    };
                    plateVersionIndex = 12;
                }
                else if (command.StartsWith("真"))
                    version =
                        new[]
                        {
                            "maimai", "maimai PLUS"
                        };
                
                ScoresDto scores;
                if (!(command == "霸者" || command.StartsWith("舞")))
                {
                    var content = "";
                    content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                        "api/maimaidxprober/query/plate",
                        new { username = "maxscore", version });
                    scores = JsonConvert.DeserializeObject<ScoresDto>(content);
                }
                else
                {
                    var list = new List<ScoresDto.ScoreDto>();
                    foreach (SongDto song in MaiCommandInstance.Songs)
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

                ScoresDto scoresInReality = JsonConvert.DeserializeObject<ScoresDto>(ApiOperator.Instance.Post(
                    BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/plate",
                    new { qq = source.Sender.UserId, version }));

                var songsToBeDisplayed = new List<SongToBeDisplayed>();

                foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                {
                    var song = MaiCommandInstance.GetSong(score.Id);
                    if (Math.Round(song.Ratings[score.LevelIndex], 1) > 13.6f)
                    {
                        var scoreDto = new ScoresDto.ScoreDto();
                        foreach (var realScore in scoresInReality.ScoreDtos)
                        {
                            if (score.Id == realScore.Id && score.LevelIndex == realScore.LevelIndex)
                                scoreDto = realScore;
                        }
                        
                        if (_excludedSongs.Contains(song.Id))
                            if (!((command == "霸者" || command.StartsWith("舞")) && song.Id == 70))
                                continue;

                        if (command == "霸者" || command.StartsWith("舞"))
                        {
                            if (score.LevelIndex == 4 && _includedRemasterSongs.Contains(song.Id))
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
                    var song = MaiCommandInstance.GetSong(score.Id);
                    var scoreDto = new ScoresDto.ScoreDto();
                    foreach (var realScore in scoresInReality.ScoreDtos)
                    {
                        if (score.Id == realScore.Id && score.LevelIndex == realScore.LevelIndex)
                            scoreDto = realScore;
                    }

                    if (_excludedSongs.Contains(song.Id))
                        if (!((command == "霸者" || command.StartsWith("舞")) && song.Id == 70))
                            continue;
                    
                    if (command == "霸者" || command.StartsWith("舞"))
                    {
                        if (score.LevelIndex == 4 && _includedRemasterSongs.Contains(song.Id))
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

                var isCompressed =
                    SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

                var image = new PlateImageGenerator().Generate(songsToBeDisplayed, allSongs, userName,
                    MaiCommandInstance,
                    category, source.Sender.UserId.ToString(), true, plateVersionIndex,
                    isCompressed);

                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqImageMsg("base64://" + image)
                    });

                return Task.CompletedTask;
            }
            catch
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage()
                {
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("未找到该姓名框")
                });
                return Task.CompletedTask;
            }
        }
    }
}