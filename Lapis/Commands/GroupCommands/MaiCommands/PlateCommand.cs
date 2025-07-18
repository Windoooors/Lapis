﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.ImageGenerators;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;
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

    private readonly Dictionary<string, string> _plateToVersion = new()
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
        { "双", "maimai でらっくす BUDDiES" },
        { "宴", "maimai でらっくす BUDDiES" },
        { "镜", "maimai でらっくす PRiSM" }
    };

    public Dictionary<string, string> Categories = new()
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

    public Dictionary<string, string> Characters = new()
    {
        { "晓", "暁" },
        { "樱", "櫻" },
        { "堇", "菫" },
        { "辉", "輝" },
        { "华", "華" }
    };

    public PlateCommand()
    {
        CommandHead = "plate";
        DirectCommandHead = "plate|牌子";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("plate", "1");
        IntendedArgumentCount = 2;
    }

    public override void ParseWithArgument(string[] arguments, CqGroupMessagePostContext source)
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
        try
        {
            string content;

            if (arguments.Length > 1)
            {
                var isGroupMember =
                    GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[1], source.GroupId,
                        out var groupMembers) && groupMembers.Length == 1;
                var isQqId = long.TryParse(arguments[1], out _);
                content = isGroupMember
                    ? ApiOperator.Instance.Post(
                        BotConfiguration.Instance.DivingFishUrl,
                        "api/maimaidxprober/query/player",
                        new { qq = groupMembers[0].Id.ToString() })
                    : isQqId
                        ? ApiOperator.Instance.Post(
                            BotConfiguration.Instance.DivingFishUrl,
                            "api/maimaidxprober/query/player",
                            new { qq = arguments[1] })
                        : ApiOperator.Instance.Post(
                            BotConfiguration.Instance.DivingFishUrl,
                            "api/maimaidxprober/query/player",
                            new { username = arguments[1] });
            }
            else
            {
                content = ApiOperator.Instance.Post(
                    BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/player",
                    new { qq = source.Sender.UserId });
            }

            userName = JsonConvert.DeserializeObject<BestDto>(content).Username;
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

        var versionCharacter =
            wuwuRegex.Replace(shenRegex.Replace(jiangRegex.Replace(jiRegex.Replace(command, "", 1), "", 1), "", 1), "",
                1);
        ;

        Characters.TryGetValue(versionCharacter, out var versionCharacterInJapanese);

        if (versionCharacterInJapanese != null)
            versionCharacter = versionCharacterInJapanese;

        var singleVersionFound = _plateToVersion.TryGetValue(versionCharacter, out var singleVersion);

        string[] version = singleVersionFound ? [singleVersion] : null;

        var plateVersionIndex = _plateToVersion.Keys.ToList().IndexOf(versionCharacter);

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

        ScoresDto scores;
        if (!(command == "霸者" || command.StartsWith("舞")))
        {
            string content;
            try
            {
                content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/plate",
                    new { username = "maxscore", version });
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

            scores = JsonConvert.DeserializeObject<ScoresDto>(content);
        }
        else
        {
            var list = new List<ScoresDto.ScoreDto>();
            foreach (var song in MaiCommandInstance.Songs)
                if (song.Id < 1000)
                    for (var i = 0; i < song.Ratings.Length; i++)
                        list.Add(new ScoresDto.ScoreDto
                            { Id = song.Id, LevelIndex = i });

            scores = new ScoresDto { ScoreDtos = list.ToArray() };
        }

        ScoresDto scoresInReality;

        var useAvatar = true;

        try
        {
            if (arguments.Length > 1)
            {
                var isGroupMember =
                    GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[1], source.GroupId,
                        out var groupMembers) && groupMembers.Length == 1;
                var isQqId = long.TryParse(arguments[1], out _);
                scoresInReality = isGroupMember
                    ? JsonConvert.DeserializeObject<ScoresDto>(ApiOperator.Instance.Post(
                        BotConfiguration.Instance.DivingFishUrl,
                        "api/maimaidxprober/query/plate",
                        new { qq = groupMembers[0].Id.ToString(), version }))
                    : isQqId
                        ? JsonConvert.DeserializeObject<ScoresDto>(ApiOperator.Instance.Post(
                            BotConfiguration.Instance.DivingFishUrl,
                            "api/maimaidxprober/query/plate",
                            new { qq = arguments[1], version }))
                        : JsonConvert.DeserializeObject<ScoresDto>(ApiOperator.Instance.Post(
                            BotConfiguration.Instance.DivingFishUrl,
                            "api/maimaidxprober/query/plate",
                            new { username = arguments[1], version }));
                useAvatar = false;
            }
            else
            {
                scoresInReality = JsonConvert.DeserializeObject<ScoresDto>(ApiOperator.Instance.Post(
                    BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/plate",
                    new { qq = source.Sender.UserId, version }));
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

        var songsToBeDisplayed = new List<SongToBeDisplayed>();

        foreach (var score in scores.ScoreDtos)
        {
            var song = MaiCommandInstance.GetSong(score.Id);
            if (Math.Round(song.Ratings[score.LevelIndex], 1) > 13.5f)
            {
                var scoreDto = new ScoresDto.ScoreDto();
                foreach (var realScore in scoresInReality.ScoreDtos)
                    if (score.Id == realScore.Id && score.LevelIndex == realScore.LevelIndex)
                        scoreDto = realScore;

                if (_excludedSongs.Contains(song.Id))
                    if (!((command == "霸者" || command.StartsWith("舞")) && song.Id == 70))
                        continue;

                if (song.Id >= 100000)
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

        foreach (var score in scores.ScoreDtos)
        {
            var song = MaiCommandInstance.GetSong(score.Id);
            var scoreDto = new ScoresDto.ScoreDto();
            foreach (var realScore in scoresInReality.ScoreDtos)
                if (score.Id == realScore.Id && score.LevelIndex == realScore.LevelIndex)
                    scoreDto = realScore;

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
            {
                allSongs.Add(new SongToBeDisplayed
                    { LevelIndex = score.LevelIndex, SongDto = song, ScoreDto = scoreDto });
            }
        }

        var category = PlateCategories.Ji;

        if (jiRegex.IsMatch(command))
            category = PlateCategories.Ji;
        if (jiangRegex.IsMatch(command))
            category = PlateCategories.Jiang;
        if (shenRegex.IsMatch(command))
            category = PlateCategories.Shen;
        if (wuwuRegex.IsMatch(command))
            category = PlateCategories.Wuwu;
        if (bazheRegex.IsMatch(command))
            category = PlateCategories.Bazhe;

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
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

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

        ParseWithArgument([command], source);
    }

    public class UsernameDto
    {
        [JsonProperty("username")] public string Username;
    }

    public class SongToBeDisplayed
    {
        public int LevelIndex;
        public ScoresDto.ScoreDto ScoreDto;
        public SongDto SongDto;
    }
}