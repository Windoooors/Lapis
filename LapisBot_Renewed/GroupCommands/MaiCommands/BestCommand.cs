using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Manganese.Text;
using ImageMagick;
using System;
using static LapisBot_Renewed.InfoCommand.GetScore;
using System.Reflection;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using static LapisBot_Renewed.InfoCommand;

namespace LapisBot_Renewed
{
    public class BestCommand : MaiCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"^b");
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            if (command == "50")
            {
                try
                {
                    var content = Program.apiOperator.Post("api/maimaidxprober/query/player", new { qq = source.Sender.Id, b50 = true });
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" Best 50 生成需要较长时间，请耐心等待") });
                    BestDto best = JsonConvert.DeserializeObject<BestDto>(content);
                    var image = BestImageGenerator.Generate(best, source.Sender.Id);
                    image.Write(Environment.CurrentDirectory + @"/temp/b50.png");
                    var _image = new ImageMessage
                    {
                        Path = Environment.CurrentDirectory + @"/temp/b50.png"
                    };
                    
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
                }
                catch
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
                }
            }
        }
    }

    public class BestDto
    {
        [JsonProperty("username")]
        public string Username;

        [JsonProperty("rating")]
        public int Rating;

        [JsonProperty("charts")]
        public ChartsDto Charts;

        public class ChartsDto
        {
            [JsonProperty("dx")]
            public ScoreDto[] DxCharts;

            [JsonProperty("sd")]
            public ScoreDto[] SdCharts;
        }

        public class ScoreDto
        {
            [JsonProperty("ds")]
            public float DifficultyFactor;

            [JsonProperty("ra")]
            public float Rating;

            [JsonProperty("achievements")]
            public float Achievements;

            [JsonProperty("fc")]
            public string Fc;

            [JsonProperty("title")]
            public string Title;

            [JsonProperty("fsd")]
            public string Fsd;

            [JsonProperty("song_id")]
            public int Id;

            [JsonProperty("type")]
            public string Type;

            [JsonProperty("level_index")]
            public int LevelIndex;
        }
    }
}
