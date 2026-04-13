using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.MaiCommands;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Lapis.Operations.DatabaseOperation;
using Lapis.Operations.MaiScoreOperation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands;

public abstract class MaiCommandBase : GroupCommand
{
    public enum Rate
    {
        D,
        C,
        B,
        Bb,
        Bbb,
        A,
        Aa,
        Aaa,
        S,
        Sp,
        Ss,
        Ssp,
        Sss,
        Sssp
    }

    public enum SongType
    {
        DX,
        SD
    }

    public static MaiCommand MaiCommandInstance;
    protected readonly Regex IdHeadRegex = new(@"^id\s|^id|^ID\s|^ID");
    protected readonly Regex IdRegex = new(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");

    public readonly MaiScoreOperator MaiScoreOperator = new();

    protected void DivingFishErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("与水鱼网通信时出现问题")
        ]);
    }

    protected void UnboundErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("您没有绑定“舞萌 DX | 中二节奏查分器”账户，请前往 https://www.diving-fish.com/maimaidx/prober 进行绑定")
        ]);
    }

    public static SongType GetSongType(int songId)
    {
        return songId switch
        {
            >= 10000 => SongType.DX,
            < 10000 => SongType.SD
        };
    }

    protected void ForbiddenErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("您禁用了非网页成绩查询")
        ]);
    }

    protected void ObjectUserUnboundErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("未找到该用户")
        ]);
    }

    public string GetMultiAliasesMatchedInformationString(SongMetaData[] songs,
        CommandBehaviorInformationDataObject behaviorInformation)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("该别称有多首歌曲匹配：");

        foreach (var song in songs)
            stringBuilder.AppendLine($"ID {song.SongId} - {song.Title} [{GetSongType(song.SongId)}]");

        stringBuilder.Append(
            behaviorInformation.ExtraParameterStrings == null
                ? $"*发送 \"lps mai {behaviorInformation.CommandString} ID{songs[0].SongId}\" 指令即可查询歌曲 {songs[0].Title} [{GetSongType(songs[0].SongId)}] 的{behaviorInformation.FunctionString}"
                : $"*发送 \"lps mai {behaviorInformation.CommandString} ID{songs[0].SongId} {
                    behaviorInformation.ExtraParameterStrings.Aggregate(
                        new StringBuilder(), (builder, item) => builder.Append(item).Append(' ')
                    ).ToString().Trim()
                }\" 指令即可{(behaviorInformation.ContentModification ? "为歌曲" : "查询歌曲")} {songs[0].Title} [{GetSongType(songs[0].SongId)}] {(behaviorInformation.ContentModification ? "" : "的")}{behaviorInformation.FunctionString}"
        );

        return stringBuilder.ToString();
    }

    public string GetMultiSearchResultInformationString(string keyword,
        CommandBehaviorInformationDataObject behaviorInformation)
    {
        var searchResult = SearchCommand.SearchCommandInstance.Search(keyword);

        var stringBuilder = new StringBuilder();

        if (searchResult.AllSongs.Length >= 100)
        {
            stringBuilder.AppendLine("搜索结果过多，请提供更多关键词");
        }
        else
        {
            stringBuilder = SearchCommand.SearchCommandInstance.GetMultiSearchResults(searchResult);

            if (searchResult.AllSongs.Length != 0)
            {
                var exampleSong = searchResult.AllSongs[0];
                stringBuilder.Append(
                    behaviorInformation.ExtraParameterStrings == null
                        ? $"*发送 \"lps mai {behaviorInformation.CommandString} ID{exampleSong.SongId}\" 指令即可查询歌曲 {exampleSong.Title} [{GetSongType(exampleSong.SongId)}] 的{behaviorInformation.FunctionString}"
                        : $"*发送 \"lps mai {behaviorInformation.CommandString} ID{exampleSong.SongId} {behaviorInformation.ExtraParameterStrings.Aggregate(
                            new StringBuilder(), (builder, item) => builder.Append(item).Append(' ')
                        ).ToString().Trim()}\" 指令即可{(behaviorInformation.ContentModification ? "为歌曲" : "查询歌曲")} {exampleSong.Title} [{GetSongType(exampleSong.SongId)}] {(behaviorInformation.ContentModification ? "" : "的")}{behaviorInformation.FunctionString}");
            }
            else
            {
                stringBuilder.Clear();
                stringBuilder = new StringBuilder("未找到该歌曲");
            }
        }

        return stringBuilder.ToString();
    }

    public static string GetSongPath(int id)
    {
        return AppContext.BaseDirectory + "resource/tracks/" + id + ".mp3";
    }

    public static Rate GetRate(float achievement)
    {
        var rate = new Rate();

        if (achievement >= 100.5)
            rate = Rate.Sssp;
        else if (achievement is < 100.5f and >= 100)
            rate = Rate.Sss;
        else if (achievement is < 100 and >= 99.5f)
            rate = Rate.Ssp;
        else if (achievement is < 99.5f and >= 99)
            rate = Rate.Ss;
        else if (achievement is < 99 and >= 98)
            rate = Rate.Sp;
        else if (achievement is < 98 and >= 97)
            rate = Rate.S;
        else if (achievement is < 97 and >= 94)
            rate = Rate.Aaa;
        else if (achievement is < 94 and >= 90)
            rate = Rate.Aa;
        else if (achievement is < 90 and >= 80)
            rate = Rate.A;
        else if (achievement is < 80 and >= 75)
            rate = Rate.Bbb;
        else if (achievement is < 75 and >= 70)
            rate = Rate.Bb;
        else if (achievement is < 70 and >= 60)
            rate = Rate.B;
        else if (achievement is < 60 and >= 50)
            rate = Rate.C;
        else if (50 > achievement)
            rate = Rate.D;

        return rate;
    }

    public class AliasItemDto
    {
        [JsonProperty("aliases")] public List<string> Aliases;
        [JsonProperty("song_id")] public int Id;
    }
}

