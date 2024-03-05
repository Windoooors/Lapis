using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Sessions.Http.Managers;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class WordDto
    {
        [JsonProperty("word")] public string Word;
        [JsonProperty("translations")] public TranslationDto[] Translations;
        public class TranslationDto
        {
            [JsonProperty("translation")] public string Translation;
            [JsonProperty("type")] public string Type;
        }
    }

    public class Vocabulary
    {
        public WordDto[] Words;
    }
    
    public class GuessWordsCommand : GroupCommand
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

            int i = 0;

            foreach (string file in Directory.GetFiles(AppContext.BaseDirectory + "resource/vocabulary/"))
            {
                if (Path.GetFileName(file) == ".DS_Store")
                    continue;
                var jsonString = File.ReadAllText(file);
                _vocabularies.Add(new Vocabulary() { Words = JsonConvert.DeserializeObject<WordDto[]>(jsonString)});
            }
            
            Program.TimeChanged += TimeChanged;

            return Task.CompletedTask;
        }

        private List<Vocabulary> _vocabularies = new List<Vocabulary>();
        
        private void TimeChanged(object obj, EventArgs e)
        {
            if (_guessingGroupsMap.Count == 0)
                return;
            for (int i = 0; i < _guessingGroupsMap.Count; i++)
            {
                //Console.WriteLine(_guessingGroupsMap.Values.ToArray()[i].Item2.Ticks + " " + DateTime.Now.Ticks);
                if (!(_guessingGroupsMap.Values.ToArray()[i].Item2.Ticks <= DateTime.Now.Ticks))
                    return;
                var keyWordDateTimePair = _guessingGroupsMap.Values.ToArray()[i];
                var groupId = _guessingGroupsMap.Keys.ToArray()[i];
                var taskAnnounce = new Task(() =>
                    AnnounceAnswer(keyWordDateTimePair.Item1, groupId, false, null));
                taskAnnounce.Start();
            }
        }

        public override Task RespondWithoutParsingCommand(string command, GroupMessageReceiver source)
        {
            if (!_guessingGroupsMap.ContainsKey(source.GroupId))
                return Task.CompletedTask;
            var keyWordDateTimePair = (new WordDto(), DateTime.MinValue);
            _guessingGroupsMap.TryGetValue(source.GroupId, out keyWordDateTimePair);
            var word = keyWordDateTimePair.Item1;
            if (command.ToUpper() == word.Word.ToUpper())
            {
                AnnounceAnswer(word, source.GroupId, true, source.Sender.Id);
                return Task.CompletedTask;
            }

            var cretified = false;
            
            foreach (Vocabulary vocabulary in _vocabularies)
            {
                foreach (WordDto wordItem in vocabulary.Words)
                {
                    if (wordItem.Word == command)
                    {
                        cretified = true;
                        break;
                    }
                }
            }
            
            if (!cretified)
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

            MessageManager.SendGroupMessageAsync(source.GroupId,
                new MessageChain() { new AtMessage() { Target = source.Sender.Id }, new PlainMessage(" " + titleText) });
            
            return Task.CompletedTask;
        }

        private Task AnnounceAnswer(WordDto word, string groupId, bool won, string senderId)
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

            if (senderId != null)
                MessageManager.SendGroupMessageAsync(groupId,
                    new MessageChain()
                        { new AtMessage() { Target = senderId }, new PlainMessage(" " + text) });
            else
                MessageManager.SendGroupMessageAsync(groupId,
                    new MessageChain() { new PlainMessage(text) });

            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
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
            MessageManager.SendGroupMessageAsync(source.GroupId,
                new MessageChain() { new AtMessage() { Target = source.Sender.Id }, new PlainMessage(" " + text) });
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
                            new PlainMessage(" 没有游戏正在进行喔！发送指令 \"l guess words 1\" 即可开启新一轮的游戏")
                        });
                    return Task.CompletedTask;
                }

                for (int i = 0; i < _guessingGroupsMap.Count; i++)
                {
                    if (_guessingGroupsMap.Keys.ToArray()[i] == source.GroupId)
                        AnnounceAnswer(_guessingGroupsMap.Values.ToArray()[i].Item1, source.GroupId, false, source.Sender.Id);
                }
                
                return Task.CompletedTask;
            }

            var count = Directory.GetFiles(AppContext.BaseDirectory + "resource/vocabulary/").Length;
            var indexRegex = new Regex("^[1-" + count + "]$");
            if (indexRegex.IsMatch(command))
            {
                StartGuessing(int.Parse(command) - 1, source);
                return Task.CompletedTask;
            }

            Program.helpCommand.Parse(command, source);
            return Task.CompletedTask;
        }

        private Task StartGuessing(int vocabularyIndex, GroupMessageReceiver source)
        {
            if (_guessingGroupsMap.ContainsKey(source.GroupId))
            {
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain()
                    {
                        new AtMessage() { Target = source.Sender.Id },
                        new PlainMessage(" 本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps guess words answer\"")
                    });
                return Task.CompletedTask;
            }

            var i = new Random().Next(0, _vocabularies[vocabularyIndex].Words.Length);
            var word = _vocabularies[vocabularyIndex].Words[i];
            var text = "试试看吧！\n";
            foreach (WordDto.TranslationDto translation in word.Translations)
                text += translation.Type + "." + translation.Translation + "; \n";
            for (int j = 0; j < word.Word.Length; j++)
                text += "_ ";
            text.TrimEnd();
            text += "\nLapis Bot 将在 30 秒后公布答案！";
            
            _guessingGroupsMap.Add(source.GroupId,
                (word, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));

            MessageManager.SendGroupMessageAsync(source.GroupId,
                new MessageChain() { new AtMessage() { Target = source.Sender.Id }, new PlainMessage(" " + text) });
            return Task.CompletedTask;
        }
    }
}
