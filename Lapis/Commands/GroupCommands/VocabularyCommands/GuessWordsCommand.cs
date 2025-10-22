using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.VocabularyCommands;

public class GuessWordsCommand : VocabularyCommandBase
{
    private readonly Dictionary<string, (WordDto, DateTime)> _guessingGroupsMap = new();

    public GuessWordsCommand()
    {
        CommandHead = "word|words|猜词";
        DirectCommandHead = "word|words|猜词";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("word", "1");
        IntendedArgumentCount = 1;
    }

    public override void Initialize()
    {
        Program.TimeChanged += TimeChanged;
    }

    private void TimeChanged(object obj, EventArgs e)
    {
        if (_guessingGroupsMap.Count == 0)
            return;
        for (var i = 0; i < _guessingGroupsMap.Count; i++)
        {
            if (_guessingGroupsMap.Values.ToArray()[i].Item2.Ticks > DateTime.Now.Ticks)
                continue;
            var keyWordDateTimePair = _guessingGroupsMap.Values.ToArray()[i];
            var groupId = _guessingGroupsMap.Keys.ToArray()[i];
            _guessingGroupsMap.Remove(groupId);
            var taskAnnounce = new Task(() =>
                AnnounceAnswer(keyWordDateTimePair.Item1, groupId, false, 0));
            taskAnnounce.Start();
        }
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
            return;
        _guessingGroupsMap.TryGetValue(source.GroupId.ToString(), out var keyWordDateTimePair);
        var word = keyWordDateTimePair.Item1;
        if (command.ToUpper() == word.Word.ToUpper())
        {
            AnnounceAnswer(word, source.GroupId.ToString(), true, source.MessageId);
            return;
        }

        var certified = false;

        foreach (var vocabulary in VocabularyCommandInstance.Vocabularies)
        foreach (var wordItem in vocabulary.Words)
            if (wordItem.Word == command)
            {
                certified = true;
                break;
            }

        if (!certified)
            return;

        var text = string.Empty;

        foreach (var wordCharacter in word.Word.ToLower())
        {
            var blankText = "_ ";
            foreach (var character in command.ToLower())
                if (character == wordCharacter)
                    blankText = character.ToString();

            text += blankText;
        }

        text = text.TrimEnd();

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("不对哦！");
        foreach (var translation in word.Translations)
            stringBuilder.AppendLine(translation.Type + "." + translation.Translation + ";");
        stringBuilder.AppendLine("提示：");
        stringBuilder.Append(text);

        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(stringBuilder.ToString())
        ]);
    }

    private void AnnounceAnswer(WordDto word, string groupId, bool won, long messageId)
    {
        _guessingGroupsMap.Remove(groupId);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(won ? "Bingo!" : "猜词结束啦！");
        stringBuilder.AppendLine("答案是：" + word.Word);
        foreach (var translation in word.Translations)
            stringBuilder.AppendLine(translation.Type + "." + translation.Translation + ";");
        stringBuilder.Remove(stringBuilder.Length - 1, 1);

        if (messageId != 0)
            SendMessage(long.Parse(groupId),
            [
                new CqReplyMsg(messageId),
                new CqTextMsg(stringBuilder.ToString())
            ]);
        else
            SendMessage(long.Parse(groupId),
            [
                new CqTextMsg(stringBuilder.ToString())
            ]);
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        var text = $"{BotConfiguration.Instance.BotName} 可从以下词库选取词语猜词\n";
        var i = 0;
        foreach (var file in Directory.GetFiles(AppContext.BaseDirectory + "resource/vocabulary/"))
        {
            if (Path.GetFileName(file) == ".DS_Store")
                continue;
            i++;
            text += i + "." + Path.GetFileName(file).Replace(Path.GetExtension(file), string.Empty) + "\n";
        }

        text += "请发送指令 \"l word 1\" 来用 " + Path.GetFileName(
                                             Directory.GetFileSystemEntries(AppContext.BaseDirectory +
                                                                            "resource/vocabulary/")[0]).Replace(
                                             Path.GetExtension(
                                                 Directory.GetFileSystemEntries(AppContext.BaseDirectory +
                                                     "resource/vocabulary/")[0]), string.Empty)
                                         + " 词库开始游戏";

        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(text)
        ]);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (arguments[0] == "answer")
        {
            if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("没有游戏正在进行喔！发送指令 \"l word 1\" 即可开启新一轮的游戏")
                ]);

                return;
            }

            for (var i = 0; i < _guessingGroupsMap.Count; i++)
                if (_guessingGroupsMap.Keys.ToArray()[i] == source.GroupId.ToString())
                    AnnounceAnswer(_guessingGroupsMap.Values.ToArray()[i].Item1, source.GroupId.ToString(), false,
                        source.MessageId);

            return;
        }

        var count = Directory.GetFiles(AppContext.BaseDirectory + "resource/vocabulary/").Length;
        var indexRegex = new Regex("^[1-" + count + "]$");
        if (indexRegex.IsMatch(arguments[0]))
        {
            StartGuessing(int.Parse(arguments[0]) - 1, source);
            return;
        }

        HelpCommand.Instance.ArgumentErrorHelp(source);
    }

    private void StartGuessing(int vocabularyIndex, CqGroupMessagePostContext source)
    {
        if (_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps word answer\"")
            ]);
            return;
        }

        var i = new Random().Next(0, VocabularyCommandInstance.Vocabularies[vocabularyIndex].Words.Length);
        var word = VocabularyCommandInstance.Vocabularies[vocabularyIndex].Words[i];
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("试试看吧！");
        foreach (var translation in word.Translations)
            stringBuilder.AppendLine(translation.Type + "." + translation.Translation + ";");
        for (var j = 0; j < word.Word.Length; j++)
            stringBuilder.Append("_ ");
        stringBuilder.AppendLine();
        stringBuilder.Append($"{BotConfiguration.Instance.BotName} 将在 30 秒后公布答案！");

        _guessingGroupsMap.Add(source.GroupId.ToString(),
            (word, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));

        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(stringBuilder.ToString())
        ]);
    }
}