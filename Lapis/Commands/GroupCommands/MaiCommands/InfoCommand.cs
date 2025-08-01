using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        IntendedArgumentCount = 2;
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("info", "1");
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        if (command.EndsWith(" 是什么歌"))
            command = command.Replace(" 是什么歌", "");
        else if (command.EndsWith("是什么歌"))
            command = command.Replace("是什么歌", "");
        else
            return;

        ParseWithArgument([command], source);
    }

    public override void ParseWithArgument(string[] arguments, CqGroupMessagePostContext source)
    {
        var songs = MaiCommandInstance.GetSongs(arguments[0], true);

        if (songs == null)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                GetMultiSearchResultInformationString(arguments[0], "info", "信息")
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

        if (arguments.Length > 1)
        {
            var isGroupMember =
                GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[1], source.GroupId,
                    out var groupMembers) && groupMembers.Length == 1;

            var isQqId = long.TryParse(arguments[1], out var qqId);

            try
            {
                scoreData = isGroupMember
                    ? GetScore.Get(groupMembers[0].Id, songs[0])
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

                return;
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

                scoreData = new GetScore.ScoreData([]);
            }
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
                "api/maimaidxprober/dev/player/record",
                new { username = name, music_id = song.Id.ToString() },
                [new KeyValuePair<string, string>("Developer-Token", BotConfiguration.Instance.DivingFishDevToken)]);

            return new ScoreData(
                JsonConvert.DeserializeObject<Dictionary<string, LevelDto[]>>(content).Values.ToArray()[0]);
        }

        private static ScoreData Get(long userId, SongDto song)
        {
            var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/dev/player/record",
                new { qq = userId.ToString(), music_id = song.Id.ToString() },
                [new KeyValuePair<string, string>("Developer-Token", BotConfiguration.Instance.DivingFishDevToken)]);

            return new ScoreData(
                JsonConvert.DeserializeObject<Dictionary<string, LevelDto[]>>(content).Values.ToArray()[0]);
        }

        public class LevelDto
        {
            [JsonProperty("achievements")] public float Achievement;
            [JsonProperty("fc")] public string Fc;
            [JsonProperty("fs")] public string Fs;
            [JsonProperty("level_index")] public int LevelIndex;
            [JsonProperty("rate")] public Rate Rate;
            [JsonProperty("ra")] public int Rating;
        }

        public class ScoreData(LevelDto[] levels)
        {
            public readonly LevelDto[] Levels = levels;

            public readonly bool UserExists = levels.Length > 0;
        }

        public class MessageDto
        {
            [JsonProperty("message")] public string Message;
        }
    }
}