using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.ImageGenerators;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class InfoCommand : MaiCommandBase
{
    public InfoCommand()
    {
        CommandHead = "info";
        DirectCommandHead = "info|查歌";

        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("info", "1");
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source,
        long[] mentionedUserIds)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        if (command.EndsWith(" 是什么歌"))
            command = command.Replace(" 是什么歌", "");
        else if (command.EndsWith("是什么歌"))
            command = command.Replace("是什么歌", "");
        else
            return;

        ParseWithArgument(command, source, mentionedUserIds);
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source,
        long[] mentionedUserIds)
    {
        command = command.Trim();

        var songs = MaiCommandInstance.GetSongs(command);

        if (songs == null)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                GetMultiSearchResultInformationString(command, "info", "信息")
            ]);
            return;
        }

        if (songs.Length != 1)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(GetMultiAliasesMatchedInformationString(songs, "info", "信息"))
            ]);

            return;
        }

        GetScore.ScoreData scoreData;

        var id = mentionedUserIds.Length == 0 ? source.Sender.UserId : mentionedUserIds[0];
        try
        {
            scoreData = GetScore.Get(id, songs[0]);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is TaskCanceledException or HttpRequestException)
                DivingFishErrorHelp(source);
            else
                UnboundErrorHelp(source);

            scoreData = new GetScore.ScoreData
            {
                Levels = [],
                UserExists = false
            };
        }

        var generator = new InfoImageGenerator();

        var isCompressed =
            SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

        var image = new CqImageMsg("base64://" + generator.Generate(songs[0], "歌曲信息",
            scoreData.Levels,
            isCompressed));

        SendMessage(source, [new CqReplyMsg(source.MessageId), image]);

        if (SettingsPool.GetValue(new SettingsIdentifierPair("info", "2"), source.GroupId))
            SendMessage(source,
                [new CqRecordMsg("file:///" + GetSongPath(songs[0].Id))]);
    }

    public static class GetScore
    {
        public static ScoreData Get(object identification, SongDto song)
        {
            return identification switch
            {
                string name => Get(name, song),
                long number => Get(number, song),
                _ => null
            };
        }

        private static ScoreData Get(string name, SongDto song)
        {
            var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/query/plate",
                new { username = name, version = new[] { song.BasicInfo.Version } });
            var scores = JsonConvert.DeserializeObject<ScoresDto>(content);

            return GetScoreData(scores, song);
        }

        private static ScoreData Get(long userId, SongDto song)
        {
            var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/query/plate",
                new { qq = userId, version = new[] { song.BasicInfo.Version } });
            var scores = JsonConvert.DeserializeObject<ScoresDto>(content);

            return GetScoreData(scores, song);
        }

        private static ScoreData GetScoreData(ScoresDto scores, SongDto song)
        {
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