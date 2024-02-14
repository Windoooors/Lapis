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
using LapisBot_Renewed.Collections;

namespace LapisBot_Renewed.GroupCommands
{

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
        public Alias[] GetAliasByAliasString(string alias)
        {
            var aliases = new List<Alias>();
            foreach (var valueAlias in SongAliases)
            {
                foreach (var valueAliasString in valueAlias.Aliases)
                {
                    if (valueAliasString.Equals(alias))
                        aliases.Add(valueAlias);
                }
            }
            return aliases.ToArray();
        }

        public Alias[] GetAliasByAliasStringUsingStartsWith(string alias)
        {
            var aliases = new List<Alias>();
            foreach (Alias valueAlias in SongAliases)
            {
                foreach (var valueAliasString in valueAlias.Aliases)
                {
                    if (alias.ToLower().StartsWith(valueAliasString.ToLower()))
                        aliases.Add(valueAlias);
                }
            }
            return aliases.ToArray();
        }

        public string GetAliasStringUsingStartsWith(string alias)
        {
            foreach (Alias valueAlias in SongAliases)
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
            foreach (Alias alias in SongAliases)
            {
                if (alias.Id == id)
                {
                    return alias;
                }
            }
            return new Alias() { Id = id, Aliases = new List<string>() };
        }

        public Alias GetAliasByIdWithDifferentAliases(int id, List<Alias> SongAliases)
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

        public int GetAliasIndexById(int id)
        {
            int i = 0;
            foreach (Alias alias in SongAliases)
            {
                if (alias.Id == id)
                {
                    return i;
                }
                i += 1;
            }
            return -1;
        }

        public int GetSongIndexById(int id)
        {
            for (int i = 0; i < Songs.Length; i++)
            {
                if (Songs[i].Id == id)
                    return i;
            }
            return -1;
        }

        public int GetSongIndexByIdUsingStartsWith(string id)
        {
            var regex = new Regex(@"\d+");
            for (int i = 0; i < Songs.Length; i++)
            {
                if (regex.Matches(id)[0].ToString() == Songs[i].Id.ToString())
                    return i;
            }
            return -1;
        }

        public override Task Unload()
        {
            foreach (var subMaiCommand in SubCommands)
                subMaiCommand.Unload();
            return Task.CompletedTask;
        }

        public int GetSongIndexByTitle(string title)
        {
            for (int i = 0; i < Songs.Length; i++)
            {
                if (title.ToLower() == Songs[i].Title.ToLower())
                    return i;
            }
            return -1;
        }

        public int GetSongIndexByTitleUsingStartsWith(string title)
        {
            for (var i = 0; i < Songs.Length; i++)
            {
                if (title.ToLower().StartsWith(Songs[i].Title.ToLower() + " "))
                    return i;
            }
            return -1;
        }

        public MaiCommand MaiCommandCommand;
        public SongDto[] Songs;
        private JObject _aliasJObject;
        public List<List<SongDto>> Levels = new List<List<SongDto>>();
        public Dictionary<string, int> LevelDictionary = new Dictionary<string, int>();
        public readonly List<Alias> SongAliases = new List<Alias>();

        public class Alias
        {
            public List<string> Aliases;
            public int Id;
        }

        private async void Reload(object sender, EventArgs e)
        {
            await Start();
        }

        private Task Start()
        {
            SongAliases.Clear();
            LevelDictionary.Clear();
            SubCommands.Clear();
            Levels.Clear();

            try
            {
                if (OperatingSystem.IsLinux())
                    _aliasJObject = JObject.Parse(Program.apiOperator.Get("https://download.fanyu.site/maimai/alias.json"));
                else if (OperatingSystem.IsMacOS())
                    _aliasJObject = JObject.Parse(Program.apiOperator.Get("https://imgur.setchin.com/data/f_76686309.json"));
                //_aliasJObject = JObject.Parse("{\n    \"魔爪\": [\n      \"11260\",\n      \"11508\",\n      \"11507\"\n    ],\n    \"原神\": [\n      \"11260\"\n    ],\n    \"我草你妈\": [\n      \"11260\"\n    ],\n    \"你妈死了\": [\n      \"11507\"\n    ]\n  }");
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
                    SongAliases.Add(new Alias() { Aliases = aliasesList, Id = id });
                }
                else
                    SongAliases.Add(new Alias() { Aliases = Enumerates.ToList(obj.Value), Id = id });
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
            if (OperatingSystem.IsLinux())
                Songs = (SongDto[])JsonConvert.DeserializeObject(Program.apiOperator.Get("https://www.diving-fish.com/api/maimaidxprober/music_data"), typeof(SongDto[]));
            else if (OperatingSystem.IsMacOS())
                Songs = (SongDto[])JsonConvert.DeserializeObject(Program.apiOperator.Get("https://imgur.setchin.com/data/f_80421402.json"), typeof(SongDto[]));
            for (int i = 0; i < 24; i++)
            {
                Levels.Add(new List<SongDto>());
            }

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

            SubCommands.Add(new RandomCommand() { LevelDictionary = this.LevelDictionary, Levels = this.Levels, Songs = this.Songs, MaiCommandCommand = this });
            SubCommands.Add(new InfoCommand() { LevelDictionary = this.LevelDictionary, Levels = this.Levels, Songs = this.Songs, _aliasJObject = _aliasJObject, MaiCommandCommand = this });
            SubCommands.Add(new AliasCommand() { MaiCommandCommand = this });
            SubCommands.Add(new BestCommand() { MaiCommandCommand = this });
            SubCommands.Add(new PlateCommand());
            SubCommands.Add(new GuessCommand() { LevelDictionary = this.LevelDictionary, Levels = this.Levels, Songs = this.Songs, _aliasJObject = _aliasJObject, MaiCommandCommand = this });

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