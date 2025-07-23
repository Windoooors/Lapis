using System.Collections.Generic;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Miscellaneous;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class SearchCommand : MaiCommandBase
{
    public SearchCommand()
    {
        CommandHead = "search";
        DirectCommandHead = "search|查歌|搜歌|搜索|索引";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("search", "1");
        SearchCommandInstance = this;
        IntendedArgumentCount = 1;
    }

    public static SearchCommand SearchCommandInstance { get; private set; }

    public SearchResult Search(string keyWord)
    {
        var songsMatchedByAlias = new Dictionary<SongDto, List<string>>();
        var songsMatchedByArtist = new List<SongDto>();
        var songsMatchedByTitle = new List<SongDto>();
        var songsMatchedByBpm = new List<SongDto>();

        var bpm = 0m;
        
        var isBpm = float.TryParse(keyWord, out _) && decimal.TryParse(keyWord, out bpm);

        foreach (var song in MaiCommandInstance.Songs)
        {
            var aliases = MaiCommandInstance.GetAliasById(song.Id).Aliases;

            foreach (var alias in aliases)
                if (Searcher.Instance.IsMatch(keyWord, alias))
                {
                    if (!songsMatchedByAlias.ContainsKey(song))
                    {
                        songsMatchedByAlias.Add(song, [alias]);
                        continue;
                    }

                    songsMatchedByAlias[song].Add(alias);
                }

            if (Searcher.Instance.IsMatch(keyWord, song.BasicInfo.Artist) && !songsMatchedByArtist.Contains(song))
                songsMatchedByArtist.Add(song);

            if (Searcher.Instance.IsMatch(keyWord, song.Title) && !songsMatchedByTitle.Contains(song))
                songsMatchedByTitle.Add(song);

            if (isBpm && song.BasicInfo.Bpm == bpm)
                songsMatchedByBpm.Add(song);
        }

        return new SearchResult(songsMatchedByArtist.ToArray(), songsMatchedByTitle.ToArray(),
            songsMatchedByBpm.ToArray(), songsMatchedByAlias);
    }

    public StringBuilder GetMultiSearchResults(SearchResult searchResult)
    {
        var songAliasDict = searchResult.SongsMatchedByAlias;
        var songsMatchedByArtist = searchResult.SongsMatchedByArtist;
        var songs = searchResult.SongsMatchedByTitle;

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("找到了以下歌曲：");

        if (songs.Length > 0)
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

        if (songsMatchedByArtist.Length > 0)
            foreach (var song in songsMatchedByArtist)
                stringBuilder.AppendLine("ID " + song.Id + " - " + song.Title + " [" + song.Type + "] （通过曲师名匹配）");

        if (searchResult.SongsMatchedByBpm.Length > 0)
            foreach (var song in searchResult.SongsMatchedByBpm)
                stringBuilder.AppendLine("ID " + song.Id + " - " + song.Title + " [" + song.Type + "] （通过 BPM 匹配）");

        return stringBuilder;
    }

    public override void ParseWithArgument(string[] arguments, CqGroupMessagePostContext source)
    {
        var searchResult = Search(arguments[0]);

        var stringBuilder = new StringBuilder();

        if (searchResult.AllSongs.Length >= 100)
        {
            stringBuilder.AppendLine("搜索结果过多，请提供更多关键词");
        }
        else
        {
            stringBuilder = GetMultiSearchResults(searchResult);

            if (searchResult.AllSongs.Length != 0)
            {
                var exampleSong = searchResult.AllSongs[0];
                stringBuilder.Append($"*发送 \"lps mai info ID{exampleSong.Id}\" 指令即可查询歌曲 {
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
        public readonly SongDto[] AllSongs;
        public readonly Dictionary<SongDto, List<string>> SongsMatchedByAlias;
        public readonly SongDto[] SongsMatchedByArtist;
        public readonly SongDto[] SongsMatchedByBpm;
        public readonly SongDto[] SongsMatchedByTitle;

        public SearchResult(SongDto[] songsMatchedByArtist, SongDto[] songsMatchedByTitle, SongDto[] songsMatchedByBpm,
            Dictionary<SongDto, List<string>> songsMatchedByAlias)
        {
            SongsMatchedByAlias = songsMatchedByAlias;
            SongsMatchedByTitle = songsMatchedByTitle;
            SongsMatchedByArtist = songsMatchedByArtist;
            SongsMatchedByBpm = songsMatchedByBpm;
            var allSongsList = new List<SongDto>();
            allSongsList.AddRange(songsMatchedByTitle);
            allSongsList.AddRange(songsMatchedByBpm);
            allSongsList.AddRange(songsMatchedByArtist);
            allSongsList.AddRange(songsMatchedByAlias.Keys);
            AllSongs = allSongsList.ToArray();
        }
    }
}