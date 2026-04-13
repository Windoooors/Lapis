using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.ImageGenerators;
using Lapis.Operations.ApiOperation;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class InfoCommand : MaiCommandBase
{
    public InfoCommand()
    {
        CommandHead = "info";
        DirectCommandHead = "info|查歌";
        IntendedArgumentCount = 2;
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("info", "1");
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("lite_command", "1"), source.GroupId))
            return;

        var originalCommandString = command;

        if (command.EndsWith(" 是什么歌"))
            command = command.Replace(" 是什么歌", "");
        else if (command.EndsWith("是什么歌"))
            command = command.Replace("是什么歌", "");
        else
            return;

        ParseWithArgument([command], originalCommandString, source);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (!MaiCommandInstance.TryGetSongs(arguments[0], out var songs,
                new CommandBehaviorInformationDataObject("info", "信息"),
                source, true))
            return;

        if (songs.Length != 1)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(GetMultiAliasesMatchedInformationString(songs,
                    new CommandBehaviorInformationDataObject("info", "信息")))
            ]);

            return;
        }

        GetScore.ScoreData scoreData;

        if (arguments.Length > 1)
        {
            var isGroupMember =
                GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[1],
                    out var groupMembers, source) && groupMembers.Length == 1;

            var isQqId = long.TryParse(arguments[1], out var qqId);

            try
            {
                scoreData = isGroupMember
                    ? GetScore.Get(groupMembers[0].QqId, songs[0])
                    : isQqId
                        ? GetScore.Get(qqId, songs[0])
                        : GetScore.Get(arguments[1], songs[0]);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException { StatusCode: HttpStatusCode.BadRequest })
                    ObjectUserUnboundErrorHelp(source);
                else if (ex.InnerException is TaskCanceledException)
                    DivingFishErrorHelp(source);

                scoreData = new GetScore.ScoreData([], null);
            }
        }
        else
        {
            try
            {
                scoreData = GetScore.Get(source.Sender.UserId, songs[0]);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException { StatusCode: HttpStatusCode.BadRequest })
                    UnboundErrorHelp(source);
                else if (ex.InnerException is TaskCanceledException)
                    DivingFishErrorHelp(source);

                scoreData = new GetScore.ScoreData([], null);
            }
        }

        var generator = new InfoImageGenerator();

        var isCompressed =
            SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

        var image = new CqImageMsg("base64://" + generator.Generate(MaiCommandInstance.ToSongDto(songs[0]), "歌曲信息",
            scoreData,
            isCompressed));

        SendMessage(source, [new CqReplyMsg(source.MessageId), image]);

        if (SettingsPool.GetValue(new SettingsIdentifierPair("info", "2"), source.GroupId) &&
            File.Exists(GetSongPath(songs[0].SongId)))
            SendMessage(source,
                [new CqRecordMsg("file:///" + GetSongPath(songs[0].SongId))]);
    }

    public static class GetScore
    {
        public static ScoreData Get(object identification, SongMetaData song)
        {
            return identification switch
            {
                string name => Get(name, song),
                long number => Get(number, song),
                _ => null
            };
        }

        private static ScoreData Get(string name, SongMetaData song)
        {
            var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/dev/player/record",
                new { username = name, music_id = song.SongId.ToString() },
                [new KeyValuePair<string, string>("Developer-Token", BotConfiguration.Instance.DivingFishDevToken)]);

            if (content.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"Unexpected status code: {content.StatusCode}", null,
                    content.StatusCode);

            return new ScoreData(
                JsonConvert.DeserializeObject<Dictionary<string, LevelDto[]>>(content.Result).Values.ToArray()[0],
                MaiCommandInstance.ToSongDto(song));
        }

        private static ScoreData Get(long userId, SongMetaData song)
        {
            var hasDataFromLapis =
                MaiCommandInstance.MaiScoreOperator.TryGetInformationFromLapis(userId, song.SongId,
                    out var dataFromLapis);

            if (hasDataFromLapis) return dataFromLapis;

            var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/dev/player/record",
                new { qq = userId.ToString(), music_id = song.SongId.ToString() },
                [new KeyValuePair<string, string>("Developer-Token", BotConfiguration.Instance.DivingFishDevToken)]);

            if (content.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"Unexpected status code: {content.StatusCode}", null,
                    content.StatusCode);

            return new ScoreData(
                JsonConvert.DeserializeObject<Dictionary<string, LevelDto[]>>(content.Result).Values.ToArray()[0],
                MaiCommandInstance.ToSongDto(song));
        }

        public class LevelDto
        {
            [JsonProperty("achievements")] public float Achievement;
            [JsonProperty("dxScore")] public int DxScore;
            [JsonProperty("fc")] public string Fc;
            [JsonProperty("fs")] public string Fs;
            [JsonProperty("level_index")] public int LevelIndex;
            public int PlayCount = -1;
            [JsonProperty("rate")] public Rate Rate;
            [JsonProperty("ra")] public int Rating;
        }

        public class ScoreData(LevelDto[] levels, SongDto song)
        {
            public readonly LevelDto[] Levels = levels;

            public readonly bool UserExists = levels.Length > 0;

            public SongDto Song = song;
        }

        public class MessageDto
        {
            [JsonProperty("message")] public string Message;
        }
    }
}