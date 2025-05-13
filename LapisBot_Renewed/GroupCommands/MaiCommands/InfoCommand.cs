using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.ImageGenerators;
using LapisBot_Renewed.Operations.ApiOperation;
using LapisBot_Renewed.Settings;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class InfoCommand : MaiCommandBase
    {
        public InfoCommand()
        {
            CommandHead = new Regex("^info");
            DirectCommandHead = new Regex("^info|^查歌");

            ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("info", "1");
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

            public bool UserExists;

            public void Get(string name, SongDto song)
            {
                try
                {
                    var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,"api/maimaidxprober/query/plate",
                        new { username = name, version = new string[] { song.BasicInfo.Version } });
                    ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                    List<Level> levelList = new List<Level>();
                    foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                    {
                        if (score.Id == song.Id)
                        {
                            levelList.Add(new Level()
                            {
                                Achievement = score.Achievements, Rate = GetRate(score.Achievements), LevelIndex = score.LevelIndex,
                                Fc = score.Fc,
                                Fs = score.Fs
                            });
                        }
                    }

                    Levels = levelList.ToArray();
                    UserExists = Levels.Length > 0;
                }
                catch
                {
                    Levels = [];
                    UserExists = false;
                }
            }

            public void Get(long number, SongDto song)
            {
                try
                {
                    var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                        "api/maimaidxprober/query/plate",
                        new { qq = number, version = new string[] { song.BasicInfo.Version } });
                    ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                    List<Level> levelList = new List<Level>();
                    foreach (ScoresDto.ScoreDto score in scores.ScoreDtos)
                    {
                        if (score.Id == song.Id)
                        {
                            levelList.Add(new Level()
                            {
                                Achievement = score.Achievements, Rate = GetRate(score.Achievements), LevelIndex = score.LevelIndex,
                                Fc = score.Fc,
                                Fs = score.Fs
                            });

                        }
                    }

                    Levels = levelList.ToArray();
                    if (Levels.Length > 0)
                        UserExists = true;
                    else
                        UserExists = false;
                }
                catch
                {
                    Levels = [];
                    UserExists = false;
                }
            }

            public static readonly GetScoreDto GetScore = new();
        }

        public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
        {
            if (!SettingsCommand.Instance.GetValue(new("litecommand", "1"), source.GroupId))
                return Task.CompletedTask;

            if (command.EndsWith(" 是什么歌"))
                command = command.Replace(" 是什么歌", "");
            else if (command.EndsWith("是什么歌"))
                command = command.Replace("是什么歌", "");
            else
                return Task.CompletedTask;
            
            ParseWithArgument(command, source);
            return Task.CompletedTask;
        }

        public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
        {
            var songs = MaiCommandInstance.GetSongsUsingStartsWith(command);

            if (songs == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该歌曲") });
                return Task.CompletedTask;
            }

            var indicatorString = MaiCommandInstance.GetSongIndicatorString(command);

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
                if (!GetScoreDto.GetScore.UserExists)
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

                    if (!GetScoreDto.GetScore.UserExists)
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
            
            var isCompressed =
                SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);
            
            var image = new CqImageMsg("base64://" + generator.Generate(songs[0], "歌曲信息",
                GetScoreDto.GetScore.Levels,
                isCompressed));

            Program.Session.SendGroupMessageAsync(source.GroupId, [new CqReplyMsg(source.MessageId), image]);

            if (SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("info", "2"), source.GroupId))
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    [new CqRecordMsg("file:///" + GetSongPath(songs[0].Id))]);

            return Task.CompletedTask;
        }
    }
}