public class ScoresDto
{
    [JsonProperty("verlist")] public ScoreDto[] ScoreDtos;

    public class ScoreDto
    {
        [JsonProperty("achievements")] public float Achievements;

        [JsonProperty("fc")] public string Fc;

        [JsonProperty("fs")] public string Fs;

        [JsonProperty("Id")] public int Id;

        [JsonProperty("level_index")] public int LevelIndex;
    }
}

public class SongDto
{
    [JsonProperty("basic_info")] public BasicInfoDto BasicInfo;

    [JsonProperty("charts")] public ChartDto[] Charts;

    public float[] FitRatings;

    [JsonProperty("id")] public int Id;

    [JsonProperty("level")] public string[] Levels;

    [JsonProperty("ds")] public float[] Ratings;

    [JsonProperty("title")] public string Title;

    [JsonProperty("type")] public string Type;

    public override bool Equals(object obj)
    {
        return obj is SongDto other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public class ChartDto
    {
        [JsonProperty("charter")] public string Charter;

        public int MaxDxScore;

        [JsonProperty("notes")] public int[] Notes;
    }

    public class BasicInfoDto
    {
        [JsonProperty("artist")] public string Artist;

        [JsonProperty("bpm")] public float Bpm;

        [JsonProperty("from")] public string Version;
    }
}

public class ChartStatisticsDto
{
    [JsonProperty("charts")] public Dictionary<string, ChartStatisticDto[]> Charts;

    public class ChartStatisticDto
    {
        [JsonProperty("fit_diff")] public float FitRating;
    }
}

public class AliasDto
{
    [JsonProperty("aliases")] public MaiCommandBase.AliasItemDto[] Content;
}

public class MaiCommand : MaiCommandBase
{
    public readonly Dictionary<string, int> LevelDictionary = new()
    {
        { "1", 0 }, { "2", 1 }, { "3", 2 },
        { "4", 3 }, { "5", 4 }, { "6", 5 },
        { "6+", 6 }, { "7", 7 }, { "7+", 8 },
        { "8", 9 }, { "8+", 10 }, { "9", 11 },
        { "9+", 12 }, { "10", 13 }, { "10+", 14 },
        { "11", 15 }, { "11+", 16 }, { "12", 17 },
        { "12+", 18 }, { "13", 19 }, { "13+", 20 },
        { "14", 21 }, { "14+", 22 }, { "15", 23 },
        { "1?", 24 }, { "2?", 25 }, { "3?", 26 },
        { "4?", 27 }, { "5?", 28 }, { "6?", 29 },
        { "6+?", 30 }, { "7?", 31 }, { "7+?", 32 },
        { "8?", 33 }, { "8+?", 34 }, { "9?", 35 },
        { "9+?", 36 }, { "10?", 37 }, { "10+?", 38 },
        { "11?", 39 }, { "11+?", 40 }, { "12?", 41 },
        { "12+?", 42 }, { "13?", 43 }, { "13+?", 44 },
        { "14?", 45 }, { "14+?", 46 }, { "15?", 47 }
    };

