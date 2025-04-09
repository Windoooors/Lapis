using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands.MaiCommands;

public class LettersCommand : MaiCommand
{
    
    private class SongList
    {
        public SongDto[] AllSongs;
        public readonly List<SongDto> GuessedSongs = new();
        public readonly List<char> GuessedLetters = new();
    }

    private readonly Dictionary<string, (SongList, DateTime)>
        _guessingGroupsMap = new();
    
    public override Task Initialize()
    {
        HeadCommand = new Regex(@"^letters$");
        DirectCommand = new Regex(@"^letters$|^开字母$");
        SubHeadCommand = new Regex(@"^letters\s");
        SubDirectCommand = new Regex(@"^letters\s|^开字母\s");
        DefaultSettings.SettingsName = "舞萌开字母";
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

    private enum SpecialCharactersToBeIncluded
    {
       Chinese,
       Japanese,
       Both,
       Neither
    };

    private SongDto GetRandomSong(SpecialCharactersToBeIncluded specialCharactersToBeIncluded)
    {
        Regex pattern  = new Regex("");
        switch (specialCharactersToBeIncluded)
        {
            case SpecialCharactersToBeIncluded.Chinese:
                pattern = new Regex(@"[\u3040-\u30FF\u31F0-\u31FF\uFF00-\uFFEF]");
                break;
            case SpecialCharactersToBeIncluded.Japanese:
                pattern = new Regex(@"[\u4e00-\u9fa5]");
                break;
            case SpecialCharactersToBeIncluded.Neither:
                pattern = new Regex(@"[\u4e00-\u9fa5]|[\u3040-\u30FF\u31F0-\u31FF\uFF00-\uFFEF]");
                break;
        }

        var random = new Random();
        var index = random.Next(0, Instance.Songs.Length);

        if (specialCharactersToBeIncluded == SpecialCharactersToBeIncluded.Both)
        {
            return Instance.Songs[index];
        }
        
        while (pattern.IsMatch(Instance.Songs[index].Title))
            index = random.Next(0, Instance.Songs.Length);
        return Instance.Songs[index];
    }

    private SongList GenerateSongList(SpecialCharactersToBeIncluded specialCharactersToBeIncluded)
    {
        var songs = new List<SongDto>();

        songs.Add(GetRandomSong(specialCharactersToBeIncluded));

        while (songs.Count < 15)
        {
            var song = GetRandomSong(specialCharactersToBeIncluded);
            if (!songs.Contains(song))
                songs.Add(song);
        }

        return new SongList() { AllSongs = songs.ToArray() };
    }
    
    private void TimeChanged(object obj, EventArgs e)
    {
        if (_guessingGroupsMap.Count == 0)
            return;
        for (int i = 0; i < _guessingGroupsMap.Count; i++)
        {
            if (_guessingGroupsMap.Values.ToArray()[i].Item2.Ticks > DateTime.Now.Ticks)
                continue;
            var keyWordDateTimePair = _guessingGroupsMap.Values.ToArray()[i];
            var groupId = _guessingGroupsMap.Keys.ToArray()[i];
            var taskAnnounce = new Task(() =>
                AnnounceAnswer(keyWordDateTimePair.Item1, groupId, 0, true));
            taskAnnounce.Start();
        }
    }
    
    private Task AnnounceAnswer(SongList songs, string groupId, long messageId, bool gameOver)
    {
        if (gameOver)
            _guessingGroupsMap.Remove(groupId);
        
        var title = "舞萌开字母结束啦！\n答案是：\n";
        
        var songNames = "";
        int index = 1;

        foreach (var song in songs.AllSongs)
        {
            if (gameOver)
                songNames += $"{index}.{song.Title}\n";
            else
            {
                var songDisplay = string.Empty;

                var hiddenLetterCount = 0;
            
                foreach (char songNameCharacter in song.Title)
                {
                    var blankMarkChar = new char[] { ' ', '\u3000', '\u200e'  }.Contains(songNameCharacter) ? songNameCharacter : '*';
                    
                    foreach (char character in songs.GuessedLetters)
                    {
                        if (char.ToLower(character) == char.ToLower(songNameCharacter))
                            blankMarkChar = songNameCharacter;
                    }
                    
                    if (blankMarkChar == '*')
                        hiddenLetterCount++;

                    songDisplay += blankMarkChar;
                }
                
                songDisplay.TrimEnd();
                
                var isSongGuessed = false;
                
                foreach (var guessedSong in songs.GuessedSongs)
                    if (song == guessedSong)
                    {
                        songDisplay = guessedSong.Title;
                        isSongGuessed = true;
                        break;
                    }

                if (hiddenLetterCount == 0 && !isSongGuessed)
                {
                    songs.GuessedSongs.Add(song);

                    if (songs.GuessedSongs.Count == songs.AllSongs.Length)
                    {
                        AnnounceAnswer(songs, groupId, messageId, true);
                        return Task.CompletedTask;
                    }
                }

                songNames += $"{index}.{songDisplay}\n";
            }

            index++;
        }
        
        songNames.Remove(songNames.Length - 1);

        var text = "";
        if (gameOver)
            text = $"{title}\n{songNames}";
        else
            text = $"{songNames}\n已开字母：{string.Join(", ", songs.GuessedLetters)}";

        if (messageId != 0)
            Program.Session.SendGroupMessageAsync(long.Parse(groupId), new CqMessage
            {
                new CqReplyMsg(messageId),
                new CqTextMsg(text)
            });
        else
            Program.Session.SendGroupMessageAsync(long.Parse(groupId), new CqMessage
            {
                new CqTextMsg(text)
            });

        if (gameOver)
            GroupsMap.Add(groupId, DateTime.Now.Add(new TimeSpan(0, 0, 0, CoolDownTime)));

        return Task.CompletedTask;
    }

    private Task StartGuessing(CqGroupMessagePostContext source, SpecialCharactersToBeIncluded specialCharactersToBeIncluded)
    {
        if (_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
        {
            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(
                    "本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps mai letters answer\"\n" +
                    "要开字母 a，请发送指令 \"开 a\"\n" +
                    "要猜编号为 \"1\" 的歌曲为 [SD] LUCIA, 请发送指令 \"1.LUCIA\"")
            });
            CancelCoolDownTimer(source.GroupId.ToString());
            return Task.CompletedTask;
        }

        var excludedCharacterInformation = "";
        if (specialCharactersToBeIncluded == SpecialCharactersToBeIncluded.Japanese)
        {
            excludedCharacterInformation = "本次开字母游戏中的谜底均不包含中文字符\n";
        }
        else if (specialCharactersToBeIncluded == SpecialCharactersToBeIncluded.Chinese)
        {
            excludedCharacterInformation = "本次开字母游戏中的谜底均不包含日文字符\n";
        }
        else if (specialCharactersToBeIncluded == SpecialCharactersToBeIncluded.Neither)
        {
            excludedCharacterInformation = "本次开字母游戏中的谜底均不包含中、日文字符\n";
        }

        var title = excludedCharacterInformation + "Lapis 将在 20mins 后公布答案！\n" + 
                    "要开字母 a，请发送指令 \"开 a\"\n" +
                    "要猜编号为 \"1\" 的歌曲为 [SD] LUCIA, 请发送指令 \"1.LUCIA\"";
        
        var songNames = "";
        int index = 1;

        var songs = GenerateSongList(specialCharactersToBeIncluded);

        foreach (var song in songs.AllSongs)
        {
            var songDisplay = "";
            
            foreach (char songNameCharacter in song.Title)
            {
                var blankMarkChar = new char[] { ' ', '\u3000' }.Contains(songNameCharacter) ? songNameCharacter : '*';
                
                songDisplay += blankMarkChar;
            }

            songDisplay.TrimEnd();
            
            songNames += $"{index}.{songDisplay}\n";
            
            index++;
        }

        Program.Session.SendGroupMessageAsync(source.GroupId,
            new CqMessage
                { $"{title}\n\n{songNames}" });
        
        _guessingGroupsMap.Add(source.GroupId.ToString(),
            (songs, DateTime.Now.Add(new TimeSpan(0, 0, 20, 0))));
        
        CancelCoolDownTimer(source.GroupId.ToString());
        
        return Task.CompletedTask;
    }
    
