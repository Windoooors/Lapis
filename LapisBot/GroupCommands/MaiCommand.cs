using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.GroupCommands.MaiCommands;
using LapisBot.GroupCommands.MaiCommands.AliasCommands;
using LapisBot.Operations.ApiOperation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LapisBot.GroupCommands;

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

    public string GetMultiAliasesMatchedInformationString(SongDto[] songs, string command, string functionString)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("该别称有多首歌曲匹配：");

        foreach (var song in songs) stringBuilder.AppendLine($"ID {song.Id} - {song.Title} [{song.Type}]");

        stringBuilder.Append(
            $"*发送 \"lps mai {command} ID {songs[0].Id}\" 指令即可查询歌曲 {songs[0].Title} [{songs[0].Type}] 的{functionString}");

        return stringBuilder.ToString();
    }

    public string GetMultiAliasesMatchedInformationString(SongDto[] songs, string command, string commandParameter,
        string functionString)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("该别称有多首歌曲匹配：");

        foreach (var song in songs) stringBuilder.AppendLine($"ID {song.Id} - {song.Title} [{song.Type}]");

        stringBuilder.Append(
            $"*发送 \"lps mai {command} ID {songs[0].Id} {commandParameter}\" 指令即可为歌曲 {songs[0].Title} [{songs[0].Type}] {functionString}");

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

    public class Alias
    {
        [JsonProperty("Alias")] public List<string> Aliases;
        [JsonProperty("SongID")] public int Id;
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
    [JsonProperty("content")] public MaiCommandBase.Alias[] Content;
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

    private List<Alias> _songAliases = [];
    public AddCommand AddCommand;
    public SongDto[] Songs;

    public MaiCommand()
    {
        MaiCommandInstance = this;
        CommandHead = new Regex("^mai");
        SubCommands =
        [
            new RandomCommand(),
            new InfoCommand(),
            new AliasCommand(),
            new BestCommand(),
            new LettersCommand(),
            new GuessCommand(),
            new PlateCommand(),
            new BindCommand(),
            new UpdateCommand(),
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
        foreach (var valueAlias in _songAliases)
        foreach (var valueAliasString in valueAlias.Aliases)
            if (valueAliasString.ToLower().Equals(alias.ToLower()))
                aliases.Add(valueAlias);

        var localAlias = LocalAlias.Instance;
        foreach (var e1 in localAlias.GetIds())
        {
            var a = LocalAlias.Instance.Get(e1);
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
        var valueAlias = new Alias { Id = id, Aliases = new List<string>() };
        foreach (var alias in _songAliases)
            if (alias.Id == id)
            {
                valueAlias = alias;
                break;
            }

        var tempAlias = new Alias { Id = id, Aliases = new List<string>() };
        foreach (var aliasString in valueAlias.Aliases) tempAlias.Aliases.Add(aliasString);

        var local = LocalAlias.Instance.Get(id);
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
                if (GetSongIndexById(alias.Id) == -1)
                    continue;
                if (!songsList.Contains(Songs[GetSongIndexById(alias.Id)]))
                    songsList.Add(Songs[GetSongIndexById(alias.Id)]);
            }

            return songsList.ToArray();
        }

        var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
        var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
        if (idRegex.IsMatch(inputString.ToLower()))
            try
            {
                var id = int.Parse(idHeadRegex.Replace(inputString.ToLower(), string.Empty));
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

        return null;
    }

    public string GetSongIndicatorString(string inputString)
    {
        var tempAlias = new List<string>();
        var tempIds = new List<string>();
        var tempTitles = new List<string>();

        foreach (var alias in _songAliases)
        foreach (var aliasString in alias.Aliases)
            if (inputString.ToLower().StartsWith(aliasString.ToLower()))
                tempAlias.Add(aliasString.ToLower());

        var localAlias = LocalAlias.Instance;
        foreach (var e1 in localAlias.GetIds())
        {
            var a = LocalAlias.Instance.Get(e1);
            foreach (var aliasString in a)
                if (inputString.ToLower().StartsWith(aliasString.ToLower()))
                    tempAlias.Add(aliasString.ToLower());
        }

        foreach (var song in Songs)
        {
            if (inputString.ToLower().StartsWith(song.Title.ToLower()))
                tempTitles.Add(song.Title.ToLower());

            if (inputString.ToLower().StartsWith(("id" + song.Id).ToLower()))
                tempIds.Add(("id" + song.Id).ToLower());

            if (inputString.ToLower().StartsWith(("id " + song.Id).ToLower()))
                tempIds.Add(("id " + song.Id).ToLower());
        }

        var sortedTempAlias = tempAlias.OrderByDescending(t => t.Length);
        var sortedTempIds = tempIds.OrderByDescending(t => t.Length);
        var sortedTempTitles = tempTitles.OrderByDescending(t => t.Length);

        foreach (var alias in sortedTempAlias)
            if (inputString.ToLower().StartsWith(alias + " ") || inputString.ToLower() == alias)
                return alias;
        foreach (var id in sortedTempIds)
            if (inputString.ToLower().StartsWith(id + " ") || inputString.ToLower() == id)
                return id;
        foreach (var title in sortedTempTitles)
            if (inputString.ToLower().StartsWith(title + " ") || inputString.ToLower() == title)
                return title;

        return null;
    }

    public SongDto[] GetSongsUsingStartsWith(string inputString)
    {
        inputString = inputString.ToLower();
        var songIndicator = GetSongIndicatorString(inputString);

        if (songIndicator == null)
            return null;

        var aliases = GetAliasByAliasString(songIndicator);

        if (aliases.Length != 0)
        {
            var songsList = new List<SongDto>();
            foreach (var alias in aliases)
            {
                if (GetSongIndexById(alias.Id) == -1)
                    continue;
                if (!songsList.Contains(Songs[GetSongIndexById(alias.Id)]))
                    songsList.Add(Songs[GetSongIndexById(alias.Id)]);
            }

            return songsList.Count == 0 ? null : songsList.ToArray();
        }

        var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
        var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
        if (idRegex.IsMatch(songIndicator))
            try
            {
                var id = int.Parse(idHeadRegex.Replace(songIndicator, string.Empty));
                var index = GetSongIndexById(id);
                if (index != -1)
                    return [Songs[index]];
                return null;
            }
            catch
            {
                return null;
            }

        var songIndex = GetSongIndexByTitle(songIndicator);
        if (songIndex != -1) return [Songs[songIndex]];

        return null;
    }

    private void Reload(object sender, EventArgs e)
    {
        Start();
    }

    private void Start()
    {
        _songAliases.Clear();

        _chartStatistics =
            JsonConvert.DeserializeObject<ChartStatisticsDto>(
                ApiOperator.Instance.Get(BotConfiguration.Instance.DivingFishUrl,
                    "api/maimaidxprober/chart_stats"));

        var aliasDto =
            JsonConvert.DeserializeObject<AliasDto>(ApiOperator.Instance.Get(BotConfiguration.Instance.AliasUrl));

        _songAliases = aliasDto.Content.ToList();

        foreach (var alias in _songAliases)
            if (alias.Id == 11422)
            {
                var invalidAliasStrings = new List<string>();

                foreach (var aliasString in alias.Aliases)
                    if (aliasString == "\u200e\u200e" || aliasString == "　" || aliasString == "\u3000" ||
                        aliasString == string.Empty || aliasString == "\n")
                        invalidAliasStrings.Add(aliasString);

                foreach (var invalidAlias in invalidAliasStrings) alias.Aliases.Remove(invalidAlias);
            }

        Songs = (SongDto[])JsonConvert.DeserializeObject(
            ApiOperator.Instance.Get(BotConfiguration.Instance.DivingFishUrl, "api/maimaidxprober/music_data"),
            typeof(SongDto[]));

        if (Songs == null)
        {
            Program.Logger.LogInformation("maimai related commands initializing failed, retrying...");
            Start();
            return;
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

        //除去 LocalAlias 中已存在的别名
        foreach (var alias in _songAliases)
        {
            var localAlias = LocalAlias.Instance;
            foreach (var e1 in localAlias.GetIds())
            {
                if (e1 != alias.Id)
                    continue;
                var a = LocalAlias.Instance.Get(e1);
                foreach (var aliasString in a)
                    if (alias.Aliases.Contains(aliasString))
                        alias.Aliases.Remove(aliasString);
            }
        }
    }

    public override void Initialize()
    {
        Program.DateChanged += Reload;

        Start();
    }

    public override void Unload()
    {
        foreach (var command in SubCommands) command.Unload();
    }
}