using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.ImageGenerators;
using Lapis.Miscellaneous;
using Lapis.Operations.ApiOperation;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class PlateCommand : MaiCommandBase
{
    public enum PlateCategories
    {
        Ji,
        Jiang,
        Shen,
        Wuwu,
        Bazhe
    }

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
        CommandHead = "plate";
        DirectCommandHead = "plate|牌子";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("plate", "1");
        IntendedArgumentCount = 2;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        var command = arguments[0];

        if (command == "真将" || command == "")
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("未找到该姓名框")
            ]);
            return;
        }

        var jiRegex = new Regex("极$");
        var jiangRegex = new Regex("将$");
        var shenRegex = new Regex("神$");
        var wuwuRegex = new Regex("舞舞$");
        var bazheRegex = new Regex("^霸者$");

        string userName;

        var cachedInLapis = MaiScoreOperator.UserDataCached(source.UserId);

        if (!cachedInLapis)
        {
            try
            {
                ApiOperator.RequestResult content;

                if (arguments.Length > 1)
                {
                    var isGroupMember =
                        GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[1],
                            out var groupMembers, source) && groupMembers.Length == 1;
                    var isQqId = long.TryParse(arguments[1], out _);
                    content = isGroupMember
                        ? ApiOperator.Instance.Post(
                            BotConfiguration.Instance.DivingFishUrl,
                            "api/maimaidxprober/query/player",
                            new { qq = groupMembers[0].QqId.ToString() },
                            [
                                new KeyValuePair<string, string>("Developer-Token",
                                    BotConfiguration.Instance.DivingFishDevToken)
                            ])
                        : isQqId
                            ? ApiOperator.Instance.Post(
                                BotConfiguration.Instance.DivingFishUrl,
                                "api/maimaidxprober/query/player",
                                new { qq = arguments[1] },
                                [
                                    new KeyValuePair<string, string>("Developer-Token",
                                        BotConfiguration.Instance.DivingFishDevToken)
                                ])
                            : ApiOperator.Instance.Post(
                                BotConfiguration.Instance.DivingFishUrl,
                                "api/maimaidxprober/query/player",
                                new { username = arguments[1] },
                                [
                                    new KeyValuePair<string, string>("Developer-Token",
                                        BotConfiguration.Instance.DivingFishDevToken)
                                ]);

                    if (content.StatusCode != HttpStatusCode.OK)
                        throw new HttpRequestException($"Unexpected status code: {content.StatusCode}", null,
                            content.StatusCode);
                }
                else
                {
                    content = ApiOperator.Instance.Post(
                        BotConfiguration.Instance.DivingFishUrl,
                        "api/maimaidxprober/query/player",
                        new { qq = source.Sender.UserId.ToString() },
                        [
                            new KeyValuePair<string, string>("Developer-Token",
                                BotConfiguration.Instance.DivingFishDevToken)
                        ]);
                }

                userName = JsonConvert.DeserializeObject<BestDto>(content.Result).Username;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskCanceledException or HttpRequestException)
                {
                    DivingFishErrorHelp(source);
                    return;
                }

                if (arguments.Length > 1)
                    ObjectUserUnboundErrorHelp(source);
                else
                    UnboundErrorHelp(source);
                return;
            }
        }
        else
        {
            var hasName = GroupMemberCommandBase.GroupMemberCommandInstance.TryGetNickname(source.UserId, out userName);
            userName = hasName ? userName : source.UserId.ToString();
        }

        var versionCharacter =
            wuwuRegex.Replace(shenRegex.Replace(jiangRegex.Replace(jiRegex.Replace(command, "", 1), "", 1), "", 1), "",
                1);

        SharedConsts.Characters.TryGetValue(versionCharacter, out var versionCharacterInJapanese);

        if (versionCharacterInJapanese != null)
            versionCharacter = versionCharacterInJapanese;

        var singleVersionFound =
            SharedConsts.PlateCharacterToVersionName.TryGetValue(versionCharacter, out var singleVersion);

        string[] version = singleVersionFound ? [singleVersion] : null;

        var plateVersionIndex = SharedConsts.PlateCharacterToVersionName.Keys.ToList().IndexOf(versionCharacter);

        if (command == "霸者" || command.StartsWith("舞"))
        {
            version =
            [
                "maimai", "maimai PLUS", "maimai GreeN", "maimai GreeN PLUS", "maimai ORANGE",
                "maimai ORANGE PLUS",
                "maimai PiNK", "maimai PiNK PLUS", "maimai MURASAKi", "maimai MURASAKi PLUS", "maimai MiLK",
                "MiLK PLUS",
                "maimai FiNALE"
            ];
            plateVersionIndex = 12;
        }
        else if (command.StartsWith("真"))
        {
            version =
            [
                "maimai", "maimai PLUS"
            ];
        }

        if (version == null)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("未找到该姓名框")
            ]);
            return;
        }

        using var songDb = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;
        var songMetaDb = songDb.SongMetaDataSet;

        ScoresDto scoresInReality;

        var useAvatar = true;

        if (!cachedInLapis)
        {
            try
            {
                if (arguments.Length > 1)
                {
                    var isGroupMember =
                        GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[1],
                            out var groupMembers, source) && groupMembers.Length == 1;
                    var isQqId = long.TryParse(arguments[1], out _);
                    var scoresInRealityContent = isGroupMember
                        ? ApiOperator.Instance.Post(
                            BotConfiguration.Instance.DivingFishUrl,
                            "api/maimaidxprober/query/plate",
                            new { qq = groupMembers[0].QqId.ToString(), version },
                            [
                                new KeyValuePair<string, string>("Developer-Token",
                                    BotConfiguration.Instance.DivingFishDevToken)
                            ])
                        : isQqId
                            ? ApiOperator.Instance.Post(
                                BotConfiguration.Instance.DivingFishUrl,
                                "api/maimaidxprober/query/plate",
                                new { qq = arguments[1], version },
                                [
                                    new KeyValuePair<string, string>("Developer-Token",
                                        BotConfiguration.Instance.DivingFishDevToken)
                                ])
                            : ApiOperator.Instance.Post(
                                BotConfiguration.Instance.DivingFishUrl,
                                "api/maimaidxprober/query/plate",
                                new { username = arguments[1], version },
                                [
                                    new KeyValuePair<string, string>("Developer-Token",
                                        BotConfiguration.Instance.DivingFishDevToken)
                                ]);

                    if (scoresInRealityContent.StatusCode != HttpStatusCode.OK)
                        throw new HttpRequestException($"Unexpected status code: {scoresInRealityContent.StatusCode}",
                            null,
                            scoresInRealityContent.StatusCode);

                    scoresInReality = JsonConvert.DeserializeObject<ScoresDto>(scoresInRealityContent.Result);

                    useAvatar = false;
                }
                else
                {
                    var scoresInRealityContent = ApiOperator.Instance.Post(
                        BotConfiguration.Instance.DivingFishUrl,
                        "api/maimaidxprober/query/plate",
                        new { qq = source.Sender.UserId.ToString(), version },
                        [
                            new KeyValuePair<string, string>("Developer-Token",
                                BotConfiguration.Instance.DivingFishDevToken)
                        ]);

                    if (scoresInRealityContent.StatusCode != HttpStatusCode.OK)
                        throw new HttpRequestException($"Unexpected status code: {scoresInRealityContent.StatusCode}",
                            null,
                            scoresInRealityContent.StatusCode);

                    scoresInReality = JsonConvert.DeserializeObject<ScoresDto>(scoresInRealityContent.Result);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskCanceledException or HttpRequestException)
                {
                    DivingFishErrorHelp(source);
                    return;
                }

                if (arguments.Length > 1)
                    ObjectUserUnboundErrorHelp(source);
                else
                    UnboundErrorHelp(source);
                return;
            }

            if (scoresInReality.ScoreDtos == null)
            {
                if (arguments.Length > 1)
                    ObjectUserUnboundErrorHelp(source);
                else
                    UnboundErrorHelp(source);
                return;
            }
        }
        else
        {
            scoresInReality = GetScoresFromLapis(version, source.Sender.UserId);
            useAvatar = arguments.Length == 1;
        }

        var songsToBeDisplayed = new List<SongToBeDisplayed>();

        var songs = songMetaDb.Include(x => x.Charts)
            .Where(x => version.Any(y => x.Version == y)).ToArray();

        foreach (var song in songs)
        {
            var charts = song.Charts;
            foreach (var chart in charts)
                if (Math.Round(chart.Rating, 1) > 13.5)
                {
                    var songToBeDisplayed = GetSongToBeDisplayed(command, chart, scoresInReality, song);
                    if (songToBeDisplayed != null)
                        songsToBeDisplayed.Add(songToBeDisplayed);
                }
        }

        var allSongs = new List<SongToBeDisplayed>();

        foreach (var song in songs)
        foreach (var chart in song.Charts)
        {
            var songToBeDisplayed = GetSongToBeDisplayed(command, chart, scoresInReality, song);
            if (songToBeDisplayed != null)
                allSongs.Add(songToBeDisplayed);
        }

        PlateCategories category;

        if (jiRegex.IsMatch(command))
        {
            category = PlateCategories.Ji;
        }
        else if (jiangRegex.IsMatch(command))
        {
            category = PlateCategories.Jiang;
        }
        else if (shenRegex.IsMatch(command))
        {
            category = PlateCategories.Shen;
        }
        else if (wuwuRegex.IsMatch(command))
        {
            category = PlateCategories.Wuwu;
        }
        else if (bazheRegex.IsMatch(command))
        {
            category = PlateCategories.Bazhe;
        }
        else
        {
            SendMessage(
                source,
                [new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该姓名框")]
            );

            return;
        }

        var isCompressed =
            SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

        try
        {
            var image = new PlateImageGenerator().Generate(songsToBeDisplayed, allSongs, userName,
                MaiCommandInstance,
                category, source.Sender.UserId.ToString(), useAvatar, plateVersionIndex,
                isCompressed);

            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqImageMsg("base64://" + image)
            ]);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is TaskCanceledException or HttpRequestException)
            {
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    "获取头像时出现错误"
                ]);
                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
        }
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("lite_command", "1"), source.GroupId))
            return;

        var originalCommandString = command;

        if (command.EndsWith(" 进度"))
            command = command.Replace(" 进度", "");
        else if (command.EndsWith(" 完成表"))
            command = command.Replace(" 完成表", "");
        else if (command.EndsWith("进度"))
            command = command.Replace("进度", "");
        else if (command.EndsWith("完成表"))
            command = command.Replace("完成表", "");
        else
            return;

        ParseWithArgument([command], originalCommandString, source);
    }

    private SongToBeDisplayed GetSongToBeDisplayed(string command, ChartMetaData chart, ScoresDto scoresInReality,
        SongMetaData song)
    {
        var scoreDto = new ScoresDto.ScoreDto();
        foreach (var realScore in scoresInReality.ScoreDtos)
            if (chart.SongId == realScore.Id && chart.LevelIndex == realScore.LevelIndex)
                scoreDto = realScore;

        if (_excludedSongs.Contains(chart.SongId))
            if (!((command == "霸者" || command.StartsWith("舞")) && chart.SongId == 70))
                return null;

        if (chart.SongId >= 100000)
            return null;

        if (command == "霸者" || command.StartsWith("舞"))
        {
            if (chart.LevelIndex == 4 && _includedRemasterSongs.Contains(chart.SongId))
                return new SongToBeDisplayed
                {
                    LevelIndex = chart.LevelIndex, Song = song,
                    ScoreDto = scoreDto
                };
            if (chart.LevelIndex != 4)
                return new SongToBeDisplayed
                {
                    LevelIndex = chart.LevelIndex, Song = song,
                    ScoreDto = scoreDto
                };
        }
        else if (chart.LevelIndex != 4)
        {
            return new SongToBeDisplayed
            {
                LevelIndex = chart.LevelIndex, Song = song, ScoreDto = scoreDto
            };
        }

        return null;
    }

    private ScoresDto GetScoresFromLapis(string[] versions, long qqId)
    {
        var scoresFromLapis = MaiScoreOperator.GetScoreByVersionFromLapis(versions, qqId);
        var result = new ScoresDto
        {
            ScoreDtos = scoresFromLapis.Select(x => new ScoresDto.ScoreDto
            {
                Achievements = x.Achievements,
                Fc = x.Fc,
                Fs = x.Fs,
                Id = x.SongId,
                LevelIndex = x.LevelIndex
            }).ToArray()
        };

        return result;
    }

    public class UsernameDto
    {
        [JsonProperty("username")] public string Username;
    }

    public class SongToBeDisplayed
    {
        public int LevelIndex;
        public ScoresDto.ScoreDto ScoreDto;
        public SongMetaData Song;
    }
}