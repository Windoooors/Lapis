using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Manganese.Text;
using LapisBot_Renewed.GroupCommands.MaiCommands;
using System.IO;
using System.Linq;
using LapisBot_Renewed.Collections;

namespace LapisBot_Renewed.GroupCommands
{

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

    public class MaiCommand : GroupCommand
    {
        public override Task Unload()
        {
            foreach (var subMaiCommand in SubCommands)
                subMaiCommand.Unload();
            return Task.CompletedTask;
        }

        private Alias[] GetAliasByAliasString(string alias)
        {
            var aliases = new List<Alias>();
            foreach (var valueAlias in _songAliases)
            {
                foreach (var valueAliasString in valueAlias.Aliases)
                {
                    if (valueAliasString.Equals(alias))
                        aliases.Add(valueAlias);
                }
            }

            return aliases.ToArray();
        }

        private Alias[] GetAliasByAliasStringUsingStartsWith(string alias)
        {
            var aliases = new List<Alias>();
            foreach (Alias valueAlias in _songAliases)
            {
                foreach (var valueAliasString in valueAlias.Aliases)
                {
                    if (alias.ToLower().StartsWith(valueAliasString.ToLower()))
                        aliases.Add(valueAlias);
                }
            }

            return aliases.ToArray();
        }

        private string GetAliasStringUsingStartsWith(string alias)
        {
            foreach (Alias valueAlias in _songAliases)
            {
                foreach (string valueAliasString in valueAlias.Aliases)
                {
                    if (alias.ToLower().StartsWith(valueAliasString.ToLower()))
                        return valueAliasString;
                }
            }

            return null;
        }

        public Alias GetAliasById(int id)
        {
            foreach (Alias alias in _songAliases)
            {
                if (alias.Id == id)
                {
                    return alias;
                }
            }

            return new Alias() { Id = id, Aliases = new List<string>() };
        }

        private Alias GetAliasByIdWithDifferentAliases(int id, List<Alias> SongAliases)
        {
            foreach (Alias alias in SongAliases)
            {
                if (alias.Id == id)
                {
                    return alias;
                }
            }

            return new Alias() { Id = id, Aliases = new List<string>() };
        }

