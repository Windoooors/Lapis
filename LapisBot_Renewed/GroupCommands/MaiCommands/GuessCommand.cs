using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LapisBot_Renewed.Collections;
using LapisBot_Renewed.ImageGenerators;
using Manganese.Text;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class AudioEditor
    {
        public static string Convert(int id)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "temp/" + id))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + "temp/" + id);
                var command = "ffmpeg -i " + AppContext.BaseDirectory + "resources/tracks/" + id + ".mp3" + " -f segment -segment_time 3 -c copy " + AppContext.BaseDirectory +
                              "temp/" + id + "/%5d.mp3";
                ApiOperator.Bash(command);
            }

            var audioCount = Directory.GetFiles(AppContext.BaseDirectory + "temp/" + id).Length;
            var audioIndex = new Random().Next(3, audioCount - 4);

            return new AudioToVoiceConverter().ConvertAudio(AppContext.BaseDirectory + "temp/" + id + "/" +
                                               audioIndex.ToString("00000") + ".mp3");
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
            DirectCommand = new Regex(@"^guess$");
            SubDirectCommand  = new Regex(@"^guess ");
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
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
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
                if (!(_guessingGroupsMap.Values.ToArray()[i].Item2.Ticks <= DateTime.Now.Ticks))
                    return;
                var keyIdDateTimePair = _guessingGroupsMap.Values.ToArray()[i];
                var groupId = _guessingGroupsMap.Keys.ToArray()[i];
                var taskAnnounce = new Task(() =>
                    AnnounceAnswer(keyIdDateTimePair, groupId));
                taskAnnounce.Start();
            }
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            StartGuessing(source);
            return Task.CompletedTask;
        }
        
        private Task StartGuessing(GroupMessageReceiver source, int difficulty)
        {
            if (!_guessingGroupsMap.ContainsKey(source.GroupId))
            {
                var random = new Random();
                SongDto[] songs = Levels[difficulty].ToArray();
                var songIndex = random.Next(0, songs.Length - 1);
                _guessingGroupsMap.Add(source.GroupId,
                    (songs[songIndex].Id, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain()
                        { new AtMessage(source.Sender.Id), new PlainMessage(" 试试看吧！Lapis Bot 将在 30s 后公布答案") });

                var voice = new VoiceMessage
                {
                    Path = AudioEditor.Convert(songs[songIndex].Id)
                };
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain { voice });
            }
            else
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 本次游戏尚未结束，要提前结束游戏，请使用指令 \"lps mai guess answer\"") });
            return Task.CompletedTask;
        }

        private Task StartGuessing(GroupMessageReceiver source)
        {
            if (!_guessingGroupsMap.ContainsKey(source.GroupId))
            {
                var random = new Random();
                var songIndex = random.Next(0, Songs.Length);
                _guessingGroupsMap.Add(source.GroupId,
                    (MaiCommandCommand.Songs[songIndex].Id, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain()
                        { new AtMessage(source.Sender.Id), new PlainMessage(" 试试看吧！Lapis Bot 将在 30s 后公布答案") });
                
                var voice = new VoiceMessage
                {
                    Path = AudioEditor.Convert(Songs[songIndex].Id)
                };
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain { voice });
            }
            else
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 本次游戏尚未结束，要提前结束游戏，请使用指令 \"lps mai guess answer\"") });
            return Task.CompletedTask;
        }

        private Task AnnounceAnswer((int, DateTime) keyIdDateTimePair, string groupId)
        {
            
            //var keyIdDateTimePair = (-1, DateTime.MinValue);

            Program.settingsCommand.GetSettings(groupId);
            
            _guessingGroupsMap.Remove(groupId);

            var image = new InfoImageGenerator().Generate(GetSongIndexById(keyIdDateTimePair.Item1), MaiCommandCommand.Songs,
                "谜底", null, Program.settingsCommand.CurrentBotSettings.CompressedImage);

            MessageManager.SendGroupMessageAsync(groupId,
                new MessageChain()
                {
                    new PlainMessage("游戏结束啦！ 答案是："),
                    new ImageMessage { Base64 = image }
                });

            if (!((GuessSettings)CurrentGroupCommandSettings).SongPreview)
                return Task.CompletedTask;
            var voice = new VoiceMessage
            {
                Path = new AudioToVoiceConverter().ConvertSong(keyIdDateTimePair.Item1)
            };
            MessageManager.SendGroupMessageAsync(groupId, new MessageChain() { voice });

            return Task.CompletedTask;
        }

        public override Task SubParse(string command, GroupMessageReceiver source)
        {
            if (command == "answer")
            {
                if (!_guessingGroupsMap.ContainsKey(source.GroupId))
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId,
                        new MessageChain
                        {
                            new AtMessage(source.Sender.Id),
                            new PlainMessage(" 没有游戏正在进行喔！使用指令 \"l mai guess\" 即可开启新一轮的游戏")
                        });
                    return Task.CompletedTask;
                }

                var keyIdDateTimePair = (-1, DateTime.MinValue);
                _guessingGroupsMap.TryGetValue(source.GroupId, out keyIdDateTimePair);

                Program.settingsCommand.GetSettings(source);
                
                _guessingGroupsMap.Remove(source.GroupId);

                var image = new InfoImageGenerator().Generate(GetSongIndexById(keyIdDateTimePair.Item1), MaiCommandCommand.Songs,
                    "谜底", null, Program.settingsCommand.CurrentBotSettings.CompressedImage);

                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain()
                    {
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(" 游戏结束啦！ 答案是："),
                        new ImageMessage { Base64 = image }
                    });

                if (!((GuessSettings)CurrentGroupCommandSettings).SongPreview)
                    return Task.CompletedTask;
                var voice = new VoiceMessage
                {
                    Path = new AudioToVoiceConverter().ConvertSong(keyIdDateTimePair.Item1)
                };
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { voice });
                
                return Task.CompletedTask;
            }

            if (!LevelDictionary.ContainsKey(command))
            {
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain
                {
                    new AtMessage(source.Sender.Id), new PlainMessage(" 不支持的等级名称")
                });
                return Task.CompletedTask;
            }
            else
            {
                int i = -1;
                LevelDictionary.TryGetValue(command, out i);
                if (i == 6)
                    return Task.CompletedTask;
                StartGuessing(source, i);
                
                return Task.CompletedTask;
            }
        }

        public Task Reply(GroupMessageReceiver source, int id)
        {
            Program.settingsCommand.GetSettings(source);
            
            _guessingGroupsMap.Remove(source.GroupId);

            var image = new InfoImageGenerator().Generate(GetSongIndexById(id), MaiCommandCommand.Songs,
                "谜底", null, Program.settingsCommand.CurrentBotSettings.CompressedImage);
                            
            MessageManager.SendGroupMessageAsync(source.GroupId,
                new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" Bingo! 答案是："), new ImageMessage { Base64 = image} });
                        
            if (!((GuessSettings)CurrentGroupCommandSettings).SongPreview)
                return Task.CompletedTask;
            
            var voice = new VoiceMessage
            {
                Path = new AudioToVoiceConverter().ConvertSong(id)
            };
            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { voice });

            return Task.CompletedTask;
        }

        public override Task RespondWithoutParsingCommand(string command, GroupMessageReceiver source)
        {
            var idPassed = false;
            var namePassed = false;
            var aliasPassed = false;

            if (_guessingGroupsMap.ContainsKey(source.GroupId))
            {
                var keyIdDateTimePair = (-1, DateTime.MinValue);
                _guessingGroupsMap.TryGetValue(source.GroupId, out keyIdDateTimePair);
                var aliases = MaiCommandCommand.GetAliasByAliasString(command);
                foreach (Alias alias in aliases)
                {
                    aliasPassed = true;
                    if (alias.Id == keyIdDateTimePair.Item1)
                    {
                        var task = new Task(() => Reply(source, keyIdDateTimePair.Item1));
                        task.Start();
                        
                        return Task.CompletedTask;
                    }
                }

                var songIndex = MaiCommandCommand.GetSongIndexByTitle(command);
                if (songIndex != -1)
                    namePassed = true;
                if (songIndex != -1 && MaiCommandCommand.Songs[songIndex].Id == keyIdDateTimePair.Item1)
                {
                    var task = new Task(() => Reply(source, keyIdDateTimePair.Item1));
                    task.Start();

                    return Task.CompletedTask;
                }

                var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
                var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
                if (idRegex.IsMatch(command))
                {
                    idPassed = true;
                    var id = idHeadRegex.Replace(command, string.Empty).ToInt32();
                    int index = MaiCommandCommand.GetSongIndexById(id);
                    if (index != -1 &&Songs[index].Id == keyIdDateTimePair.Item1)
                    {
                        var task = new Task(() => Reply(source, keyIdDateTimePair.Item1));
                        task.Start();
                        
                        return Task.CompletedTask;
                    }
                }
            }

            if (idPassed || namePassed || aliasPassed)
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain()
                {
                    new AtMessage(source.Sender.Id), new PlainMessage(" 不对喔")
                });

            return Task.CompletedTask;
        }
    }
}