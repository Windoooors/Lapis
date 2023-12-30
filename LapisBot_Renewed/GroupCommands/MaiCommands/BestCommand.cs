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
using System.Threading.Tasks;
using System.IO;

namespace LapisBot_Renewed
{
    public class BestCommand : MaiCommand
    {
        public override Task Initialize()
        {
            headCommand = new Regex(@"^b50$");
            subHeadCommand = new Regex(@"^b50\s");
            directCommand = new Regex(@"^b50$|^逼五零$");
            subDirectCommand = new Regex(@"^b50\s|^逼五零\s");
            defaultSettings.SettingsName = "Best 50";
            _groupCommandSettings = defaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings");

            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source, bool isSubParse)
        {
            if (isSubParse)
            {
                try
                {
                    var content = Program.apiOperator.Post("api/maimaidxprober/query/player", new { username = command, b50 = true });
                    //MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" Best 50 生成需要较长时间，请耐心等待") });
                    BestDto best = JsonConvert.DeserializeObject<BestDto>(content);
                    Program.settingsCommand.GetSettings(source);
                    var image = BestImageGenerator.Generate(best, source.Sender.Id, false, Program.settingsCommand.CurrentBotSettings.CompressedImage);
                    //image.Write(Environment.CurrentDirectory + @"/temp/b50.png");
                    var _image = new ImageMessage
                    {
                        Base64 = image
                    };

                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
                }
                catch
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该玩家") });
                }
            }
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            try
            {
                var content = Program.apiOperator.Post("api/maimaidxprober/query/player", new { qq = source.Sender.Id, b50 = true });
                //MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" Best 50 生成需要较长时间，请耐心等待") });

                BestDto best = JsonConvert.DeserializeObject<BestDto>(content);
                foreach (BestDto.ScoreDto score in best.Charts.SdCharts)
                {
                    var achievement = score.Achievements;
                    if (achievement >= 100.5)
                        score.rate = Rate.SSSp;
                    else if (100.5 > achievement && achievement >= 100)
                        score.rate = Rate.SSS;
                    else if (100 > achievement && achievement >= 99.5)
                        score.rate = Rate.SSp;
                    else if (99.5 > achievement && achievement >= 99)
                        score.rate = Rate.SS;
                    else if (99 > achievement && achievement >= 98)
                        score.rate = Rate.Sp;
                    else if (98 > achievement && achievement >= 97)
                        score.rate = Rate.S;
                    else if (97 > achievement && achievement >= 94)
                        score.rate = Rate.AAA;
                    else if (94 > achievement && achievement >= 90)
                        score.rate = Rate.AA;
                    else if (90 > achievement && achievement >= 80)
                        score.rate = Rate.A;
                    else if (80 > achievement && achievement >= 75)
                        score.rate = Rate.BBB;
                    else if (75 > achievement && achievement >= 70)
                        score.rate = Rate.BB;
                    else if (70 > achievement && achievement >= 60)
                        score.rate = Rate.B;
                    else if (60 > achievement && achievement >= 50)
                        score.rate = Rate.C;
                    else if (50 > achievement)
                        score.rate = Rate.D;
                }
                foreach (BestDto.ScoreDto score in best.Charts.DxCharts)
                {
                    var achievement = score.Achievements;
                    if (achievement >= 100.5)
                        score.rate = Rate.SSSp;
                    else if (100.5 > achievement && achievement >= 100)
                        score.rate = Rate.SSS;
                    else if (100 > achievement && achievement >= 99.5)
                        score.rate = Rate.SSp;
                    else if (99.5 > achievement && achievement >= 99)
                        score.rate = Rate.SS;
                    else if (99 > achievement && achievement >= 98)
                        score.rate = Rate.Sp;
                    else if (98 > achievement && achievement >= 97)
                        score.rate = Rate.S;
                    else if (97 > achievement && achievement >= 94)
                        score.rate = Rate.AAA;
                    else if (94 > achievement && achievement >= 90)
                        score.rate = Rate.AA;
                    else if (90 > achievement && achievement >= 80)
                        score.rate = Rate.A;
                    else if (80 > achievement && achievement >= 75)
                        score.rate = Rate.BBB;
                    else if (75 > achievement && achievement >= 70)
                        score.rate = Rate.BB;
                    else if (70 > achievement && achievement >= 60)
                        score.rate = Rate.B;
                    else if (60 > achievement && achievement >= 50)
                        score.rate = Rate.C;
                    else if (50 > achievement)
                        score.rate = Rate.D;
                }
                
                Program.settingsCommand.GetSettings(source);
                
                var image = BestImageGenerator.Generate(best, source.Sender.Id, true, Program.settingsCommand.CurrentBotSettings.CompressedImage);
                //image.Write(Environment.CurrentDirectory + @"/temp/b50.png");
                var _image = new ImageMessage
                {
                    Base64 = image
                };

                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
            }
            catch(Exception ex)
            {
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
            }
            return Task.CompletedTask;
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

            public InfoCommand.Rate rate;
        }
    }
}
