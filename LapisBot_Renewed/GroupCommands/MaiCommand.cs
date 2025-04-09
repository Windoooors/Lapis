using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using LapisBot_Renewed.GroupCommands.MaiCommands;
using System.IO;
using System.Linq;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.GroupCommands.MaiCommands.AliasCommands;
using LapisBot_Renewed.Operations.ApiOperation;

namespace LapisBot_Renewed.GroupCommands
{

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
        [JsonProperty("Id")] public int Id;

        [JsonProperty("Title")] public string Title;

        [JsonProperty("Cabinet")] public string Type;

        [JsonProperty("Artist")] public string Artist;

        [JsonProperty("MapInformations")] public MapInfomationDto[] Charts;

        public class MapInfomationDto
        {
            [JsonProperty("Level")] public string Level;
            [JsonProperty("Author")] public string Author;
        }
    }

    public class SongDto
    {
        [JsonProperty("id")] public int Id;

        [JsonProperty("title")] public string Title;

        [JsonProperty("type")] public string Type;

        [JsonProperty("level")] public string[] Levels;

        [JsonProperty("ds")] public float[] Ratings;

        [JsonProperty("charts")] public ChartDto[] Charts;

        [JsonProperty("basic_info")] public BasicInfoDto BasicInfo;

        public float[] FitRatings;

        public class ChartDto
        {
            [JsonProperty("charter")] public string Charter;

            [JsonProperty("notes")] public int[] Notes;

            public int MaxDxScore;
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
        [JsonProperty("content")] public MaiCommand.Alias[] Content;
    }

    public class MaiCommand : GroupCommand
    {
        public static MaiCommand Instance;

        public override Task Unload()
        {
            foreach (var subMaiCommand in SubCommands)
                subMaiCommand.Unload();
            return Task.CompletedTask;
        }

        private Alias[] GetAliasByAliasString(string alias)
        {
            alias = alias.ToLower();
            var aliases = new List<Alias>();
            foreach (var valueAlias in _songAliases)
            {
                foreach (var valueAliasString in valueAlias.Aliases)
                {
                    if (valueAliasString.ToLower().Equals(alias.ToLower()))
                        aliases.Add(valueAlias);
                }
            }

            var localAlias = LocalAlias.Instance;
            foreach (var e1 in localAlias.GetIds())
            {
                var a = LocalAlias.Instance.Get(e1);
                foreach (var aliasString in a)
                {
                    if (aliasString.ToLower() == alias)
                    {
                        var temp = new Alias() { Aliases = new List<string>() };
                        temp.Id = e1;

                        foreach (var e2 in a)
                        {
                            temp.Aliases.Add(e2);
                        }

                        aliases.Add(temp);
                    }
                }
            }

            return aliases.ToArray();
        }

        public Alias GetAliasById(int id)
        {
            var valueAlias = new Alias() { Id = id, Aliases = new List<string>() };
            foreach (Alias alias in _songAliases)
            {
                if (alias.Id == id)
                {
                    valueAlias = alias;
                    break;
                }
            }

            var tempAlias = new Alias() { Id = id, Aliases = new List<string>() };
            foreach (var aliasString in valueAlias.Aliases)
            {
                tempAlias.Aliases.Add(aliasString);
            }

            var local = LocalAlias.Instance.Get(id);
            if (local != null)
            {
                foreach (var e in local)
                {
                    if (!valueAlias.Aliases.Contains(e))
                    {
                        tempAlias.Aliases.Add(e);
                    }
                }
            }

            return tempAlias;
        }

        private int GetSongIndexById(int id)
        {
            for (int i = 0; i < Songs.Length; i++)
            {
                if (Songs[i].Id == id)
                    return i;
            }

            return -1;
        }

        private int GetSongIndexByTitle(string title)
        {
            for (int i = 0; i < Songs.Length; i++)
            {
                if (title.ToLower() == Songs[i].Title.ToLower())
                    return i;
            }

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
            {
                try
                {
                    var id = int.Parse(idHeadRegex.Replace(inputString.ToLower(), string.Empty));
                    int index = GetSongIndexById(id);
                    if (index != -1)
                        return new[] { Songs[index] };
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }

            var songIndex = GetSongIndexByTitle(inputString);
            if (songIndex != -1)
            {
                return new[] { Songs[songIndex] };
            }

            return null;
        }

        public string GetSongIndicatorString(string inputString)
        {
            var tempAlias = new List<string>();
            var tempIds = new List<string>();
            var tempTitles = new List<string>();

            foreach (var alias in _songAliases)
            {
                foreach (var aliasString in alias.Aliases)
                {
                    if (inputString.ToLower().StartsWith(aliasString.ToLower()))
                        tempAlias.Add(aliasString.ToLower());
                }
            }

            var localAlias = LocalAlias.Instance;
            foreach (var e1 in localAlias.GetIds())
            {
                var a = LocalAlias.Instance.Get(e1);
                foreach (var aliasString in a)
                {
                    if (inputString.ToLower().StartsWith(aliasString.ToLower()))
                        tempAlias.Add(aliasString.ToLower());
                }
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

            foreach (string alias in sortedTempAlias)
                if (inputString.ToLower().StartsWith(alias + " ") || inputString.ToLower() == alias)
                    return alias;
            foreach (string id in sortedTempIds)
                if (inputString.ToLower().StartsWith(id + " ") || inputString.ToLower() == id)
                    return id;
            foreach (string title in sortedTempTitles)
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
            {
                try
                {
                    var id = int.Parse(idHeadRegex.Replace(songIndicator, string.Empty));
                    int index = GetSongIndexById(id);
                    if (index != -1)
                        return new[] { Songs[index] };
                    return null;
                }
                catch
                {
                    return null;
                }
            }

            var songIndex = GetSongIndexByTitle(songIndicator);
            if (songIndex != -1)
            {
                return new[] { Songs[songIndex] };
            }

            return null;
        }

        public AddCommand AddCommand;
        public SongDto[] Songs;
        public ExtraSongDto[] ExtraSongs;
        public ChartStatisticsDto ChartStatistics;
        public readonly List<List<SongDto>> Levels = new();
        public readonly List<List<ExtraSongDto>> ExtraLevels = new();
        public readonly Dictionary<string, int> LevelDictionary = new();
        private List<Alias> _songAliases = new();

        public class Alias
        {
            [JsonProperty("Alias")] public List<string> Aliases;
            [JsonProperty("SongID")] public int Id;
        }

        private async void Reload(object sender, EventArgs e)
        {
            await Start();
            await AddCommand.Unload();
        }

        private Task Start()
        {
            Instance = this;

            _songAliases.Clear();
            LevelDictionary.Clear();
            SubCommands.Clear();
            Levels.Clear();
            ExtraLevels.Clear();

            if (!BotSettings.Instance.IsDevelopingMode)
            {
                ChartStatistics =
                    JsonConvert.DeserializeObject<ChartStatisticsDto>(
                        ApiOperator.Instance.Get(BotSettings.Instance.DivingFishUrl, "api/maimaidxprober/chart_stats"));

                var aliasDto =
                    JsonConvert.DeserializeObject<AliasDto>(ApiOperator.Instance.Get(BotSettings.Instance.AliasUrl));

                _songAliases = aliasDto.Content.ToList();
            }

            foreach (Alias alias in _songAliases)
            {
                if (alias.Id == 11422)
                {
                    var invalidAliasStrings = new List<string>();

                    foreach (string aliasString in alias.Aliases)
                        if (aliasString == "\u200e\u200e" || aliasString == "　" || aliasString == "\u3000" ||
                            aliasString == String.Empty || aliasString == "\n")
                            invalidAliasStrings.Add(aliasString);

                    foreach (var invalidAlias in invalidAliasStrings)
                    {
                        alias.Aliases.Remove(invalidAlias);
                    }
                }
            }

            LevelDictionary.Add("1", 0);
            LevelDictionary.Add("2", 1);
            LevelDictionary.Add("3", 2);
            LevelDictionary.Add("4", 3);
            LevelDictionary.Add("5", 4);
            LevelDictionary.Add("6", 5);
            LevelDictionary.Add("6+", 6);
            LevelDictionary.Add("7", 7);
            LevelDictionary.Add("7+", 8);
            LevelDictionary.Add("8", 9);
            LevelDictionary.Add("8+", 10);
            LevelDictionary.Add("9", 11);
            LevelDictionary.Add("9+", 12);
            LevelDictionary.Add("10", 13);
            LevelDictionary.Add("10+", 14);
            LevelDictionary.Add("11", 15);
            LevelDictionary.Add("11+", 16);
            LevelDictionary.Add("12", 17);
            LevelDictionary.Add("12+", 18);
            LevelDictionary.Add("13", 19);
            LevelDictionary.Add("13+", 20);
            LevelDictionary.Add("14", 21);
            LevelDictionary.Add("14+", 22);
            LevelDictionary.Add("15", 23);
            LevelDictionary.Add("1?", 24);
            LevelDictionary.Add("2?", 25);
            LevelDictionary.Add("3?", 26);
            LevelDictionary.Add("4?", 27);
            LevelDictionary.Add("5?", 28);
            LevelDictionary.Add("6?", 29);
            LevelDictionary.Add("6+?", 30);
            LevelDictionary.Add("7?", 31);
            LevelDictionary.Add("7+?", 32);
            LevelDictionary.Add("8?", 33);
            LevelDictionary.Add("8+?", 34);
            LevelDictionary.Add("9?", 35);
            LevelDictionary.Add("9+?", 36);
            LevelDictionary.Add("10?", 37);
            LevelDictionary.Add("10+?", 38);
            LevelDictionary.Add("11?", 39);
            LevelDictionary.Add("11+?", 40);
            LevelDictionary.Add("12?", 41);
            LevelDictionary.Add("12+?", 42);
            LevelDictionary.Add("13?", 43);
            LevelDictionary.Add("13+?", 44);
            LevelDictionary.Add("14?", 45);
            LevelDictionary.Add("14+?", 46);
            LevelDictionary.Add("15?", 47);
            Songs = (SongDto[])JsonConvert.DeserializeObject(
                ApiOperator.Instance.Get(BotSettings.Instance.DivingFishUrl, "api/maimaidxprober/music_data"),
                typeof(SongDto[]));
            for (int i = 0; i < 48; i++)
                Levels.Add(new List<SongDto>());
            for (int i = 0; i < 48; i++)
                ExtraLevels.Add(new List<ExtraSongDto>());

            foreach (SongDto song in Songs)
            {
                foreach (SongDto.ChartDto chart in song.Charts)
                {
                    foreach (int notes in chart.Notes)
                    {
                        chart.MaxDxScore += notes * 3;
                    }
                }

                foreach (string level in song.Levels)
                {
                    int j;
                    LevelDictionary.TryGetValue(level, out j);
                    Levels[j].Add(song);
                }

                ChartStatisticsDto.ChartStatisticDto[] chartStatistics =
                    Array.Empty<ChartStatisticsDto.ChartStatisticDto>();

                ChartStatistics.Charts.TryGetValue(song.Id.ToString(), out chartStatistics);
                List<float> fitRatings = new();
                if (chartStatistics != null)
                    foreach (ChartStatisticsDto.ChartStatisticDto chartStatistic in chartStatistics)
                    {
                        fitRatings.Add(chartStatistic.FitRating);
                    }
                else
                {
                    foreach (float rating in song.Ratings)
                        fitRatings.Add(rating);
                }

                song.FitRatings = fitRatings.ToArray();
            }

            ExtraSongs = (ExtraSongDto[])JsonConvert.DeserializeObject(
                File.ReadAllText(AppContext.BaseDirectory + "/resource/extra_music_data.json"),
                typeof(ExtraSongDto[]));

            foreach (ExtraSongDto song in ExtraSongs)
            {
                foreach (ExtraSongDto.MapInfomationDto mapInfomationDto in song.Charts)
                {
                    int j;
                    LevelDictionary.TryGetValue(mapInfomationDto.Level, out j);
                    ExtraLevels[j].Add(song);
                }
            }

            SubCommands.Add(new RandomCommand());
            SubCommands.Add(new InfoCommand());
            SubCommands.Add(new AliasCommand());
            SubCommands.Add(new BestCommand());
            SubCommands.Add(new PlateCommand());
            SubCommands.Add(new LettersCommand());
            SubCommands.Add(new GuessCommand());
            SubCommands.Add(new AircadeCommand());
            SubCommands.Add(new PlateCommand());
            SubCommands.Add(new BindCommand());
            SubCommands.Add(new UpdateCommand());

            foreach (var subMaiCommand in SubCommands)
            {
                subMaiCommand.Initialize();
                subMaiCommand.ParentCommand = this;
            }

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
                    {
                        if (alias.Aliases.Contains(aliasString))
                            alias.Aliases.Remove(aliasString);
                    }
                }
            }

            Console.WriteLine("MaiCommand Initialized");
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            Program.DateChanged += Reload;
            HeadCommand = new Regex(@"^mai\s");
            DefaultSettings.SettingsName = "maimai DX 相关";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");

            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            Start();
            return Task.CompletedTask;
        }

        public override Task SubParse(string command, CqGroupMessagePostContext source)
        {
            return Task.CompletedTask;
        }

        public int GetSongIndex(string command)
        {
            var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
            var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
            if (idRegex.IsMatch(command))
            {
                var id = int.Parse(idHeadRegex.Replace(command, string.Empty));
                int index = GetSongIndexById(id);
                return index;
            }
            else
            {
                int index = GetSongIndexByTitle(command);
                return index;
            }
        }
    }
}