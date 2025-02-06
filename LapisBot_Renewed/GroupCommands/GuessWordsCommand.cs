using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{

    
    public class GuessWordsCommand : VocabularyCommand
    {
        private Dictionary<string, (WordDto, DateTime)> _guessingGroupsMap = new Dictionary<string, (WordDto, DateTime)>();
        
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^guess word|^guess words|^猜词");
            SubHeadCommand = new Regex(@"^guess word\s|^guess words\s|^猜词\s");
            DirectCommand = new Regex(@"^guess word|^guess words|^猜词");
            SubDirectCommand = new Regex(@"^guess word\s|^guess words\s|^猜词\s");
            DefaultSettings.SettingsName = "猜单词";
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
                if (_guessingGroupsMap.Values.ToArray()[i].Item2.Ticks > DateTime.Now.Ticks)
                    continue;
                var keyWordDateTimePair = _guessingGroupsMap.Values.ToArray()[i];
                var groupId = _guessingGroupsMap.Keys.ToArray()[i];
                var taskAnnounce = new Task(() =>
                    AnnounceAnswer(keyWordDateTimePair.Item1, groupId, false, 0));
                taskAnnounce.Start();
            }
        }

        public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
        {
            if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
                return Task.CompletedTask;
            var keyWordDateTimePair = (new WordDto(), DateTime.MinValue);
            _guessingGroupsMap.TryGetValue(source.GroupId.ToString(), out keyWordDateTimePair);
            var word = keyWordDateTimePair.Item1;
            if (command.ToUpper() == word.Word.ToUpper())
            {
                AnnounceAnswer(word, source.GroupId.ToString(), true, source.MessageId);
                return Task.CompletedTask;
            }

            var certified = false;
            
            foreach (Vocabulary vocabulary in Vocabularies)
            {
                foreach (WordDto wordItem in vocabulary.Words)
                {
                    if (wordItem.Word == command)
                    {
                        certified = true;
                        break;
                    }
                }
            }
            
            if (!certified)
                return Task.CompletedTask;
            
            var text = string.Empty;
            
            foreach (char wordCharacter in word.Word.ToLower())
            {
                var blankText = "_ ";
                foreach (char character in command.ToLower())
                {
                    if (character == wordCharacter)
                        blankText = character.ToString();
                }

                text += blankText;
            }

            text.TrimEnd();
            
            var titleText = "不对哦！\n";
            foreach (WordDto.TranslationDto translation in word.Translations)
                titleText += translation.Type + "." + translation.Translation + "; \n";
            titleText += "提示：\n";
            titleText += text;

            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(titleText)
            });
            
            return Task.CompletedTask;
        }

        private Task AnnounceAnswer(WordDto word, string groupId, bool won, long messageId)
        {
            _guessingGroupsMap.Remove(groupId);

            var text = String.Empty;
            if (won)
                text = "Bingo!";
            else
                text = "猜词结束啦！";
            text += "\n答案是：" + word.Word + " ";
            foreach (WordDto.TranslationDto translation in word.Translations)
                text += translation.Type + "." + translation.Translation + "; \n";

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
            
            GroupsMap.Add(groupId, DateTime.Now.Add(new TimeSpan(0, 0, 0, CoolDownTime)));

            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            var text = "Lapis Bot 可从以下词库选取词语猜词\n";
            var i = 0;
            foreach (string file in Directory.GetFiles(AppContext.BaseDirectory + "resource/vocabulary/"))
            {
                if (Path.GetFileName(file) == ".DS_Store")
                    continue;
                i++;
                text += i + "." + Path.GetFileName(file).Replace(Path.GetExtension(file), string.Empty) + "\n";
            }

            text += "请发送指令 \"l guess words 1\" 来用 " + Path.GetFileName(
                                                        Directory.GetFileSystemEntries(AppContext.BaseDirectory +
                                                            "resource/vocabulary/")[0]).Replace(Path.GetExtension(
                                                        Directory.GetFileSystemEntries(AppContext.BaseDirectory +
                                                            "resource/vocabulary/")[0]), string.Empty)
                                                    + " 词库开始游戏";
            text.TrimEnd();

            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(text)
            });
            CancelCoolDownTimer(source.GroupId.ToString());
            return Task.CompletedTask;
        }

        public override Task SubParse(string command, CqGroupMessagePostContext source)
        {
            if (command == "answer")
            {
                if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
                {

                    Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("没有游戏正在进行喔！发送指令 \"l guess words 1\" 即可开启新一轮的游戏")
                    });
                    CancelCoolDownTimer(source.GroupId.ToString());
                    return Task.CompletedTask;
                }

                for (int i = 0; i < _guessingGroupsMap.Count; i++)
                {
                    if (_guessingGroupsMap.Keys.ToArray()[i] == source.GroupId.ToString())
                    {
                        CancelCoolDownTimer(source.GroupId.ToString());
                        AnnounceAnswer(_guessingGroupsMap.Values.ToArray()[i].Item1, source.GroupId.ToString(), false,
                            source.MessageId);
                    }
                }

                return Task.CompletedTask;
            }

            var count = Directory.GetFiles(AppContext.BaseDirectory + "resource/vocabulary/").Length;
            var indexRegex = new Regex("^[1-" + count + "]$");
            if (indexRegex.IsMatch(command))
            {
                StartGuessing(int.Parse(command) - 1, source);
                CancelCoolDownTimer(source.GroupId.ToString());
                return Task.CompletedTask;
            }
            
            foreach (Vocabulary vocabulary in Vocabularies)
            {
                foreach (WordDto wordItem in vocabulary.Words)
                {
                    if (wordItem.Word == command)
                    {
                        if (_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
                        {

                            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                            {
                                new CqReplyMsg(source.MessageId),
                                new CqTextMsg("本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps guess words answer\"")
                            });
                            return Task.CompletedTask;
                        }
                        
                        var word = wordItem;
                        var text = "试试看吧！\n";
                        foreach (WordDto.TranslationDto translation in word.Translations)
                            text += translation.Type + "." + translation.Translation + "; \n";
                        for (int j = 0; j < word.Word.Length; j++)
                            text += "_ ";
                        text.TrimEnd();
                        text += "\nLapis Bot 将在 30 秒后公布答案！";
            
                        _guessingGroupsMap.Add(source.GroupId.ToString(),
                            (word, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));


                        Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                        {
                            new CqReplyMsg(source.MessageId),
                            new CqTextMsg(text)
                        });
                        return Task.CompletedTask;
                    }
                }
            }

            Program.helpCommand.Parse(command, source);
            CancelCoolDownTimer(source.GroupId.ToString());
            return Task.CompletedTask;
        }

        private Task StartGuessing(int vocabularyIndex, CqGroupMessagePostContext source)
        {
            if (_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                {
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps guess words answer\"")
                });
                return Task.CompletedTask;
            }

            var i = new Random().Next(0, Vocabularies[vocabularyIndex].Words.Length);
            var word = Vocabularies[vocabularyIndex].Words[i];
            var text = "试试看吧！\n";
            foreach (WordDto.TranslationDto translation in word.Translations)
                text += translation.Type + "." + translation.Translation + "; \n";
            for (int j = 0; j < word.Word.Length; j++)
                text += "_ ";
            text.TrimEnd();
            text += "\nLapis Bot 将在 30 秒后公布答案！";
            
            _guessingGroupsMap.Add(source.GroupId.ToString(),
                (word, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));


            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(text)
            });
            return Task.CompletedTask;
        }
    }
}
