using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Collections;
using LapisBot_Renewed.ImageGenerators;
using Newtonsoft.Json;
using Xabe.FFmpeg;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class AudioEditor
    {
        public static string Convert(int id)
        {
            try
            {
                if (!Directory.Exists(AppContext.BaseDirectory +
                                      "temp/" + id + "/"))
                    Directory.CreateDirectory(AppContext.BaseDirectory +
                                              "temp/" + id + "/");

                var info = FFmpeg.GetMediaInfo(AppContext.BaseDirectory + "resource/tracks/" + id + ".mp3").Result;
                var duration = (int)info
                    .Duration
                    .TotalSeconds;

                var startTime = TimeSpan.FromSeconds(new Random().Next(10, duration - 13));

                var outputDuration = TimeSpan.FromSeconds(3);

                if (File.Exists(AppContext.BaseDirectory +
                                "temp/" + id + "/" +
                                startTime.TotalSeconds + ".mp3"))
                    return new(AppContext.BaseDirectory +
                               "temp/" + id + "/" +
                               startTime.TotalSeconds + ".mp3");

                var conversion = FFmpeg.Conversions.New();

                conversion.AddStream(info.AudioStreams).AddParameter($"-ss {startTime} -t {outputDuration}")
                    .SetOutput(AppContext.BaseDirectory +
                               "temp/" + id + "/" +
                               startTime.TotalSeconds + ".mp3");
                    
                var result = conversion.Start().Result;
                
                return new(AppContext.BaseDirectory +
                           "temp/" + id + "/" +
                           startTime.TotalSeconds + ".mp3");
            }
            catch (Exception ex)
            {
                Console.Write(ex.StackTrace);
                return "";
            }
        }
    }
    
    public class GuessSettings : GroupCommand.GroupCommandSettings
    {
        public bool SongPreview { get; set; }

        public GuessSettings Clone(GuessSettings guessSettings)
        {
            return JsonConvert.DeserializeObject<GuessSettings>(JsonConvert.SerializeObject(guessSettings));
        }
    }
    
    public class GuessCommand : MaiCommand
    {
        private Dictionary<string, (int, DateTime)> _guessingGroupsMap = new Dictionary<string, (int, DateTime)>();

        public override Task GetDefaultSettings()
        {
            CurrentGroupCommandSettings = ((GuessSettings)DefaultSettings).Clone((GuessSettings)DefaultSettings);
            return Task.CompletedTask;
        }
        
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^guess$");
            SubHeadCommand = new Regex(@"^guess ");
            DirectCommand = new Regex(@"^songs$|^猜歌$|^song$");
            SubDirectCommand  = new Regex(@"^songs |^猜歌 |^song ");
            CoolDownTime = 5;
            DefaultSettings = new GuessSettings
            {
                Enabled = true,
                SongPreview = false,
                DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" }, { "SongPreview", "歌曲试听" } },
                SettingsName = "猜歌"
            };
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
                settingsList.Add(JsonConvert.DeserializeObject<GuessSettings>(settingsString));
            }
            
            Program.TimeChanged += TimeChanged;

            return Task.CompletedTask;
        }

        private void TimeChanged(object obj, EventArgs e)
        {
            if (_guessingGroupsMap.Count == 0)
                return;
            for (int i = 0; i < _guessingGroupsMap.Count; i++)
            {
                //Console.WriteLine(_guessingGroupsMap.Values.ToArray()[i].Item2.Ticks + " " + DateTime.Now.Ticks);
                if (_guessingGroupsMap.Values.ToArray()[i].Item2.Ticks > DateTime.Now.Ticks)
                    continue;
                var keyIdDateTimePair = _guessingGroupsMap.Values.ToArray()[i];
                var groupId = _guessingGroupsMap.Keys.ToArray()[i];
                var taskAnnounce = new Task(() =>
                    AnnounceAnswer(keyIdDateTimePair, groupId, false, 0));
                taskAnnounce.Start();
            }
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            StartGuessing(source);
            CancelCoolDownTimer(source.GroupId.ToString());
            return Task.CompletedTask;
        }
        
        private Task StartGuessing(CqGroupMessagePostContext source, int difficulty)
        {
            if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
            {
                var random = new Random();
                SongDto[] songs = Instance.Levels[difficulty].ToArray();
                var songIndex = random.Next(0, songs.Length);
                _guessingGroupsMap.Add(source.GroupId.ToString(),
                    (songs[songIndex].Id, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                        { new CqReplyMsg(source.MessageId), new CqTextMsg("试试看吧！Lapis Bot 将在 30s 后公布答案") });

                var path = AudioEditor.Convert(songs[songIndex].Id);

                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                        { new CqRecordMsg("file:///" + path) });
            }
            else
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps mai guess answer\"")
                    });
            return Task.CompletedTask;
        }

        private Task StartGuessing(CqGroupMessagePostContext source)
        {
            if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
            {
                var random = new Random();
                var songIndex = random.Next(0, Instance.Songs.Length);
                _guessingGroupsMap.Add(source.GroupId.ToString(),
                    (Instance.Songs[songIndex].Id, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                        { new CqReplyMsg(source.MessageId), new CqTextMsg("试试看吧！Lapis Bot 将在 30s 后公布答案") });

                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                        { new CqRecordMsg("file:///" + AudioEditor.Convert(Instance.Songs[songIndex].Id)) });
            }
            else
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps mai guess answer\"")
                    });
            return Task.CompletedTask;
        }

        private Task AnnounceAnswer((int, DateTime) keyIdDateTimePair, string groupId, bool won, long messageId)
        {
            
            //var keyIdDateTimePair = (-1, DateTime.MinValue);

            Program.SettingsCommand.GetSettings(groupId);
            
            _guessingGroupsMap.Remove(groupId);
            
            var text = String.Empty;
            if (won)
                text = "Bingo! 答案是：";
            else
                text = "游戏结束啦！ 答案是：";

            var image = new InfoImageGenerator().Generate(Instance.GetSong((keyIdDateTimePair.Item1)),
                "谜底", null, Program.SettingsCommand.CurrentBotSettings.CompressedImage);

            if (messageId == 0)
                Program.Session.SendGroupMessageAsync(long.Parse(groupId),
                    new CqMessage
                        { new CqTextMsg(text), new CqImageMsg("base64://" + image) });
            else
                Program.Session.SendGroupMessageAsync(long.Parse(groupId),
                    new CqMessage
                        { new CqReplyMsg(messageId), new CqTextMsg(text), new CqImageMsg("base64://" + image) });
            
            GroupsMap.Add(groupId, DateTime.Now.Add(new TimeSpan(0, 0, 0, CoolDownTime)));

            if (!((GuessSettings)CurrentGroupCommandSettings).SongPreview)
                return Task.CompletedTask;

            Program.Session.SendGroupMessageAsync(long.Parse(groupId),
                new CqMessage
                    { new CqRecordMsg("file:///" + new AudioToVoiceConverter().GetSongPath(keyIdDateTimePair.Item1)) });

            return Task.CompletedTask;
        }

        public override Task SubParse(string command, CqGroupMessagePostContext source)
        {
            if (command == "answer")
            {
                if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        new CqMessage
                        {
                            new CqReplyMsg(source.MessageId),
                            new CqTextMsg("没有游戏正在进行喔！发送指令 \"l mai guess\" 即可开启新一轮的游戏")
                        });
                    CancelCoolDownTimer(source.GroupId.ToString());
                    return Task.CompletedTask;
                }

                for (int i = 0; i < _guessingGroupsMap.Count; i++)
                {
                    if (_guessingGroupsMap.Keys.ToArray()[i] == source.GroupId.ToString())
                    {
                        CancelCoolDownTimer(source.GroupId.ToString());
                        AnnounceAnswer(_guessingGroupsMap.Values.ToArray()[i], source.GroupId.ToString(), false,
                            source.MessageId);
                    }
                }
                
                return Task.CompletedTask;
            }

            if (!Instance.LevelDictionary.ContainsKey(command))
            {
                CancelCoolDownTimer(source.GroupId.ToString());
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("不支持的等级名称")
                    });
                return Task.CompletedTask;
            }
            else
            {
                int i = -1;
                Instance.LevelDictionary.TryGetValue(command, out i);
                if (i == 6)
                    return Task.CompletedTask;
                StartGuessing(source, i);
                CancelCoolDownTimer(source.GroupId.ToString());
                
                return Task.CompletedTask;
            }
        }

        public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
        {
            var passed = false;

            if (_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
            {
                var keyIdDateTimePair = (-1, DateTime.MinValue);
                _guessingGroupsMap.TryGetValue(source.GroupId.ToString(), out keyIdDateTimePair);

                var songs = Instance.GetSongs(command);
                if (songs.Length != 0)
                    passed = true;
                for (int j = 0; j < songs.Length; j++)
                {
                    if (passed && songs[j].Id == keyIdDateTimePair.Item1)
                    {
                        var task = new Task(() =>
                            AnnounceAnswer(keyIdDateTimePair, source.GroupId.ToString(), true, source.MessageId));
                        task.Start();

                        return Task.CompletedTask;
                    }
                }
            }

            if (passed)
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("不对哦")
                    });

            return Task.CompletedTask;
        }
    }
}