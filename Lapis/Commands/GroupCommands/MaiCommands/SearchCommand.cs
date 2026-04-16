using System.Collections.Generic;
using System.Linq;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.ColorSpaces;

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
        var songsMatchedByAlias = new Dictionary<SongMetaData, List<string>>();
        var songsMatchedByArtist = new List<SongMetaData>();
        var songsMatchedByTitle = new List<SongMetaData>();
        var songsMatchedByBpm = new List<SongMetaData>();
        var isBpm = float.TryParse(keyWord, out var bpm);

        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;
        var songSet =
            db.SongMetaDataSet.Include(x => x.Charts);

        songsMatchedByTitle
            .AddRange(songSet.Where(x => EF.Functions.Like(x.Title.ToLower(), $"%{keyWord.ToLower().Replace(" ", "%")}%")
            ));

        var matchPattern = $"%{keyWord?.ToLower().Replace(" ", "%")}%";

        songsMatchedByArtist.AddRange(songSet.Where(x =>
            EF.Functions.Like(x.Artist.ToLower(), matchPattern)));

        var aliasesSet = db.SongAliasDataSet.Include(x => x.Aliases);
        
        var matchedAliases = aliasesSet
            .SelectMany(x => x.Aliases) 
            .Where(y => EF.Functions.Like(y.Alias, matchPattern))
            .Select(y => new AliasSongIdPair(y.Alias, y.SongId))
            .ToArray();
            
        foreach (var aliasItem in matchedAliases)
        {
            HashSet<SongMetaData> songs =
            [
                MaiCommandInstance.GetSongById(aliasItem.SimplifiedSongId) ??
                MaiCommandInstance.GetSongById(aliasItem.SimplifiedSongId + 10000)
            ];

            var dxSong = MaiCommandInstance.GetSongById(aliasItem.SimplifiedSongId + 10000);
            if (dxSong != null)
                songs.Add(dxSong);

            for (var i = 0; i < 16; i++)
            {
                var id = aliasItem.SimplifiedSongId + i * 10000 + 100000;

                var song = MaiCommandInstance.GetSongById(id);
                if (song != null)
                    songs.Add(song);
            }

            foreach (var songMetaData in songs)
            {
                if (songMetaData == null)
                    continue;

                var added = songsMatchedByAlias.TryAdd(songMetaData, [aliasItem.Alias]);
                if (!added && songsMatchedByAlias.TryGetValue(songMetaData, out var value)) value.Add(aliasItem.Alias);
            }
        }

        if (isBpm)
            songsMatchedByBpm.AddRange(songSet.Where(x => x.Bpm.Equals(bpm)));

        return new SearchResult(songsMatchedByArtist.ToArray(), songsMatchedByTitle.ToArray(),
            songsMatchedByBpm.ToArray(), songsMatchedByAlias);
    }

    private class AliasSongIdPair(string alias, int id)
    {
        public string Alias { get; set; } = alias;
        public int SimplifiedSongId { get; set; } = id;
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
                stringBuilder.AppendLine("ID " + song.SongId + " - " + song.Title + " [" + GetSongType(song.SongId) +
                                         "] （通过标题匹配）");

        if (songAliasDict.Count > 0)
            foreach (var pair in songAliasDict)
            {
                var aliasStringBuilder = new StringBuilder();
                foreach (var alias in pair.Value) aliasStringBuilder.AppendJoin(' ', $"\"{alias}\"");

                stringBuilder.AppendLine("ID " + pair.Key.SongId + " - " + pair.Key.Title + " [" +
                                         GetSongType(pair.Key.SongId) +
                                         $"] （通过别称 {aliasStringBuilder} 匹配）");
            }

        if (songsMatchedByArtist.Length > 0)
            foreach (var song in songsMatchedByArtist)
                stringBuilder.AppendLine("ID " + song.SongId + " - " + song.Title + " [" + GetSongType(song.SongId) +
                                         "] （通过曲师名匹配）");

        if (searchResult.SongsMatchedByBpm.Length > 0)
            foreach (var song in searchResult.SongsMatchedByBpm)
                stringBuilder.AppendLine("ID " + song.SongId + " - " + song.Title + " [" + GetSongType(song.SongId) +
                                         "] （通过 BPM 匹配）");

        return stringBuilder;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
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
                stringBuilder.Append($"*发送 \"lps mai info ID{exampleSong.SongId}\" 指令即可查询歌曲 {
                    MaiCommandInstance.GetSongById(exampleSong.SongId).Title} [{GetSongType(exampleSong.SongId)}] 的信息");
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
        public readonly SongMetaData[] AllSongs;
        public readonly Dictionary<SongMetaData, List<string>> SongsMatchedByAlias;
        public readonly SongMetaData[] SongsMatchedByArtist;
        public readonly SongMetaData[] SongsMatchedByBpm;
        public readonly SongMetaData[] SongsMatchedByTitle;

        public SearchResult(SongMetaData[] songsMatchedByArtist, SongMetaData[] songsMatchedByTitle,
            SongMetaData[] songsMatchedByBpm,
            Dictionary<SongMetaData, List<string>> songsMatchedByAlias)
        {
            SongsMatchedByAlias = songsMatchedByAlias;
            SongsMatchedByTitle = songsMatchedByTitle;
            SongsMatchedByArtist = songsMatchedByArtist;
            SongsMatchedByBpm = songsMatchedByBpm;
            var allSongsList = new List<SongMetaData>();
            allSongsList.AddRange(songsMatchedByTitle);
            allSongsList.AddRange(songsMatchedByBpm);
            allSongsList.AddRange(songsMatchedByArtist);
            allSongsList.AddRange(songsMatchedByAlias.Keys);
            AllSongs = allSongsList.ToArray();
        }
    }
}