        private int GetAliasIndexById(int id)
        {
            int i = 0;
            foreach (Alias alias in _songAliases)
            {
                if (alias.Id == id)
                {
                    return i;
                }

                i += 1;
            }

            return -1;
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

        private int GetSongIndexByIdUsingStartsWith(int id)
        {
            var regex = new Regex(@"\d+");
            for (int i = 0; i < Songs.Length; i++)
            {
                if (regex.Matches(id.ToString())[0].ToString() == Songs[i].Id.ToString())
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

        private int GetSongIndexByTitleUsingStartsWith(string title)
        {
            for (var i = 0; i < Songs.Length; i++)
            {
                if (title.ToLower().StartsWith(Songs[i].Title.ToLower() + " "))
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
            var aliases = GetAliasByAliasString(inputString);
            
            if (aliases.Length != 0)
            {
                var songsList = new List<SongDto>();
                foreach (var alias in aliases)
                {
                    songsList.Add(Songs[GetSongIndexById(alias.Id)]);
                }

                return songsList.ToArray();
            }

            var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
            var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
            if (idRegex.IsMatch(inputString))
            {
                try
                {
                    var id = idHeadRegex.Replace(inputString, string.Empty).ToInt32();
                    int index = GetSongIndexById(id);
                    if (index != -1)
                        return [Songs[index]];
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
                return [Songs[songIndex]];
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
                    if (inputString.StartsWith(aliasString))
                        tempAlias.Add(aliasString);
                }
            }

            foreach (var song in Songs)
            {
                if (inputString.StartsWith(song.Title))
                    tempTitles.Add(song.Title);

                if (inputString.StartsWith("id" + song.Id))
                    tempIds.Add("id" + song.Id);

                if (inputString.StartsWith("ID" + song.Id))
                    tempIds.Add("ID" + song.Id);

                if (inputString.StartsWith("id " + song.Id))
                    tempIds.Add("id " + song.Id);

                if (inputString.StartsWith("ID " + song.Id))
                    tempIds.Add("ID " + song.Id);
            }

            var sortedTempAlias = tempAlias.OrderByDescending(t => t.Length);
            var sortedTempIds = tempIds.OrderByDescending(t => t.Length);
            var sortedTempTitles = tempTitles.OrderByDescending(t => t.Length);
            
            foreach (string alias in sortedTempAlias)
                if (inputString.StartsWith(alias + " ") || inputString == alias)
                    return alias;
            foreach (string id in sortedTempIds)
                if (inputString.StartsWith(id + " ") || inputString == id)
                    return id;
            foreach (string title in sortedTempTitles)
                if (inputString.StartsWith(title + " ") || inputString == title)
                    return title;

            return null;
        }

        public SongDto[] GetSongsUsingStartsWith(string inputString)
        {
            var songIndicator = GetSongIndicatorString(inputString);

            if (songIndicator == null)
                return null;
            
            var aliases = GetAliasByAliasString(songIndicator);
            
            if (aliases.Length != 0)
            {
                var songsList = new List<SongDto>();
                foreach (var alias in aliases)
                {
                    songsList.Add(Songs[GetSongIndexById(alias.Id)]);
                }

                return songsList.ToArray();
            }

            var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
            var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
            if (idRegex.IsMatch(songIndicator))
            {
                try
                {
                    var id = idHeadRegex.Replace(songIndicator, string.Empty).ToInt32();
                    int index = GetSongIndexById(id);
                    if (index != -1)
                        return [Songs[index]];
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
                return [Songs[songIndex]];
            }

            return null;
        }

        public MaiCommand MaiCommandCommand;
        public SongDto[] Songs;
        public ExtraSongDto[] ExtraSongs;
        private JObject _aliasJObject;
        public readonly List<List<SongDto>> Levels = [];
        public readonly List<List<ExtraSongDto>> ExtraLevels = [];
        public readonly Dictionary<string, int> LevelDictionary = [];
        private readonly List<Alias> _songAliases = [];

        public class Alias
        {
            public List<string> Aliases;
            public int Id;

            public class AliasStringItem
            {
                public string AliasString;
                public bool IsAliasFromLapis;
            }
        }

        private async void Reload(object sender, EventArgs e)
        {
            await Start();
        }

        private Task Start()
        {
            _songAliases.Clear();
            LevelDictionary.Clear();
            SubCommands.Clear();
            Levels.Clear();
            ExtraLevels.Clear();

            try
            {
                if (OperatingSystem.IsLinux())
                    _aliasJObject = JObject.Parse(Program.apiOperator.Get("https://download.fanyu.site/maimai/alias.json"));
                else if (OperatingSystem.IsMacOS())
                    _aliasJObject = JObject.Parse(Program.apiOperator.Get("https://imgur.setchin.com/data/f_94470325.json"));
                /*_aliasJObject = JObject.Parse("{\n    \"魔爪\": [\n      \"11260\",\n      \"11508\",\n      \"11507\"\n    ],\n    \"原神\": [\n      \"11260\"\n    ],\n    \"我草你妈\": [\n      \"11260\"\n    ],\n    \"你妈死了\": [\n      \"11507\"\n    ]\n  }");
            */            
            }
            catch
            {
                _aliasJObject = new JObject();
            }

            var aliasObject = new Dictionary<string, string[]>();

            try
            {
                aliasObject = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(_aliasJObject.ToString());
            }
            catch
            {
                aliasObject = new Dictionary<string, string[]>();
            }

            foreach (KeyValuePair<string, string[]> obj in aliasObject)
            {
                var idString = obj.Key;
                var id = idString.ToInt32();
                if (id == 11422)
                {
                    var aliasesList = new List<string>();
                    foreach (string alias in obj.Value)
                        if (!(alias == "\u200e\u200e" || alias == "ㅤ" || alias == String.Empty))
                            aliasesList.Add(alias);
                    _songAliases.Add(new Alias() { Aliases = aliasesList, Id = id });
                }
                else
                    _songAliases.Add(new Alias() { Aliases = Enumerates.ToList(obj.Value), Id = id });
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
            if (OperatingSystem.IsLinux())
                Songs = (SongDto[])JsonConvert.DeserializeObject(Program.apiOperator.Get("https://www.diving-fish.com/api/maimaidxprober/music_data"), typeof(SongDto[]));
            else if (OperatingSystem.IsMacOS())
                Songs = (SongDto[])JsonConvert.DeserializeObject(Program.apiOperator.Get("https://imgur.setchin.com/data/f_70738752.json"), typeof(SongDto[]));
            for (int i = 0; i < 24; i++)
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

            SubCommands.Add(new RandomCommand() { MaiCommandCommand = this });
            SubCommands.Add(new InfoCommand() { MaiCommandCommand = this });
            SubCommands.Add(new AliasAddCommand() { MaiCommandCommand = this });
            SubCommands.Add(new AliasCommand() { MaiCommandCommand = this });
            SubCommands.Add(new BestCommand() { MaiCommandCommand = this });
            SubCommands.Add(new PlateCommand() { MaiCommandCommand = this });
            SubCommands.Add(new GuessCommand() { MaiCommandCommand = this });
            SubCommands.Add(new QueueQueryCommand() { MaiCommandCommand = this });
            
            foreach (var subMaiCommand in SubCommands)
            {
                subMaiCommand.Initialize();
                subMaiCommand.ParentCommand = this;
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
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings");
                
            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }
            Start();
            return Task.CompletedTask;
        }

        public override Task SubParse(string command, GroupMessageReceiver source)
        {
            return Task.CompletedTask;
        }

        public int GetSongIndex(string command)
        {
            var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
            var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
            if (idRegex.IsMatch(command))
            {
                var id = idHeadRegex.Replace(command, string.Empty).ToInt32();
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