using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.ImageGenerators;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class BestCommand : MaiCommandBase
{
    public BestCommand()
    {
        CommandHead = "b50";
        DirectCommandHead = "b50|逼五零";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("b50", "1");
        IntendedArgumentCount = 1;
    }

    private void TagRatingByScore(BestDto.ScoreDto[] scores)
    {
        foreach (var score in scores)
        {
            score.Rate = GetRate(score.Achievements);
            score.MaxDxScore = MaiCommandInstance.GetSong(score.Id)
                .Charts[score.LevelIndex].MaxDxScore;
        }
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        var isGroupMember =
            GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[0], source.GroupId,
                out var groupMembers) && groupMembers.Length == 1;

        var isQqId = long.TryParse(arguments[0], out _);

        try
        {
            if (isGroupMember)
            {
                var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/player",
                    new
                    {
                        qq = groupMembers[0].Id.ToString(),
                        b50 = true
                    });

                var best = JsonConvert.DeserializeObject<BestDto>(content);

                if (best.Charts == null)
                {
                    ObjectUserUnboundErrorHelp(source);
                    return;
                }

                Process(source, best, false);
            }
            else if (isQqId)
            {
                var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/player",
                    new
                    {
                        qq = arguments[0],
                        b50 = true
                    });

                var best = JsonConvert.DeserializeObject<BestDto>(content);

                if (best.Charts == null)
                {
                    ObjectUserUnboundErrorHelp(source);
                    return;
                }

                Process(source, best, false);
            }
            else
            {
                var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/player",
                    new
                    {
                        username = arguments[0],
                        b50 = true
                    });

                var best = JsonConvert.DeserializeObject<BestDto>(content);

                if (best.Charts == null)
                {
                    ObjectUserUnboundErrorHelp(source);
                    return;
                }

                Process(source, best, false);
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException is TaskCanceledException or HttpRequestException)
            {
                DivingFishErrorHelp(source);
                return;
            }

            if (ex is HttpRequestException httpRequestException)
            {
                switch (httpRequestException.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        ObjectUserUnboundErrorHelp(source);
                        break;
                    case HttpStatusCode.Forbidden:
                        ForbiddenErrorHelp(source);
                        break;
                    default:
                        HelpCommand.Instance.UnexpectedErrorHelp(source);
                        break;
                }
                
                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
        }
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        try
        {
            var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/query/player",
                new
                {
                    qq = source.Sender.UserId.ToString(),
                    b50 = true
                });

            var best = JsonConvert.DeserializeObject<BestDto>(content);

            if (best.Charts == null)
            {
                UnboundErrorHelp(source);
                return;
            }

            Process(source, best, true);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is TaskCanceledException or HttpRequestException)
            {
                DivingFishErrorHelp(source);
                return;
            }
            
            if (ex is HttpRequestException httpRequestException)
            {
                switch (httpRequestException.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        UnboundErrorHelp(source);
                        break;
                    case HttpStatusCode.Forbidden:
                        ForbiddenErrorHelp(source);
                        break;
                    default:
                        HelpCommand.Instance.UnexpectedErrorHelp(source);
                        break;
                }

                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
        }
    }

    private void Process(CqGroupMessagePostContext source, BestDto best, bool useHead)
    {
        TagRatingByScore(best.Charts.SdCharts);
        TagRatingByScore(best.Charts.DxCharts);

        try
        {
            var image = new BestImageGenerator().Generate(best, source.Sender.UserId.ToString(), useHead,
                SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId));

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
}

public class BestDto
{
    [JsonProperty("charts")] public ChartsDto Charts;

    [JsonProperty("rating")] public int Rating;
    [JsonProperty("username")] public string Username;

    public class ChartsDto
    {
        [JsonProperty("dx")] public ScoreDto[] DxCharts;

        [JsonProperty("sd")] public ScoreDto[] SdCharts;
    }

    public class ScoreDto
    {
        [JsonProperty("achievements")] public float Achievements;
        [JsonProperty("ds")] public float DifficultyFactor;

        [JsonProperty("dxScore")] public int DxScore;

        [JsonProperty("fc")] public string Fc;

        [JsonProperty("fs")] public string Fs;

        [JsonProperty("song_id")] public int Id;

        [JsonProperty("level_index")] public int LevelIndex;

        public int MaxDxScore;

        public MaiCommandBase.Rate Rate;

        [JsonProperty("ra")] public float Rating;

        [JsonProperty("title")] public string Title;

        [JsonProperty("type")] public string Type;
    }
}