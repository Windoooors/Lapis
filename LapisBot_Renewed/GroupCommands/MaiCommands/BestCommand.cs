using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using System.Threading.Tasks;
using System.IO;
using LapisBot_Renewed.ImageGenerators;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class BestCommand : MaiCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^b50$");
            SubHeadCommand = new Regex(@"^b50\s");
            DirectCommand = new Regex(@"^b50$|^逼五零$");
            SubDirectCommand = new Regex(@"^b50\s|^逼五零\s");
            DefaultSettings.SettingsName = "Best 50";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");

            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        public override Task SubParse(string command, GroupMessageReceiver source)
        {
            var content = string.Empty;
            BestDto best;
            
            content = Program.apiOperator.Post("api/maimaidxprober/query/player",
                new { username = command, b50 = true });
            best = JsonConvert.DeserializeObject<BestDto>(content);

            if (best.Charts == null)
                content = Program.apiOperator.Post("api/maimaidxprober/query/player",
                    new { qq = command, b50 = true });
            best = JsonConvert.DeserializeObject<BestDto>(content);

            if (best.Charts == null)
            {
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该玩家") });
                return Task.CompletedTask;
            }

            //MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" Best 50 生成需要较长时间，请耐心等待") });

            foreach (BestDto.ScoreDto score in best.Charts.SdCharts)
            {
                var achievement = score.Achievements;
                if (achievement >= 100.5)
                    score.rate = InfoCommand.Rate.Sssp;
                else if (100.5 > achievement && achievement >= 100)
                    score.rate = InfoCommand.Rate.Sss;
                else if (100 > achievement && achievement >= 99.5)
                    score.rate = InfoCommand.Rate.Ssp;
                else if (99.5 > achievement && achievement >= 99)
                    score.rate = InfoCommand.Rate.Ss;
                else if (99 > achievement && achievement >= 98)
                    score.rate = InfoCommand.Rate.Sp;
                else if (98 > achievement && achievement >= 97)
                    score.rate = InfoCommand.Rate.S;
                else if (97 > achievement && achievement >= 94)
                    score.rate = InfoCommand.Rate.Aaa;
                else if (94 > achievement && achievement >= 90)
                    score.rate = InfoCommand.Rate.Aa;
                else if (90 > achievement && achievement >= 80)
                    score.rate = InfoCommand.Rate.A;
                else if (80 > achievement && achievement >= 75)
                    score.rate = InfoCommand.Rate.Bbb;
                else if (75 > achievement && achievement >= 70)
                    score.rate = InfoCommand.Rate.Bb;
                else if (70 > achievement && achievement >= 60)
                    score.rate = InfoCommand.Rate.B;
                else if (60 > achievement && achievement >= 50)
                    score.rate = InfoCommand.Rate.C;
                else if (50 > achievement)
                    score.rate = InfoCommand.Rate.D;
                score.MaxDxScore = MaiCommandCommand.GetSong(score.Id)
                    .Charts[score.LevelIndex].MaxDxScore;
            }

            foreach (BestDto.ScoreDto score in best.Charts.DxCharts)
            {
                var achievement = score.Achievements;
                if (achievement >= 100.5)
                    score.rate = InfoCommand.Rate.Sssp;
                else if (100.5 > achievement && achievement >= 100)
                    score.rate = InfoCommand.Rate.Sss;
                else if (100 > achievement && achievement >= 99.5)
                    score.rate = InfoCommand.Rate.Ssp;
                else if (99.5 > achievement && achievement >= 99)
                    score.rate = InfoCommand.Rate.Ss;
                else if (99 > achievement && achievement >= 98)
                    score.rate = InfoCommand.Rate.Sp;
                else if (98 > achievement && achievement >= 97)
                    score.rate = InfoCommand.Rate.S;
                else if (97 > achievement && achievement >= 94)
                    score.rate = InfoCommand.Rate.Aaa;
                else if (94 > achievement && achievement >= 90)
                    score.rate = InfoCommand.Rate.Aa;
                else if (90 > achievement && achievement >= 80)
                    score.rate = InfoCommand.Rate.A;
                else if (80 > achievement && achievement >= 75)
                    score.rate = InfoCommand.Rate.Bbb;
                else if (75 > achievement && achievement >= 70)
                    score.rate = InfoCommand.Rate.Bb;
                else if (70 > achievement && achievement >= 60)
                    score.rate = InfoCommand.Rate.B;
                else if (60 > achievement && achievement >= 50)
                    score.rate = InfoCommand.Rate.C;
                else if (50 > achievement)
                    score.rate = InfoCommand.Rate.D;
                score.MaxDxScore = MaiCommandCommand.GetSong(score.Id)
                    .Charts[score.LevelIndex].MaxDxScore;
            }

            Program.settingsCommand.GetSettings(source);
            var image = new BestImageGenerator().Generate(best, source.Sender.Id, false,
                Program.settingsCommand.CurrentBotSettings.CompressedImage);
            //image.Write(Environment.CurrentDirectory + @"/temp/b50.png");
            var imageMessage = new ImageMessage
            {
                Base64 = image
            };

            MessageManager.SendGroupMessageAsync(source.GroupId,
                new MessageChain() { new AtMessage(source.Sender.Id), imageMessage });

            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            try
            {
                var content = Program.apiOperator.Post("api/maimaidxprober/query/player",
                    new { qq = source.Sender.Id, b50 = true });
                //MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" Best 50 生成需要较长时间，请耐心等待") });

                BestDto best = JsonConvert.DeserializeObject<BestDto>(content);
                foreach (BestDto.ScoreDto score in best.Charts.SdCharts)
                {
                    var achievement = score.Achievements;
                    if (achievement >= 100.5)
                        score.rate = InfoCommand.Rate.Sssp;
                    else if (100.5 > achievement && achievement >= 100)
                        score.rate = InfoCommand.Rate.Sss;
                    else if (100 > achievement && achievement >= 99.5)
                        score.rate = InfoCommand.Rate.Ssp;
                    else if (99.5 > achievement && achievement >= 99)
                        score.rate = InfoCommand.Rate.Ss;
                    else if (99 > achievement && achievement >= 98)
                        score.rate = InfoCommand.Rate.Sp;
                    else if (98 > achievement && achievement >= 97)
                        score.rate = InfoCommand.Rate.S;
                    else if (97 > achievement && achievement >= 94)
                        score.rate = InfoCommand.Rate.Aaa;
                    else if (94 > achievement && achievement >= 90)
                        score.rate = InfoCommand.Rate.Aa;
                    else if (90 > achievement && achievement >= 80)
                        score.rate = InfoCommand.Rate.A;
                    else if (80 > achievement && achievement >= 75)
                        score.rate = InfoCommand.Rate.Bbb;
                    else if (75 > achievement && achievement >= 70)
                        score.rate = InfoCommand.Rate.Bb;
                    else if (70 > achievement && achievement >= 60)
                        score.rate = InfoCommand.Rate.B;
                    else if (60 > achievement && achievement >= 50)
                        score.rate = InfoCommand.Rate.C;
                    else if (50 > achievement)
                        score.rate = InfoCommand.Rate.D;
                    score.MaxDxScore = MaiCommandCommand.GetSong(score.Id)
                        .Charts[score.LevelIndex].MaxDxScore;
                }

                foreach (BestDto.ScoreDto score in best.Charts.DxCharts)
                {
                    var achievement = score.Achievements;
                    if (achievement >= 100.5)
                        score.rate = InfoCommand.Rate.Sssp;
                    else if (100.5 > achievement && achievement >= 100)
                        score.rate = InfoCommand.Rate.Sss;
                    else if (100 > achievement && achievement >= 99.5)
                        score.rate = InfoCommand.Rate.Ssp;
                    else if (99.5 > achievement && achievement >= 99)
                        score.rate = InfoCommand.Rate.Ss;
                    else if (99 > achievement && achievement >= 98)
                        score.rate = InfoCommand.Rate.Sp;
                    else if (98 > achievement && achievement >= 97)
                        score.rate = InfoCommand.Rate.S;
                    else if (97 > achievement && achievement >= 94)
                        score.rate = InfoCommand.Rate.Aaa;
                    else if (94 > achievement && achievement >= 90)
                        score.rate = InfoCommand.Rate.Aa;
                    else if (90 > achievement && achievement >= 80)
                        score.rate = InfoCommand.Rate.A;
                    else if (80 > achievement && achievement >= 75)
                        score.rate = InfoCommand.Rate.Bbb;
                    else if (75 > achievement && achievement >= 70)
                        score.rate = InfoCommand.Rate.Bb;
                    else if (70 > achievement && achievement >= 60)
                        score.rate = InfoCommand.Rate.B;
                    else if (60 > achievement && achievement >= 50)
                        score.rate = InfoCommand.Rate.C;
                    else if (50 > achievement)
                        score.rate = InfoCommand.Rate.D;
                    score.MaxDxScore = MaiCommandCommand.GetSong(score.Id)
                        .Charts[score.LevelIndex].MaxDxScore;
                }

                Program.settingsCommand.GetSettings(source);

                var image = new BestImageGenerator().Generate(best, source.Sender.Id, true,
                    Program.settingsCommand.CurrentBotSettings.CompressedImage);
                //image.Write(Environment.CurrentDirectory + @"/temp/b50.png");
                var imageMessage = new ImageMessage
                {
                    Base64 = image
                };

                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain() { new AtMessage(source.Sender.Id), imageMessage });
            }
            catch (Exception)
            {
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain()
                    {
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(
                            " 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定")
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
