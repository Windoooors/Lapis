using System;
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

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source, long[] mentionedUserIds)
    {
        if (command == "" && mentionedUserIds.Length != 0)
            Parse(source, mentionedUserIds);
    }

    public override void Parse(CqGroupMessagePostContext source, long[] mentionedUserIds)
    {
        string content;
        try
        {
            content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/query/player",
                new
                {
                    qq = mentionedUserIds.Length == 0
                        ? source.Sender.UserId.ToString()
                        : mentionedUserIds[0].ToString(),
                    b50 = true
                });
        }
        catch (Exception ex)
        {
            if (ex.InnerException is TaskCanceledException or HttpRequestException)
            {
                DivingFishErrorHelp(source);
                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
            return;
        }

        var best = JsonConvert.DeserializeObject<BestDto>(content);

        if (best.Charts == null)
        {
            UnboundErrorHelp(source);
            return;
        }

        TagRatingByScore(best.Charts.SdCharts);
        TagRatingByScore(best.Charts.DxCharts);

        try
        {
            var image = new BestImageGenerator().Generate(best, mentionedUserIds.Length == 0
                    ? source.Sender.UserId.ToString()
                    : mentionedUserIds[0].ToString(), true,
                true);

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