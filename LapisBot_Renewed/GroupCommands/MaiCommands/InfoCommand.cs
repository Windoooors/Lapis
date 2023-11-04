using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using ImageMagick;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages;
using Manganese.Text;
using Newtonsoft.Json.Linq;
using static LapisBot_Renewed.MaiCommand;

namespace LapisBot_Renewed
{
    public class InfoCommand : MaiCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"^info\s");
        }

        public enum Rate { D, C, B, BB, BBB, A, AA, AAA, S, Sp, SS, SSp, SSS, SSSp }

        public enum FCState { None, FC, FCp }
        public enum FSState { None, FS, FSd }

        public class GetScore
        {
            public class Level {
                public Rate rate;
                public double achievement;
                public int levelIndex;
                public FCState fc;
                public FSState fs;
            }

            public Level[] levels;

            public void Get(long number, string version, int id)
            {
                var content = Program.apiOperator.Post("api/maimaidxprober/query/plate", new { qq = number, version = new string[] { version } });
                ScoresDto scores = JsonConvert.DeserializeObject<ScoresDto>(content);

                List<Level> levelList = new List<Level>();
                foreach(ScoresDto.ScoreDto score in scores.ScoreDtos)
                {
                    if (score.Id == id)
                    {
                        var rate = new Rate();

                        var achievement = score.Achievements;
                        if (achievement >= 100.5)
                            rate = Rate.SSSp;
                        else if (100.5 > achievement && achievement >= 100)
                            rate = Rate.SSS;
                        else if (100 > achievement && achievement >= 99.5)
                            rate = Rate.SSp;
                        else if (99.5 > achievement && achievement >= 99)
                            rate = Rate.SS;
                        else if (99 > achievement && achievement >= 98)
                            rate = Rate.Sp;
                        else if (98 > achievement && achievement >= 97)
                            rate = Rate.S;
                        else if (97 > achievement && achievement >= 94)
                            rate = Rate.AAA;
                        else if (94 > achievement && achievement >= 90)
                            rate = Rate.AA;
                        else if (90 > achievement && achievement >= 80)
                            rate = Rate.A;
                        else if (80 > achievement && achievement >= 75)
                            rate = Rate.BBB;
                        else if (75 > achievement && achievement >= 70)
                            rate = Rate.BB;
                        else if (70 > achievement && achievement >= 60)
                            rate = Rate.B;
                        else if (60 > achievement && achievement >= 50)
                            rate = Rate.C;
                        else if (50 > achievement)
                            rate = Rate.D;

                        var fc = new FCState();
                        var fs = new FSState();

                        if (score.Fc == "fc")
                            fc = FCState.FC;
                        else if (score.Fc == "fcp")
                            fc = FCState.FCp;
                        else if (score.Fc == "")
                            fc = FCState.None;
                        /*
                        if (score.Fsd == "fs")
                            fs = FSState.FS;
                        else if (score.Fsd == "fsd")
                            fs = FSState.FSd;
                        else if (score.Fsd == "")
                            fs = FSState.None;
                        */
                        levelList.Add(new Level() { achievement = score.Achievements, rate = rate, levelIndex = score.LevelIndex, fc = fc, fs = fs});

                    }
                }
                levels = levelList.ToArray();
            }

            public class ScoresDto
            {
                [JsonProperty("verlist")]
                public ScoreDto[] ScoreDtos;

                public class ScoreDto
                {
                    [JsonProperty("achievements")]
                    public float Achievements;

                    [JsonProperty("fc")]
                    public string Fc;

                    [JsonProperty("fsd")]
                    public string Fsd;

                    [JsonProperty("Id")]
                    public int Id;

                    [JsonProperty("level_index")]
                    public int LevelIndex;
                }
            }

            public static GetScore getScore = new GetScore();
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            var aliases = maiCommand.GetAliasByAliasString(command);
            if (aliases.Length != 0)
            {
                if (aliases.Length == 1)
                {
                    try
                    {
                        var i = maiCommand.GetSongIndexById(aliases[0].id);
                        GetScore.getScore.Get(source.Sender.Id.ToInt64(), songs[i].BasicInfo.Version, songs[i].Id);
                        var _image = new ImageMessage
                        {
                            Base64 = InfoImageGenerator.Generate(i, songs, "歌曲信息", GetScore.getScore.levels).ToBase64(),
                        };
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
                    }
                }
                else
                {
                    string ids = string.Empty;
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        int _index = maiCommand.GetSongIndexById(aliases[i].id);
                        ids += "ID " + aliases[i].id + " - " + maiCommand.songs[_index].Title + " [" + maiCommand.songs[_index].Type + "]";
                        if (i != aliases.Length - 1)
                            ids += "\n";
                    }
                    int index = maiCommand.GetSongIndexById(aliases[0].id);
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(" 该别称有多首歌曲匹配：\n" + ids + "\n*使用 \"lps mai alias ID " + aliases[0].id + "\" 指令即可查询歌曲 " + maiCommand.songs[index].Title + " [" + maiCommand.songs[index].Type + "] 的相关信息")});
                }
            }
            else
            {
                var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
                var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
                if (idRegex.IsMatch(command))
                {
                    try
                    {
                        var id = idHeadRegex.Replace(command, string.Empty).ToInt32();
                        int index = maiCommand.GetSongIndexById(id);
                        if (index != -1)
                        {
                            try
                            {
                                GetScore.getScore.Get(source.Sender.Id.ToInt64(), songs[index].BasicInfo.Version, songs[index].Id);
                                var _image = new ImageMessage
                                {
                                    Base64 = InfoImageGenerator.Generate(index, songs, "歌曲信息", GetScore.getScore.levels).ToBase64(),
                                };

                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
                            }
                            catch
                            {
                                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
                            }
                        }
                        else
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                    }

                }
                else
                {
                    int index = maiCommand.GetSongIndexByTitle(command);
                    if (index != -1)
                    {
                        try
                        {
                            GetScore.getScore.Get(source.Sender.Id.ToInt64(), songs[index].BasicInfo.Version, songs[index].Id);
                            var _image = new ImageMessage
                            {
                                Base64 = InfoImageGenerator.Generate(index, songs, "歌曲信息", GetScore.getScore.levels).ToBase64(),
                            };

                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
                        }
                        catch
                        {
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您没有绑定“舞萌 DX | 中二节奏查分器”账户，清前往 https://www.diving-fish.com/maimaidx/prober 进行绑定") });
                        }
                    }
                    else
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                }
                //MessageManager.SendGroupMessageAsync(source.GroupId, " ");
            }
        }
    }
}
