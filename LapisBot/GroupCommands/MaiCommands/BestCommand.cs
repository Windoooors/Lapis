using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.ImageGenerators;
using LapisBot.Operations.ApiOperation;
using LapisBot.Settings;
using LapisBot.UniversalCommands;
using Newtonsoft.Json;

namespace LapisBot.GroupCommands.MaiCommands;

public class BestCommand : MaiCommandBase
{
    public BestCommand()
    {
        CommandHead = new Regex("^b50");
        DirectCommandHead = new Regex("^b50|^逼五零");
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

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        var content = string.Empty;
        BestDto best;

        try
        {
            content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/query/player",
                new { username = command, b50 = true });
        }
        catch (Exception e)
        {
            if (e is TaskCanceledException or HttpRequestException)
            {
                DivingFishErrorHelp(source);
                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
            return;
        }

        best = JsonConvert.DeserializeObject<BestDto>(content);

        if (best.Charts == null)
            try
            {
                content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/player",
                    new { qq = command, b50 = true });
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException or HttpRequestException)
                {
                    DivingFishErrorHelp(source);
                    return;
                }

                HelpCommand.Instance.UnexpectedErrorHelp(source);
                return;
            }

        best = JsonConvert.DeserializeObject<BestDto>(content);

        if (best.Charts == null)
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("未找到该玩家")
            ]);
            return;
        }

        TagRatingByScore(best.Charts.SdCharts);
        TagRatingByScore(best.Charts.DxCharts);

        var isCompressed =
            SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

        var image = new BestImageGenerator().Generate(best, source.Sender.UserId.ToString(), false,
            isCompressed);

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + image)
        ]);
    }

    public override void Parse(CqGroupMessagePostContext source)
    {
        string content;
        try
        {
            content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/query/player",
                new { qq = source.Sender.UserId.ToString(), b50 = true });
        }
        catch (Exception ex)
        {
            if (ex.InnerException is TaskCanceledException or HttpRequestException)
            {
                DivingFishErrorHelp(source);
                return;
            }

            UnboundErrorHelp(source);
            return;
        }

        var best = JsonConvert.DeserializeObject<BestDto>(content);
        TagRatingByScore(best.Charts.SdCharts);
        TagRatingByScore(best.Charts.DxCharts);

        try
        {
            var image = new BestImageGenerator().Generate(best, source.Sender.UserId.ToString(), true,
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