    public override Task Parse(string command, CqGroupMessagePostContext source)
    {
        StartGuessing(source, SpecialCharactersToBeIncluded.Both);
        
        return Task.CompletedTask;
    }

    public override Task SubParse(string command, CqGroupMessagePostContext source)
    {
        var specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Both;

        if (command.ToLower().StartsWith("both"))
        {
            specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Both;
            CancelCoolDownTimer(source.GroupId.ToString());
        }
        else if (command.ToLower().StartsWith("neither"))
        {
            specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Neither;
            CancelCoolDownTimer(source.GroupId.ToString());
        }
        else if (command.ToLower().StartsWith("chinese"))
        {
            specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Chinese;
            CancelCoolDownTimer(source.GroupId.ToString());
        }
        else if (command.ToLower().StartsWith("japanese"))
        {
            specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Japanese;
            CancelCoolDownTimer(source.GroupId.ToString());
        }
        else if (command.ToLower() == "answer")
        {
            (SongList, DateTime) keyWordDateTimePair;
            _guessingGroupsMap.TryGetValue(source.GroupId.ToString(), out keyWordDateTimePair);

            if (keyWordDateTimePair.Item1 == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqReplyMsg(source.MessageId), "现在没有正在进行的舞萌开字母游戏！" });
                CancelCoolDownTimer(source.GroupId.ToString());
                return Task.CompletedTask;
            }
            
