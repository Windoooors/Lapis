using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.ImageGenerators;
using LapisBot.Operations.ApiOperation;
using LapisBot.Settings;
using Newtonsoft.Json;

namespace LapisBot.GroupCommands.MaiCommands;

public class InfoCommand : MaiCommandBase
{
    public InfoCommand()
    {
        CommandHead = new Regex("^info");
        DirectCommandHead = new Regex("^info|^查歌");

        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("info", "1");
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        if (command.EndsWith(" 是什么歌"))
            command = command.Replace(" 是什么歌", "");
        else if (command.EndsWith("是什么歌"))
            command = command.Replace("是什么歌", "");
        else
            return;

        ParseWithArgument(command, source);
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        var songs = MaiCommandInstance.GetSongsUsingStartsWith(command);

        if (songs == null)
        {
            Program.Session.SendGroupMessageAsync(source.GroupId,
                [new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该歌曲")]);
            return;
        }

        var indicatorString = MaiCommandInstance.GetSongIndicatorString(command);

        if (songs.Length != 1)
        {
            Program.Session.SendGroupMessageAsync(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(GetMultiAliasesMatchedInformationString(songs, "info", "信息"))
            ]);

            return;
        }

        GetScore.ScoreData scoreData;

        var indicatorRegex = new Regex(indicatorString);
        var userName = indicatorRegex.Replace(command.ToLower(), "", 1);
        if (userName != string.Empty)
        {
            scoreData = GetScore.Get(userName.Substring(1, userName.Length - 1), songs[0]);
            if (!scoreData.UserExists)
            {
                try
                {
                    scoreData = GetScore.Get(long.Parse(userName.Substring(1, userName.Length - 1)), songs[0]);
                }
                catch
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        [new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该玩家")]);
                    return;
                }

                if (!scoreData.UserExists)
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        [new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该玩家")]);
                    return;
                }
            }
        }
        else
        {
            var id = source.Sender.UserId;
            scoreData = GetScore.Get(id, songs[0]);
        }

        var generator = new InfoImageGenerator();

        var isCompressed =
            SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

        var image = new CqImageMsg("base64://" + generator.Generate(songs[0], "歌曲信息",
            scoreData.Levels,
            isCompressed));

        Program.Session.SendGroupMessageAsync(source.GroupId, [new CqReplyMsg(source.MessageId), image]);

        if (SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("info", "2"), source.GroupId))
            Program.Session.SendGroupMessageAsync(source.GroupId,
                [new CqRecordMsg("file:///" + GetSongPath(songs[0].Id))]);
    }

    public static class GetScore
    {
        public static ScoreData Get(string name, SongDto song)
        {
            try
            {
                var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/plate",
                    new { username = name, version = new[] { song.BasicInfo.Version } });
                var scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                var levelList = new List<Level>();
                foreach (var score in scores.ScoreDtos)
                    if (score.Id == song.Id)
                        levelList.Add(new Level
                        {
                            Achievement = score.Achievements, Rate = GetRate(score.Achievements),
                            LevelIndex = score.LevelIndex,
                            Fc = score.Fc,
                            Fs = score.Fs
                        });

                return new ScoreData
                {
                    Levels = levelList.ToArray(),
                    UserExists = levelList.ToArray().Length > 0
                };
            }
            catch
            {
                return new ScoreData
                {
                    Levels = [],
                    UserExists = false
                };
            }
        }

        public static ScoreData Get(long number, SongDto song)
        {
            try
            {
                var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/plate",
                    new { qq = number, version = new[] { song.BasicInfo.Version } });
                var scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                var levelList = new List<Level>();
                foreach (var score in scores.ScoreDtos)
                    if (score.Id == song.Id)
                        levelList.Add(new Level
                        {
                            Achievement = score.Achievements, Rate = GetRate(score.Achievements),
                            LevelIndex = score.LevelIndex,
                            Fc = score.Fc,
                            Fs = score.Fs
                        });

                return new ScoreData
                {
                    Levels = levelList.ToArray(),
                    UserExists = levelList.ToArray().Length > 0
                };
            }
            catch
            {
                return new ScoreData
                {
                    Levels = [],
                    UserExists = false
                };
            }
        }

        public class Level
        {
            public double Achievement;
            public string Fc;
            public string Fs;
            public int LevelIndex;
            public Rate Rate;
        }

        public class ScoreData
        {
            public Level[] Levels;

            public bool UserExists;
        }
    }
}