    public MaiCommand()
    {
        MaiCommandInstance = this;
        CommandHead = "mai";
        SubCommands =
        [
            new RandomCommand(),
            new InfoCommand(),
            new AliasCommand(),
            new BestCommand(),
            new LettersCommand(),
            new GuessCommand(),
            new PlateCommand(),
            new UpdateCommand(),
            new BindCommand(),
            new SearchCommand(),
            new CutoffPointCommand(),
            new PlayerInfoCommand()
        ];
    }

    public SongMetaData[] GetSongsUsingDifficultyString(string difficultyString)
    {
        var isRating = float.TryParse(difficultyString, out var rating);

        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        var charts = (isRating
            ? db.ChartMetaDataSet.Where(x => x.Rating.Equals(rating))
            : db.ChartMetaDataSet.Where(x => x.LevelName == difficultyString)).ToArray();

        var songs = charts.Select(x => GetSongById(x.SongId)).ToHashSet().ToArray();

        return songs;
    }

    private SongAlias GetAliasByAliasString(string alias)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;
        var dataSet = db.SongAliasDataSet.Include(x => x.Aliases).ToList();

        var findResult = dataSet.FirstOrDefault(x => x.Aliases.Exists(y => y.Alias.ToLower() == alias.ToLower()));

        if (findResult == null)
            return null;

        return GetAliasById(findResult.SimplifiedSongId);
    }

