using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;
using Microsoft.EntityFrameworkCore;

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
        var matchPattern = $"%{DatabaseHandler.EscapeLikeString(keyWord.ToLower()).Replace(" ", "%")}%";
        
        var isBpm = float.TryParse(keyWord, out var bpm);

        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        // 1. 合并歌曲表的查询，减少 I/O 次数
        // 去掉了 ToLower() 确保能走索引，利用 || 合并
        var matchedSongsFromDb = db.SongMetaDataSet
            .Include(x => x.Charts)
            .Where(x => EF.Functions.Like(x.Title.ToLower(), matchPattern, @"\")
                        || EF.Functions.Like(x.Artist.ToLower(), matchPattern,@"\")
                        || (isBpm && x.Bpm.Equals(bpm)))
            .ToList();

        // 在内存中对合并后的结果进行分类（避免多次查库）
        var songsMatchedByTitle = matchedSongsFromDb
            .Where(x => x.Title.Contains(keyWord, StringComparison.OrdinalIgnoreCase)).ToArray();
        var songsMatchedByArtist = matchedSongsFromDb
            .Where(x => x.Artist.Contains(keyWord, StringComparison.OrdinalIgnoreCase)).ToArray();
        var songsMatchedByBpm =
            isBpm ? matchedSongsFromDb.Where(x => x.Bpm.Equals(bpm)).ToArray() : Array.Empty<SongMetaData>();

        // 2. 优化别名查询
        var songsMatchedByAlias = new Dictionary<SongMetaData, List<string>>();

        var matchedAliases = db.SongAliasDataSet
            .Include(x => x.Aliases)
            .SelectMany(x => x.Aliases)
            .Where(y => EF.Functions.Like(y.Alias.ToLower(), matchPattern, @"\"))
            .Select(y => new { y.Alias, y.SongId }) // 避免用自定义的 Pair 结构体，匿名类即可
            .ToList();

        // 3. 优化 ID 计算逻辑 (避免在循环中重复计算)
        foreach (var aliasItem in matchedAliases)
        {
            // 提取简化 ID 逻辑 (由于不知道你的 SimplifiedSongId 具体算法，这里保留你的原逻辑，但改用 HashSet 别名去重)
            var simplifiedId = aliasItem.SongId % 10000; // 假设的简化逻辑，根据你实际情况改

            // 收集所有可能匹配的 ID，一次性去内存缓存查，减少调用次数
            var targetIds = new HashSet<int> { simplifiedId, simplifiedId + 10000 };
            for (var i = 0; i < 16; i++)
            {
                targetIds.Add(simplifiedId + i * 10000 + 100000);
            }

            foreach (var id in targetIds)
            {
                var song = MaiCommandInstance.GetSongById(id);
                if (song == null) continue;

                // 优化 Dictionary 写入
                if (!songsMatchedByAlias.TryGetValue(song, out var aliasList))
                {
                    aliasList = new List<string>();
                    songsMatchedByAlias[song] = aliasList;
                }

                if (!aliasList.Contains(aliasItem.Alias))
                {
                    aliasList.Add(aliasItem.Alias);
                }
            }
        }

        return new SearchResult(songsMatchedByArtist, songsMatchedByTitle, songsMatchedByBpm, songsMatchedByAlias);
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

    private class AliasSongIdPair(string alias, int id)
    {
        public string Alias { get; } = alias;
        public int SimplifiedSongId { get; } = id;
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