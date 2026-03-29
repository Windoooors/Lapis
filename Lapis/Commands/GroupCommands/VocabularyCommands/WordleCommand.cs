using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.ImageGenerators;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.VocabularyCommands;

public class WordleCommand : VocabularyCommandBase
{
    private const int GameDuration = 300;

    private readonly Dictionary<long, (WordleGame, DateTime)>
        _guessingGroupsMap = new();

    public WordleCommand()
    {
        CommandHead = "wordle";
        DirectCommandHead = "wordle";

        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("wordle", "1");
        IntendedArgumentCount = 2;
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
                AnnounceAnswer(keyWordDateTimePair.Item1, groupId, true, false));
            taskAnnounce.Start();
        }
    }

    private void AnnounceAnswer(WordleGame wordleGame, long groupId, bool gameOver, bool win, long messageId = -1)
    {
        var compressed = SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), groupId);

        var image = "base64://" + new WordleGameImageGenerator().Generate(wordleGame, compressed);

        var text = gameOver
            ? win
                ? $"答对啦！答案是：{wordleGame.Word.Word}\n以下是游戏历程："
                : $"游戏结束啦！答案是：{wordleGame.Word.Word}\n以下是游戏历程："
            : "";

        SendMessage(groupId, messageId != -1
            ?
            [
                new CqReplyMsg(messageId),
                text,
                new CqImageMsg(
                    image
                )
            ]
            :
            [
                text,
                new CqImageMsg(
                    image
                )
            ]);
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        StartGame(5, 6, source);
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!_guessingGroupsMap.TryGetValue(source.GroupId, out var game))
            return;

        var word = command.ToLower().Trim();

        if (!DictionaryCommand.DictionaryCommandInstance.TryLookUp(word, out var wordItem)) return;

        if (wordItem.Word.Length != game.Item1.WordLength)
        {
            SendMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                $"请发送长度为 {game.Item1.WordLength} 的单词"
            ]);

            return;
        }

        game.Item1.GuessedWords.Add(wordItem.Word);

        var win = word.ToLower() == game.Item1.Word.Word.ToLower();
        var gameOver = win || game.Item1.GuessedWords.Count == game.Item1.MaxTries;

        AnnounceAnswer(game.Item1, source.GroupId, gameOver, win, source.MessageId);

        if (gameOver) _guessingGroupsMap.Remove(source.GroupId);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (arguments[0] == "answer")
        {
            if (!_guessingGroupsMap.TryGetValue(source.GroupId, out var game))
            {
                SendMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    "当前没有正在进行的游戏哦\n发送 \"l wordle\" 可以开启一轮新的游戏！"
                ]);

                return;
            }

            AnnounceAnswer(game.Item1, source.GroupId, true, false, source.MessageId);
            _guessingGroupsMap.Remove(source.GroupId);

            return;
        }

        var wordLength = 5;
        var maxTries = 6;

        if (arguments.Length >= 2)
        {
            int.TryParse(arguments[0], out wordLength);
            int.TryParse(arguments[1], out maxTries);
        }

        if (arguments.Length == 1) int.TryParse(arguments[0], out wordLength);

        StartGame(wordLength, maxTries, source);
    }

    private void StartGame(int wordLength, int maxTries, CqGroupMessagePostContext source)
    {
        if (_guessingGroupsMap.ContainsKey(source.GroupId))
        {
            SendMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                "当前有游戏正在进行中"
            ]);

            AnnounceAnswer(_guessingGroupsMap[source.GroupId].Item1, source.GroupId, false, false);

            return;
        }

        if (wordLength < 5 || maxTries < 1 || maxTries > 10 || wordLength > 16)
        {
            SendMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                "不支持的参数"
            ]);
            
            return;
        }
        
        var game = new WordleGame(wordLength, maxTries);
        var random = new Random();

        while (true)
        {
            var categoryRandom = random.Next(0, VocabularyCommandInstance.LargeVocabulary.Words.Count);

            var categoryKeyString = VocabularyCommandInstance.LargeVocabulary.Words.Keys.ToArray()[categoryRandom];
            var category = VocabularyCommandInstance.LargeVocabulary.Words[categoryKeyString];

            var wordRandom = random.Next(0, category.Length);
            var word = category[wordRandom];

            if (word.Word.Length == wordLength && word.Word.ToList().All(char.IsLetter) &&
                !word.Word.ToList().All(char.IsUpper))
            {
                game.Word = word;
                break;
            }
        }

        SendMessage(source.GroupId, [
            new CqReplyMsg(source.MessageId),
            $"试试看吧！{BotConfiguration.Instance.BotName} 将在 {GameDuration} 秒后公布答案！"
        ]);

        AnnounceAnswer(game, source.GroupId, false, false);

        _guessingGroupsMap.Add(source.GroupId,
            (game, DateTime.Now.Add(new TimeSpan(0, 0, 0, GameDuration))));
    }

    public class WordleGame(int wordLength, int maxTries)
    {
        public int MaxTries = maxTries;

        public int WordLength = wordLength;
        public WordDto Word { get; set; }
        public HashSet<string> GuessedWords { get; set; } = [];
    }
}