            AnnounceAnswer(keyWordDateTimePair.Item1, source.GroupId.ToString(), source.MessageId, true);
            CancelCoolDownTimer(source.GroupId.ToString());
            return Task.CompletedTask;
        }
        else
        {
            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                { new CqReplyMsg(source.MessageId), "不支持的参数类型" });
            CancelCoolDownTimer(source.GroupId.ToString());
            return Task.CompletedTask;
        }
        
        StartGuessing(source, specialCharactersToBeIncluded);
        return Task.CompletedTask;
    }

    public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        (SongList, DateTime) keyValuePair;
        _guessingGroupsMap.TryGetValue(source.GroupId.ToString(), out keyValuePair);

        if (keyValuePair.Item1 == null)
            return Task.CompletedTask;
        
        if (command.StartsWith("开 "))
        {
            var targetLetter = command.Replace("开 ", "");
            if (targetLetter == "" || targetLetter.Length > 1)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqReplyMsg(source.MessageId), "不支持的参数类型" });
                return Task.CompletedTask;
            }

            var targetChar = targetLetter[0];
            if (!keyValuePair.Item1.GuessedLetters.Contains(char.ToLower(targetChar)))
                keyValuePair.Item1.GuessedLetters.Add(char.ToLower(targetChar));

            AnnounceAnswer(keyValuePair.Item1, source.GroupId.ToString(), source.MessageId, false);
            return Task.CompletedTask;
        }

        var indexPattern = new Regex(@"^[1-9][0-9]\.|^[1-9]\.");
        
        if (indexPattern.IsMatch(command))
        {
            var index = int.Parse(new Regex("^[1-9][0-9]|^[1-9]").Match(command).ToString());
            if (index > 15 || index < 1)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqReplyMsg(source.MessageId), "不支持的参数类型" });
                return Task.CompletedTask;
            }

            var songIndicator = indexPattern.Replace(command, "");
            if (songIndicator == "")
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqReplyMsg(source.MessageId), "不支持的参数类型" });
                return Task.CompletedTask;
            }

            var songs = Instance.GetSongs(songIndicator);

            if (songs == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    { new CqReplyMsg(source.MessageId), "未开出歌曲" });
                return Task.CompletedTask;
            }

            foreach (var song in songs)
            {
                if (keyValuePair.Item1.AllSongs[index - 1] == song)
                {
                    if (keyValuePair.Item1.GuessedSongs.Contains(song))
                    {
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                            new CqMessage
                            {
                                new CqReplyMsg(source.MessageId), "该歌曲已经被开出"
                            });
                        return Task.CompletedTask;
                    }
                    
                    keyValuePair.Item1.GuessedSongs.Add(song);

                    if (keyValuePair.Item1.GuessedSongs.Count == keyValuePair.Item1.AllSongs.Length)
                    {
                        AnnounceAnswer(keyValuePair.Item1, source.GroupId.ToString(), source.MessageId, true);
                        return Task.CompletedTask;
                    }

                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        new CqMessage
                        {
                            new CqReplyMsg(source.MessageId), "开出歌曲：",
                            "[" + song.Type.ToUpper() + "] " + song.Title
                        });
                    AnnounceAnswer(keyValuePair.Item1, source.GroupId.ToString(), source.MessageId, false);
                    return Task.CompletedTask;
                }
            }

            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                { new CqReplyMsg(source.MessageId), "未开出歌曲" });
            return Task.CompletedTask;
        }
        
        return Task.CompletedTask;
    }
}