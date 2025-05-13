using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.ImageGenerators;
using LapisBot_Renewed.Operations.ApiOperation;
using LapisBot_Renewed.Settings;
using Microsoft.Extensions.Logging;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
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
            foreach (BestDto.ScoreDto score in scores)
            {
                score.rate = GetRate(score.Achievements);
                score.MaxDxScore = MaiCommandInstance.GetSong(score.Id)
                    .Charts[score.LevelIndex].MaxDxScore;
            }
        }

        public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
        {
            var content = string.Empty;
            BestDto best;

            content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl, "api/maimaidxprober/query/player",
                new { username = command, b50 = true });
            best = JsonConvert.DeserializeObject<BestDto>(content);

            if (best.Charts == null)
                content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/query/player",
                    new { qq = command, b50 = true });
            best = JsonConvert.DeserializeObject<BestDto>(content);

            if (best.Charts == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("未找到该玩家")
                    });
                return Task.CompletedTask;
            }
            
            TagRatingByScore(best.Charts.SdCharts);
            TagRatingByScore(best.Charts.DxCharts);
            
            var isCompressed =
                SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

            var image = new BestImageGenerator().Generate(best, source.Sender.UserId.ToString(), false,
                isCompressed);
            
            Program.Session.SendGroupMessageAsync(source.GroupId,
                new CqMessage
                {
                    new CqReplyMsg(source.MessageId),
                    new CqImageMsg("base64://" + image)
                });

            return Task.CompletedTask;
        }

        public override Task Parse(CqGroupMessagePostContext source)
        {
            try
            {
                var content = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl, "api/maimaidxprober/query/player",
                    new { qq = source.Sender.UserId.ToString(), b50 = true });
                BestDto best = JsonConvert.DeserializeObject<BestDto>(content);
                TagRatingByScore(best.Charts.SdCharts);
                TagRatingByScore(best.Charts.DxCharts);
                
                var image = new BestImageGenerator().Generate(best, source.Sender.UserId.ToString(), true,
                    true);

                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqImageMsg("base64://" + image)
                    });
            }
            catch (Exception ex)
            {
                Program.Logger.LogError(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace);
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("您没有绑定“舞萌 DX | 中二节奏查分器”账户，请前往 https://www.diving-fish.com/maimaidx/prober 进行绑定")
                    });
            }

            return Task.CompletedTask;
        }
    }
    
    public class BestDto
    {
        [JsonProperty("username")] public string Username;

        [JsonProperty("rating")] public int Rating;

        [JsonProperty("charts")] public ChartsDto Charts;

        public class ChartsDto
        {
            [JsonProperty("dx")] public ScoreDto[] DxCharts;

            [JsonProperty("sd")] public ScoreDto[] SdCharts;
        }

        public class ScoreDto
        {
            [JsonProperty("ds")] public float DifficultyFactor;

            [JsonProperty("ra")] public float Rating;

            [JsonProperty("achievements")] public float Achievements;

            [JsonProperty("fc")] public string Fc;

            [JsonProperty("title")] public string Title;

            [JsonProperty("fs")] public string Fs;

            [JsonProperty("song_id")] public int Id;

            [JsonProperty("type")] public string Type;

            [JsonProperty("level_index")] public int LevelIndex;

            [JsonProperty("dxScore")] public int DxScore;

            public int MaxDxScore;

            public InfoCommand.Rate rate;
        }
    }
}