    public SongAlias GetAliasById(int id)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;
        return db.SongAliasDataSet.Include(x => x.Aliases).FirstOrDefault(x => (id - x.SimplifiedSongId) % 10000 == 0);
    }

    public SongDto ToSongDto(SongMetaData song)
    {
        var chart = song.Charts;

        return new SongDto
        {
            BasicInfo = new SongDto.BasicInfoDto
            {
                Artist = song.Artist,
                Bpm = song.Bpm,
                Version = song.Version
            },
            Charts = chart.Select(x => new SongDto.ChartDto
            {
                Charter = x.CharterName,
                MaxDxScore = x.MaxDxScore,
                Notes = [x.TapCount, x.HoldCount, x.SlideCount, x.TouchCount, x.BreakCount]
            }).ToArray(),
            FitRatings = chart.Select(x => x.FitRating).ToArray(),
            Ratings = chart.Select(x => x.Rating).ToArray(),
            Id = song.SongId,
            Levels = chart.Select(x => x.LevelName).ToArray(),
            Title = song.Title,
            Type = GetSongType(song.SongId).ToString()
        };
    }

    private SongMetaData[] GetSongsBySimplifiedId(int simplifiedId)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        return db.SongMetaDataSet.Include(x => x.Charts).Where(x => x.SongId - simplifiedId % 10000 == 0).ToArray();
    }

    public SongMetaData GetSongById(int id)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        var songDbItem =
            db.SongMetaDataSet.Include(x => x.Charts).FirstOrDefault(x => x.SongId == id);

        return songDbItem;
    }

    public bool TryGetSongs(string inputString, out SongMetaData[] songs,
        CommandBehaviorInformationDataObject commandBehaviorInformationDataObject = null,
        CqMessagePostContext source = null,
        bool noUtage = false) // Return false when no song is matched, and send error message to the group.
    {
        inputString = inputString.ToLower();
        var aliasItem = GetAliasByAliasString(inputString);

        if (aliasItem != null && aliasItem.Aliases.Count != 0)
        {
            var songsSet = new HashSet<SongMetaData>();
            foreach (var alias in aliasItem.Aliases)
            {
                var songsByAlias = GetSongsBySimplifiedId(aliasItem.SimplifiedSongId);

                if (songsByAlias.Length == 0)
                    continue;

                foreach (var songMetaData in songsByAlias) songsSet.Add(songMetaData);
            }

            var utageSongs = songsSet.Where(x => x.SongId > 100000).Select(x => x).ToArray();

            if (utageSongs.Length != 0)
            {
                var standardSongs = songsSet
                    .Where(x => utageSongs[0].Title.Contains(x.Title) && utageSongs[0].Title != x.Title)
                    .Select(x => x)
                    .ToArray();

                if (standardSongs.Length != 0 && noUtage)
                {
                    songs = standardSongs;
                    return true;
                }
            }

            if (songsSet.Count != 0)
            {
                songs = songsSet.ToArray();
                return true;
            }
        }

        if (IdRegex.IsMatch(inputString.ToLower()))
            try
            {
                var id = int.Parse(IdHeadRegex.Replace(inputString.ToLower(), string.Empty, 1));
                var song = GetSongById(id);
                if (song != null)
                {
                    songs = [song];

                    return true;
                }

                songs = null;

                if (source != null && commandBehaviorInformationDataObject != null)
                    SendMessage(source,
                    [
                        new CqReplyMsg(source.MessageId),
                        GetMultiSearchResultInformationString(inputString, commandBehaviorInformationDataObject)
                    ]);
                return false;
            }
            catch
            {
                songs = null;

                if (source != null && commandBehaviorInformationDataObject != null)
                    SendMessage(source,
                    [
                        new CqReplyMsg(source.MessageId),
                        GetMultiSearchResultInformationString(inputString, commandBehaviorInformationDataObject)
                    ]);
                return false;
            }

        var songsByTitle = GetSongsByTitle(inputString);
        if (songsByTitle.Length != 0)
        {
            songs = songsByTitle;
            return true;
        }

        var searchedSongsList = SearchCommand.SearchCommandInstance.Search(inputString).AllSongs;

        var searchedSongsHashSet = new HashSet<SongMetaData>();
        foreach (var searchedSong in searchedSongsList) searchedSongsHashSet.Add(searchedSong);

        var dereplicatedSearchedSongsList = searchedSongsHashSet.ToList();

        var utageRegex = new Regex(@"\[[\u4e00-\u9fa5]|[\u3040-\u30FF\u31F0-\u31FF\uFF00-\uFFEF]\]");

        if (dereplicatedSearchedSongsList.Count > 1)
        {
            var utageSongsList = new List<SongMetaData>();
            foreach (var searchedSong in dereplicatedSearchedSongsList)
                if (utageRegex.IsMatch(searchedSong.Title))
                    utageSongsList.Add(searchedSong);

            foreach (var utageSong in utageSongsList) dereplicatedSearchedSongsList.Remove(utageSong);
        }

        if (dereplicatedSearchedSongsList.Count == 1)
        {
            songs = dereplicatedSearchedSongsList.ToArray();
            return true;
        }

        songs = null;

        if (source != null && commandBehaviorInformationDataObject != null)
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                GetMultiSearchResultInformationString(inputString, commandBehaviorInformationDataObject)
            ]);
        return false;
    }

    private SongMetaData[] GetSongsByTitle(string title)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        return db.SongMetaDataSet.Include(x => x.Charts).Where(x => x.Title == title).ToArray();
    }

    private void Reload(object sender, EventArgs e)
    {
        Start();
    }

    private void Start()
    {
        //UpdateSongDatabase();

        foreach (var command in SubCommands) command.Initialize();
    }

    public override void Initialize()
    {
        Program.DateChanged += Reload;

        Start();
    }

    private void UpdateSongDatabase()
    {
        try
        {
            var chartStatsResponse = ApiOperator.Instance.Get(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/chart_stats", 60);

            if (chartStatsResponse.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"Unexpected status code: {chartStatsResponse.StatusCode}", null,
                    chartStatsResponse.StatusCode);

            var chartStatisticsResult =
                JsonConvert.DeserializeObject<ChartStatisticsDto>(chartStatsResponse.Result);

            var songMetaResult = ApiOperator.Instance.Get(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/music_data", 60);

            if (songMetaResult.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"Unexpected status code: {songMetaResult.StatusCode}", null,
                    songMetaResult.StatusCode);

            var songs = (SongDto[])JsonConvert.DeserializeObject(songMetaResult.Result, typeof(SongDto[]));

            if (songs != null)
                foreach (var song in songs)
                {
                    foreach (var chart in song.Charts)
                    foreach (var notes in chart.Notes)
                        chart.MaxDxScore += notes * 3;

                    chartStatisticsResult.Charts.TryGetValue(song.Id.ToString(), out var chartStatistics);
                    List<float> fitRatings = new();
                    if (chartStatistics != null)
                        foreach (var chartStatistic in chartStatistics)
                            fitRatings.Add(chartStatistic.FitRating);
                    else
                        foreach (var rating in song.Ratings)
                            fitRatings.Add(rating);

                    song.FitRatings = fitRatings.ToArray();

                    if (song.Id < 100000)
                        continue;
                    for (var i = 0; i < song.Levels.Length; i++)
                        if (!song.Levels[i].Contains('?'))
                            song.Levels[i] += '?';
                }

            var aliasResponseContent = ApiOperator.Instance.Get(BotConfiguration.Instance.AliasUrl, 60);

            if (aliasResponseContent.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"Unexpected status code: {aliasResponseContent.StatusCode}", null,
                    aliasResponseContent.StatusCode);

            var aliasDto =
                JsonConvert.DeserializeObject<AliasDto>(
                    aliasResponseContent.Result);

            using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

            DatabaseHandler.Instance.SongMetaDatabaseOperator.InsertSongs(songs, aliasDto.Content, db);
        }
        catch (Exception e)
        {
            Program.Logger.LogWarning("Song database update failed");
        }
    }
}