using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Miscellaneous;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class SearchCommand : MaiCommandBase
{
    private readonly string[] _specialCharacters =
    [
        ".", ",", ";", ":", "?", "!", "\"", "'",
        "(", ")", "[", "]", "{", "}", "<", ">",
        "~", "-", "=", "+", "*", "/", "\\",
        "%", "&", "#", "@", "$", "。", "，", "；", "：", "？", "！", "“”", "‘’",
        "（", "）", "［", "］", "｛", "｝", "〈", "〉",
        "～", "－", "＝", "＋", "×", "／", "＼",
        "％", "＆", "＃", "＠", "＄"
    ];

    public SearchCommand()
    {
        CommandHead = new Regex("^search");
        DirectCommandHead = new Regex("^search|^查歌|^搜歌|^搜索|^索引");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("search", "1");
    }

    public static SearchCommand SearchCommandInstance { get; } = new();

    private bool IsSameKeywords(string[] keywords)
    {
        var firstKeyword = keywords[0];
        foreach (var keyword in keywords)
            if (keyword != firstKeyword)
                return false;

        return true;
    }

    private bool IsMatch(string keyword, string input)
    {
        var inputWithNoSpecialCharacters = input;
        foreach (var specialCharacter in _specialCharacters)
            inputWithNoSpecialCharacters = inputWithNoSpecialCharacters.Replace(specialCharacter, string.Empty);

        var kanjiKeyword = HanziToKanjiConverter.Convert(keyword);

        return IsMatchBase(keyword, input) ||
               IsMatchBase(keyword, inputWithNoSpecialCharacters) ||
               IsMatchBase(kanjiKeyword, input) ||
               IsMatchBase(kanjiKeyword, inputWithNoSpecialCharacters);
    }

    private bool IsMatchBase(string pattern, string input)
    {
        if (pattern.ToLower() == input.ToLower())
            return true;

        var splitResult = pattern.ToLower().Split(' ');
        var patterns = splitResult.Length == 0 ? [pattern.ToLower()] : splitResult;
        var allMatched = true;

        if (patterns.Length > 1 && IsSameKeywords(patterns))
        {
            var regex = new Regex(patterns[0]);
            var matches = regex.Matches(input.ToLower());
            if (matches.Count > 1)
                return true;
            return false;
        } // 处理例如通过 break break break 的关键词匹配歌曲 BREaK! BREaK! BREaK! 的情况

        foreach (var regexString in patterns)
        {
            var regex = new Regex(regexString);

            if (!regex.IsMatch(input.ToLower()))
                allMatched = false;
        }

        return allMatched;
    }

    public SearchResult Search(string keyWord)
    {
        var songsMatchedByAlias = new Dictionary<SongDto, List<string>>();
        var songsMatchedByArtist = new List<SongDto>();
        var songsMatchedByTitle = new List<SongDto>();
        var allSongs = new List<SongDto>();

        foreach (var song in MaiCommandInstance.Songs)
        {
            var aliases = MaiCommandInstance.GetAliasById(song.Id).Aliases;

            foreach (var alias in aliases)
                if (IsMatch(keyWord, alias))
                {
                    if (!songsMatchedByAlias.ContainsKey(song))
                    {
                        songsMatchedByAlias.Add(song, [alias]);
                        continue;
                    }

                    songsMatchedByAlias[song].Add(alias);
                }

            if (IsMatch(keyWord, song.BasicInfo.Artist) && !songsMatchedByArtist.Contains(song))
                songsMatchedByArtist.Add(song);

            if (IsMatch(keyWord, song.Title) && !songsMatchedByTitle.Contains(song))
                songsMatchedByTitle.Add(song);
        }

        allSongs.AddRange(songsMatchedByTitle);
        allSongs.AddRange(songsMatchedByArtist);
        allSongs.AddRange(songsMatchedByAlias.Keys);

        return new SearchResult
        {
            SongsMatchedByArtist = songsMatchedByArtist,
            SongsMatchedByTitle = songsMatchedByTitle,
            SongsMatchedByAlias = songsMatchedByAlias,
            AllSongs = allSongs
        };
    }

    public StringBuilder GetMultiSearchResults(SearchResult searchResult)
    {
        var songAliasDict = searchResult.SongsMatchedByAlias;
        var songsMatchedByArtist = searchResult.SongsMatchedByArtist;
        var songs = searchResult.SongsMatchedByTitle;

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("找到了以下歌曲：");

        if (songs.Count > 0)
            foreach (var song in songs)
                stringBuilder.AppendLine("ID " + song.Id + " - " + song.Title + " [" + song.Type + "] （通过标题匹配）");

        if (songAliasDict.Count > 0)
            foreach (var pair in songAliasDict)
            {
                var aliasStringBuilder = new StringBuilder();
                foreach (var alias in pair.Value) aliasStringBuilder.AppendJoin(' ', $"\"{alias}\"");

                stringBuilder.AppendLine("ID " + pair.Key.Id + " - " + pair.Key.Title + " [" + pair.Key.Type +
                                         $"] （通过别称 {aliasStringBuilder} 匹配）");
            }

        if (songsMatchedByArtist.Count > 0)
            foreach (var song in songsMatchedByArtist)
                stringBuilder.AppendLine("ID " + song.Id + " - " + song.Title + " [" + song.Type + "] （通过曲师名匹配）");

        return stringBuilder;
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        var searchResult = Search(command);

        var stringBuilder = new StringBuilder();

        if (searchResult.AllSongs.Count >= 30)
        {
            stringBuilder.AppendLine("搜索结果过多，请提供更多关键词");
        }
        else
        {
            stringBuilder = GetMultiSearchResults(searchResult);

            if (searchResult.AllSongs.Count != 0)
            {
                var exampleSong = searchResult.AllSongs[0];
                stringBuilder.Append($"*发送 \"lps mai info ID {exampleSong.Id}\" 指令即可查询歌曲 {
                    MaiCommandInstance.GetSong(exampleSong.Id).Title} [{MaiCommandInstance.GetSong(exampleSong.Id).Type}] 的信息");
            }
            else
            {
                stringBuilder.Clear();
                stringBuilder = new StringBuilder("未找到该歌曲");
            }
        }

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(stringBuilder.ToString())
        ]);
    }

    public class SearchResult
    {
        public List<SongDto> AllSongs = new();
        public Dictionary<SongDto, List<string>> SongsMatchedByAlias = new();
        public List<SongDto> SongsMatchedByArtist = new();
        public List<SongDto> SongsMatchedByTitle = new();
    }
}