using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.MaiCommands;
using Lapis.Commands.GroupCommands.MaiCommands.AliasCommands;
using Lapis.Commands.UniversalCommands;
using Lapis.Miscellaneous;
using Lapis.Operations.ApiOperation;
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

    protected static MaiCommand MaiCommandInstance;
    protected readonly Regex IdHeadRegex = new(@"^id\s|^id|^ID\s|^ID");
    protected readonly Regex IdRegex = new(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");

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

    protected void ObjectUserUnboundErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("未找到该用户")
        ]);
    }

    public string GetMultiAliasesMatchedInformationString(SongDto[] songs, string command, string functionString)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("该别称有多首歌曲匹配：");

        foreach (var song in songs) stringBuilder.AppendLine($"ID {song.Id} - {song.Title} [{song.Type}]");

        stringBuilder.Append(
            $"*发送 \"lps mai {command} ID{songs[0].Id}\" 指令即可查询歌曲 {songs[0].Title} [{songs[0].Type}] 的{functionString}");

        return stringBuilder.ToString();
    }

    public string GetMultiAliasesMatchedInformationString(SongDto[] songs, string command, string commandParameter,
        string functionString)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("该别称有多首歌曲匹配：");

        foreach (var song in songs) stringBuilder.AppendLine($"ID {song.Id} - {song.Title} [{song.Type}]");

        stringBuilder.Append(
            $"*发送 \"lps mai {command} ID{songs[0].Id} {commandParameter}\" 指令即可为歌曲 {songs[0].Title} [{songs[0].Type}] {functionString}");

        return stringBuilder.ToString();
    }

    public string GetMultiSearchResultInformationString(string keyword, string command, string functionString)
    {
        var searchResult = SearchCommand.SearchCommandInstance.Search(keyword);

        var stringBuilder = new StringBuilder();

        if (searchResult.AllSongs.Count >= 30)
        {
            stringBuilder.AppendLine("搜索结果过多，请提供更多关键词");
        }
        else
        {
            stringBuilder = SearchCommand.SearchCommandInstance.GetMultiSearchResults(searchResult);

            if (searchResult.AllSongs.Count != 0)
            {
                var exampleSong = searchResult.AllSongs[0];
                stringBuilder.Append(
                    $"*发送 \"lps mai {command} ID{exampleSong.Id}\" 指令即可查询歌曲 {exampleSong.Title} [{exampleSong.Type}] 的{functionString}");
            }
            else
            {
                stringBuilder.Clear();
                stringBuilder = new StringBuilder("未找到该歌曲");
            }
        }

        return stringBuilder.ToString();
    }

    public string GetMultiSearchResultInformationString(string keyword, string command, string commandParameter,
        string functionString)
    {
        var searchResult = SearchCommand.SearchCommandInstance.Search(keyword);

        var stringBuilder = new StringBuilder();

        if (searchResult.AllSongs.Count >= 30)
        {
            stringBuilder.AppendLine("搜索结果过多，请提供更多关键词");
        }
        else
        {
            stringBuilder = SearchCommand.SearchCommandInstance.GetMultiSearchResults(searchResult);

            if (searchResult.AllSongs.Count != 0)
            {
                var exampleSong = searchResult.AllSongs[0];
                stringBuilder.Append(
                    $"*发送 \"lps mai {command} ID{exampleSong.Id} {commandParameter}\" 指令即可为歌曲 {exampleSong.Title} [{exampleSong.Type}] {functionString}");
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

    protected static Rate GetRate(float achievement)
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

public class ExtraSongDto
{
    [JsonProperty("Artist")] public string Artist;

    [JsonProperty("MapInformations")] public MapInfomationDto[] Charts;
    [JsonProperty("Id")] public int Id;

    [JsonProperty("Title")] public string Title;

    [JsonProperty("Cabinet")] public string Type;

    public class MapInfomationDto
    {
        [JsonProperty("Author")] public string Author;
        [JsonProperty("Level")] public string Level;
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

    private ChartStatisticsDto _chartStatistics;

    public List<Alias> SongAliases = [];
    public SongDto[] Songs;

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
            new SearchCommand()
        ];
    }

    public SongDto[] GetSongsUsingDifficultyString(string difficultyString)
    {
        var songs = new List<SongDto>();

        foreach (var song in Songs)
            if (song.Levels.Contains(difficultyString))
                songs.Add(song);

        return songs.ToArray();
    }

    private Alias[] GetAliasByAliasString(string alias)
    {
        alias = alias.ToLower();
        var aliases = new List<Alias>();
        foreach (var valueAlias in SongAliases)
        foreach (var valueAliasString in valueAlias.Aliases)
            if (valueAliasString.ToLower().Equals(alias.ToLower()))
                aliases.Add(valueAlias);

        var localAlias = LocalAliasManager.Instance;
        foreach (var e1 in localAlias.GetIds())
        {
            var a = LocalAliasManager.Instance.Get(e1);
            if (a == null)
                continue;
            foreach (var aliasString in a)
                if (aliasString.ToLower() == alias)
                {
                    var temp = new Alias { Aliases = [] };
                    temp.Id = e1;

                    foreach (var e2 in a) temp.Aliases.Add(e2);

                    aliases.Add(temp);
                }
        }

        return aliases.ToArray();
    }

    public Alias GetAliasById(int id)
    {
        var valueAlias = new Alias { Id = id, Aliases = [] };
        foreach (var alias in SongAliases)
            if (alias.Id == id)
            {
                valueAlias = alias;
                break;
            }

        var tempAlias = new Alias { Id = id, Aliases = [] };
        foreach (var aliasString in valueAlias.Aliases) tempAlias.Aliases.Add(aliasString);

        var local = LocalAliasManager.Instance.Get(id);
        if (local != null)
            foreach (var e in local)
                if (!valueAlias.Aliases.Contains(e))
                    tempAlias.Aliases.Add(e);

        return tempAlias;
    }

    private int GetSongIndexById(int id)
    {
        for (var i = 0; i < Songs.Length; i++)
            if (Songs[i].Id == id)
                return i;

        return -1;
    }

    private int GetSongIndexByTitle(string title)
    {
        for (var i = 0; i < Songs.Length; i++)
            if (title.ToLower() == Songs[i].Title.ToLower())
                return i;

        return -1;
    }

    public SongDto GetSong(int id)
    {
        return Songs[GetSongIndexById(id)];
    }

    public SongDto[] GetSongs(string inputString)
    {
        inputString = inputString.ToLower();
        var aliases = GetAliasByAliasString(inputString);

        if (aliases.Length != 0)
        {
            var songsList = new List<SongDto>();
            foreach (var alias in aliases)
            {
                if (GetSongIndexById((int)alias.Id) == -1)
                    continue;
                if (!songsList.Contains(Songs[GetSongIndexById((int)alias.Id)]))
                    songsList.Add(Songs[GetSongIndexById((int)alias.Id)]);
            }

            if (songsList.Count != 0)
                return songsList.ToArray();
        }

        if (IdRegex.IsMatch(inputString.ToLower()))
            try
            {
                var id = int.Parse(IdHeadRegex.Replace(inputString.ToLower(), string.Empty, 1));
                var index = GetSongIndexById(id);
                if (index != -1)
                    return [Songs[index]];
                return null;
            }
            catch
            {
                return null;
            }

        var songIndex = GetSongIndexByTitle(inputString);
        if (songIndex != -1) return [Songs[songIndex]];

        var searchedSongsList = SearchCommand.SearchCommandInstance.Search(inputString).AllSongs;

        var searchedSongsHashSet = new HashSet<SongDto>();
        foreach (var searchedSong in searchedSongsList) searchedSongsHashSet.Add(searchedSong);

        var dereplicatedSearchedSongsList = searchedSongsHashSet.ToList();

        var utageRegex = new Regex(@"\[[\u4e00-\u9fa5]|[\u3040-\u30FF\u31F0-\u31FF\uFF00-\uFFEF]\]");

        if (dereplicatedSearchedSongsList.Count > 1)
        {
            var utageSongsList = new List<SongDto>();
            foreach (var searchedSong in dereplicatedSearchedSongsList)
                if (utageRegex.IsMatch(searchedSong.Title))
                    utageSongsList.Add(searchedSong);

            foreach (var utageSong in utageSongsList) dereplicatedSearchedSongsList.Remove(utageSong);
        }

        if (dereplicatedSearchedSongsList.Count == 1)
            return dereplicatedSearchedSongsList.ToArray();

        return null;
    }

    private void Reload(object sender, EventArgs e)
    {
        Start();
    }

    private void Start()
    {
        try
        {
            var responseContent = ApiOperator.Instance.Get(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/chart_stats", 60);
            _chartStatistics =
                JsonConvert.DeserializeObject<ChartStatisticsDto>(responseContent);
        }
        catch (Exception ex)
        {
            Program.Logger.LogWarning(
                ex.InnerException is TaskCanceledException or HttpRequestException
                    ? "Error occurred when trying to get chart statistics from DivingFish, check if DivingFish URL is correct."
                    : "Unknown error occurred when trying to get chart statistics from DivingFish."
            );
            _chartStatistics = new ChartStatisticsDto();
        }

        try
        {
            Songs = (SongDto[])JsonConvert.DeserializeObject(
                ApiOperator.Instance.Get(BotConfiguration.Instance.DivingFishUrl, "api/maimaidxprober/music_data", 60),
                typeof(SongDto[]));
        }
        catch (Exception ex)
        {
            Program.Logger.LogWarning(
                ex.InnerException is TaskCanceledException or HttpRequestException
                    ? "Error occurred when trying to get song metadata from DivingFish, check if DivingFish URL is correct."
                    : "Unknown error occurred when trying to get song metadata from DivingFish."
            );

            Program.Logger.LogInformation("maimai related commands initializing failed, retrying...");
            Start();
            return;
        }

        if (Songs == null)
        {
            Program.Logger.LogInformation("maimai related commands initializing failed, retrying...");
            Start();
            return;
        }

        AliasDto aliasDto;

        try
        {
            aliasDto =
                JsonConvert.DeserializeObject<AliasDto>(
                    ApiOperator.Instance.Get(BotConfiguration.Instance.AliasUrl, 60));
        }
        catch (Exception ex)
        {
            Program.Logger.LogWarning(
                ex.InnerException is TaskCanceledException or HttpRequestException
                    ? "Error occurred when trying to get alias data from Lxns, check if alias URL is correct."
                    : "Unknown error occurred when trying to get alias data from Lxns."
            );

            Program.Logger.LogInformation("maimai related commands initializing failed, retrying...");
            Start();
            return;
        }

        var songAliasItemDtos = aliasDto.Content.ToList();

        foreach (var aliasItemDto in songAliasItemDtos)
        {
            if (GetSongIndexById(aliasItemDto.Id) == -1)
                aliasItemDto.Id += 10000;

            if (aliasItemDto.Id == 11422)
            {
                var invalidAliasStrings = new List<string>();

                foreach (var aliasString in aliasItemDto.Aliases)
                    if (aliasString == "\u200e\u200e" || aliasString == "　" || aliasString == "\u3000" ||
                        aliasString == string.Empty || aliasString == "\n")
                        invalidAliasStrings.Add(aliasString);

                foreach (var invalidAlias in invalidAliasStrings) aliasItemDto.Aliases.Remove(invalidAlias);
            }

            SongAliases.Add(new Alias
            {
                Id = aliasItemDto.Id,
                Aliases = aliasItemDto.Aliases.ToHashSet()
            });
        }

        foreach (var song in Songs)
        {
            foreach (var chart in song.Charts)
            foreach (var notes in chart.Notes)
                chart.MaxDxScore += notes * 3;

            _chartStatistics.Charts.TryGetValue(song.Id.ToString(), out var chartStatistics);
            List<float> fitRatings = new();
            if (chartStatistics != null)
                foreach (var chartStatistic in chartStatistics)
                    fitRatings.Add(chartStatistic.FitRating);
            else
                foreach (var rating in song.Ratings)
                    fitRatings.Add(rating);

            song.FitRatings = fitRatings.ToArray();
        }

        foreach (var command in SubCommands) command.Initialize();
    }

    public override void Initialize()
    {
        Program.DateChanged += Reload;

        Start();
    }
}