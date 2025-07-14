using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Miscellaneous;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class LettersCommand : MaiCommandBase
{
    private readonly Dictionary<string, (SongList, DateTime)>
        _guessingGroupsMap = new();

    public LettersCommand()
    {
        CommandHead = "letters";
        DirectCommandHead = "letters|开字母";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("letter", "1");
        IntendedArgumentCount = 1;
    }

    public override void Initialize()
    {
        Program.TimeChanged += TimeChanged;
    }

    private SongDto GetRandomSong(SpecialCharactersToBeIncluded specialCharactersToBeIncluded)
    {
        var pattern = new Regex("");
        switch (specialCharactersToBeIncluded)
        {
            case SpecialCharactersToBeIncluded.Chinese:
                pattern = new Regex(SharedConsts.JapaneseCharacterRegexString);
                break;
            case SpecialCharactersToBeIncluded.Japanese:
                pattern = new Regex(SharedConsts.ChineseCharacterRegexString);
                break;
            case SpecialCharactersToBeIncluded.Neither:
                pattern = new Regex(
                    $"{SharedConsts.JapaneseCharacterRegexString}|{SharedConsts.ChineseCharacterRegexString}");
                break;
        }

        var random = new Random();
        var index = random.Next(0, MaiCommandInstance.Songs.Length);

        if (specialCharactersToBeIncluded == SpecialCharactersToBeIncluded.Both) return MaiCommandInstance.Songs[index];

        while (pattern.IsMatch(MaiCommandInstance.Songs[index].Title))
            index = random.Next(0, MaiCommandInstance.Songs.Length);
        return MaiCommandInstance.Songs[index];
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

        return new SongList { AllSongs = songs.ToArray() };
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
                AnnounceAnswer(keyWordDateTimePair.Item1, groupId, 0, true));
            taskAnnounce.Start();
        }
    }

    private void AnnounceAnswer(SongList songs, string groupId, long messageId, bool gameOver)
    {
        if (gameOver)
            _guessingGroupsMap.Remove(groupId);

        var title = "舞萌开字母结束啦！\n答案是：";

        var songNames = "";
        var index = 1;

        foreach (var song in songs.AllSongs)
        {
            if (gameOver)
            {
                songNames += $"{index}.{song.Title}\n";
            }
            else
            {
                var songDisplay = string.Empty;

                var hiddenLetterCount = 0;

                foreach (var songNameCharacter in song.Title)
                {
                    var blankMarkChar = new[] { ' ', '\u3000', '\u200e' }.Contains(songNameCharacter)
                        ? songNameCharacter
                        : '*';

                    foreach (var character in songs.GuessedLetters)
                        if (char.ToLower(character) == char.ToLower(songNameCharacter))
                            blankMarkChar = songNameCharacter;

                    if (blankMarkChar == '*')
                        hiddenLetterCount++;

                    songDisplay += blankMarkChar;
                }

                songDisplay = songDisplay.TrimEnd();

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
                        return;
                    }
                }

                songNames += $"{index}.{songDisplay}\n";
            }

            index++;
        }

        songNames = songNames.TrimEnd();

        var text = gameOver ? $"{title}\n{songNames}" : $"{songNames}\n已开字母：{string.Join(", ", songs.GuessedLetters)}";

        if (messageId != 0)
            Program.Session.SendGroupMessage(long.Parse(groupId), [
                new CqReplyMsg(messageId),
                new CqTextMsg(text)
            ]);
        else
            Program.Session.SendGroupMessage(long.Parse(groupId), [
                new CqTextMsg(text)
            ]);
    }

    private void StartGuessing(CqGroupMessagePostContext source,
        SpecialCharactersToBeIncluded specialCharactersToBeIncluded)
    {
        if (_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
        {
            var messageStringBuilder = new StringBuilder();
            messageStringBuilder.AppendLine("本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps mai letters answer\"");
            messageStringBuilder.AppendLine("要开字母 a，请发送指令 \"开 a\"");
            messageStringBuilder.Append("要猜编号为 \\\"1\\\" 的歌曲为 [SD] LUCIA, 请发送指令 \\\"1.LUCIA\\\"");

            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(messageStringBuilder.ToString())
            ]);
            return;
        }

        var excludedCharacterInformation = "";
        if (specialCharactersToBeIncluded == SpecialCharactersToBeIncluded.Japanese)
            excludedCharacterInformation = "本次开字母游戏中的谜底均不包含中文字符\n";
        else if (specialCharactersToBeIncluded == SpecialCharactersToBeIncluded.Chinese)
            excludedCharacterInformation = "本次开字母游戏中的谜底均不包含日文字符\n";
        else if (specialCharactersToBeIncluded == SpecialCharactersToBeIncluded.Neither)
            excludedCharacterInformation = "本次开字母游戏中的谜底均不包含中、日文字符\n";

        var titleStringBuilder = new StringBuilder();
        titleStringBuilder.AppendLine(
            $"{excludedCharacterInformation}{BotConfiguration.Instance.BotName} 将在 20mins 后公布答案！");
        titleStringBuilder.AppendLine("要开字母 a，请发送指令 \"开 a\"");
        titleStringBuilder.AppendLine("要猜编号为 \"1\" 的歌曲为 [SD] LUCIA, 请发送指令 \"1.LUCIA\"");

        var songNamesStringBuilder = new StringBuilder();
        var index = 1;

        var songs = GenerateSongList(specialCharactersToBeIncluded);

        foreach (var song in songs.AllSongs)
        {
            var songDisplay = "";

            foreach (var songNameCharacter in song.Title)
            {
                var blankMarkChar = new[] { ' ', '\u3000' }.Contains(songNameCharacter) ? songNameCharacter : '*';

                songDisplay += blankMarkChar;
            }

            songNamesStringBuilder.AppendLine($"{index}.{songDisplay}");

            index++;
        }

        songNamesStringBuilder.Remove(songNamesStringBuilder.Length - 1, 1);

        SendMessage(source,
            [$"{titleStringBuilder}{songNamesStringBuilder}"]);

        _guessingGroupsMap.Add(source.GroupId.ToString(),
            (songs, DateTime.Now.Add(new TimeSpan(0, 0, 20, 0))));
    }

    public override void Parse(CqGroupMessagePostContext source)
    {
        StartGuessing(source, SpecialCharactersToBeIncluded.Both);
    }

    public override void ParseWithArgument(string[] arguments, CqGroupMessagePostContext source)
    {
        var command = arguments[0];

        var specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Both;

        if (command.ToLower().StartsWith("both"))
        {
            specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Both;
        }
        else if (command.ToLower().StartsWith("neither"))
        {
            specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Neither;
        }
        else if (command.ToLower().StartsWith("chinese"))
        {
            specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Chinese;
        }
        else if (command.ToLower().StartsWith("japanese"))
        {
            specialCharactersToBeIncluded = SpecialCharactersToBeIncluded.Japanese;
        }
        else if (command.ToLower() == "answer")
        {
            (SongList, DateTime) keyWordDateTimePair;
            _guessingGroupsMap.TryGetValue(source.GroupId.ToString(), out keyWordDateTimePair);

            if (keyWordDateTimePair.Item1 == null)
            {
                SendMessage(source,
                    [new CqReplyMsg(source.MessageId), "现在没有正在进行的舞萌开字母游戏！"]);
                return;
            }

            AnnounceAnswer(keyWordDateTimePair.Item1, source.GroupId.ToString(), source.MessageId, true);
            return;
        }
        else
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "不支持的参数类型"]);
            return;
        }

        StartGuessing(source, specialCharactersToBeIncluded);
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        _guessingGroupsMap.TryGetValue(source.GroupId.ToString(), out var keyValuePair);

        if (keyValuePair.Item1 == null)
            return;

        if (command.StartsWith("开 "))
        {
            var targetLetter = command.Replace("开 ", "");
            if (targetLetter == "" || targetLetter.Length > 1)
            {
                SendMessage(source, [new CqReplyMsg(source.MessageId), "不支持的参数类型"]);
                return;
            }

            var targetChar = targetLetter[0];
            if (!keyValuePair.Item1.GuessedLetters.Contains(char.ToLower(targetChar)))
                keyValuePair.Item1.GuessedLetters.Add(char.ToLower(targetChar));

            AnnounceAnswer(keyValuePair.Item1, source.GroupId.ToString(), source.MessageId, false);
            return;
        }

        var indexPattern = new Regex(@"^[1-9][0-9]\.|^[1-9]\.");

        if (!indexPattern.IsMatch(command))
            return;

        var index = int.Parse(new Regex("^[1-9][0-9]|^[1-9]").Match(command).ToString());
        if (index > 15 || index < 1)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "不支持的参数类型"]);
            return;
        }

        var songIndicator = indexPattern.Replace(command, "", 1);
        if (songIndicator == "")
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "不支持的参数类型"]);
            return;
        }

        var songs = MaiCommandInstance.GetSongs(songIndicator);

        if (songs == null)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "未开出歌曲"]);
            return;
        }

        foreach (var song in songs)
        {
            if (keyValuePair.Item1.AllSongs[index - 1] != song)
                continue;

            if (keyValuePair.Item1.GuessedSongs.Contains(song))
            {
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId), "该歌曲已经被开出"
                ]);
                return;
            }

            keyValuePair.Item1.GuessedSongs.Add(song);

            if (keyValuePair.Item1.GuessedSongs.Count == keyValuePair.Item1.AllSongs.Length)
            {
                AnnounceAnswer(keyValuePair.Item1, source.GroupId.ToString(), source.MessageId, true);
                return;
            }

            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId), $"开出歌曲：\"{song.Title}\" [{song.Type.ToUpper()}]"
            ]);
            AnnounceAnswer(keyValuePair.Item1, source.GroupId.ToString(), source.MessageId, false);
            return;
        }

        SendMessage(source, [new CqReplyMsg(source.MessageId), "未开出歌曲"]);
    }

    private class SongList
    {
        public readonly List<char> GuessedLetters = new();
        public readonly List<SongDto> GuessedSongs = new();
        public SongDto[] AllSongs;
    }

    private enum SpecialCharactersToBeIncluded
    {
        Chinese,
        Japanese,
        Both,
        Neither
    }
}