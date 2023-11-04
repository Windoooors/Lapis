using System;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Manganese.Text;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using static LapisBot_Renewed.InfoCommand;
using static LapisBot_Renewed.AliasCommand;

namespace LapisBot_Renewed
{

    public class SongDto
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("level")]
        public string[] Levels;

        [JsonProperty("ds")]
        public float[] Ratings;

        [JsonProperty("charts")]
        public ChartDto[] Charts;

        [JsonProperty("basic_info")]
        public BasicInfoDto BasicInfo;

        public class ChartDto
        {
            [JsonProperty("charter")]
            public string Charter;
        }

        public class BasicInfoDto
        {
            [JsonProperty("artist")]
            public string Artist;

            [JsonProperty("from")]
            public string Version;
        }
    }

    public class MaiCommand : GroupCommand
    {
        public Alias[] GetAliasByAliasString(string alias)
        {
            var aliases = new List<Alias>();
            foreach (Alias _alias in songAliases)
            {
                foreach (string __alias in _alias.aliases)
                {
                    if (__alias == alias)
                        aliases.Add(_alias);
                }
            }
            return aliases.ToArray();
        }

        public Alias GetAliasById(int id)
        {
            foreach (Alias alias in songAliases)
            {
                if (alias.id == id)
                {
                    return alias;
                }
            }
            return new Alias() { id = id, aliases = new List<string>() };
        }

        public Alias GetAliasByIdWithDifferentAliases(int id, List<Alias> _songAliases)
        {
            foreach (Alias alias in _songAliases)
            {
                if (alias.id == id)
                {
                    return alias;
                }
            }
            return new Alias() { id = id, aliases = new List<string>() };
        }

        public int GetAliasIndexById(int id)
        {
            int i = 0;
            foreach (Alias alias in songAliases)
            {
                if (alias.id == id)
                {
                    return i;
                }
                i += 1;
            }
            return -1;
        }

        public int GetSongIndexById(int id)
        {
            for (int i = 0; i < songs.Length; i++)
            {
                if (songs[i].Id == id)
                    return i;
            }
            return -1;
        }

        public override void Unload()
        {
            foreach (MaiCommand maiCommand in subCommands)
                maiCommand.Unload();
        }

        public int GetSongIndexByTitle(string title)
        {
            for (int i = 0; i < songs.Length; i++)
            {
                if (title == songs[i].Title)
                    return i;
            }
            return -1;
        }

        public int GetSongIndexByTitleUsingStartsWith(string title)
        {
            for (int i = 0; i < songs.Length; i++)
            {
                if (title.StartsWith(songs[i].Title + " "))
                    return i;
            }
            return -1;
        }

        public MaiCommand maiCommand;
        public SongDto[] songs;
        public JObject aliasJObject;
        public List<List<SongDto>> levels = new List<List<SongDto>>();
        public Dictionary<string, int> levelDictionary = new Dictionary<string, int>();
        public List<Alias> songAliases = new List<Alias>();

        public List<MaiCommand> subCommands = new List<MaiCommand>();

        public class Alias
        {
            public List<string> aliases;
            public int id;
        }

        private void Reload(object sender, EventArgs e)
        {
            Start();
        }

        public void Start()
        {
            songAliases.Clear();
            levelDictionary.Clear();
            subCommands.Clear();
            levels.Clear();

            try
            {
                aliasJObject = JObject.Parse(Program.apiOperator.Get("https://download.fanyu.site/maimai/alias.json"));
                //aliasJObject = JObject.Parse("{\n    \"魔爪\": [\n      \"11260\",\n      \"11508\",\n      \"11507\"\n    ],\n    \"原神\": [\n      \"11260\"\n    ],\n    \"我草你妈\": [\n      \"11260\"\n    ],\n    \"你妈死了\": [\n      \"11507\"\n    ]\n  }");
            }
            catch
            {
                aliasJObject = new JObject();
            }

            var aliasObject = new Dictionary<string, string[]>();

            try
            {
                aliasObject = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(aliasJObject.ToString());
            }
            catch
            {
                aliasObject = new Dictionary<string, string[]>();
            }
            foreach (KeyValuePair<string, string[]> obj in aliasObject)
            {
                foreach (string idString in obj.Value)
                {
                    if (idString != "未找到")
                    {
                        var id = idString.ToInt32();
                        if (songAliases.Count == 0)
                        {
                            songAliases.Add(new Alias() { aliases = new List<string>() { obj.Key }, id = id });
                            continue;
                        }
                        var i = 0;
                        foreach (Alias alias in songAliases)
                        {
                            i++;
                            if (alias.id == id)
                            {
                                alias.aliases.Add(obj.Key);
                                break;
                            }
                            else if (i == songAliases.Count)
                            {
                                songAliases.Add(new Alias() { aliases = new List<string>() { obj.Key }, id = id });
                                i = 0;
                                break;
                            }
                        }
                    }
                }
            }

            levelDictionary.Add("1", 0);
            levelDictionary.Add("2", 1);
            levelDictionary.Add("3", 2);
            levelDictionary.Add("4", 3);
            levelDictionary.Add("5", 4);
            levelDictionary.Add("6", 5);
            levelDictionary.Add("6+", 6);
            levelDictionary.Add("7", 7);
            levelDictionary.Add("7+", 8);
            levelDictionary.Add("8", 9);
            levelDictionary.Add("8+", 10);
            levelDictionary.Add("9", 11);
            levelDictionary.Add("9+", 12);
            levelDictionary.Add("10", 13);
            levelDictionary.Add("10+", 14);
            levelDictionary.Add("11", 15);
            levelDictionary.Add("11+", 16);
            levelDictionary.Add("12", 17);
            levelDictionary.Add("12+", 18);
            levelDictionary.Add("13", 19);
            levelDictionary.Add("13+", 20);
            levelDictionary.Add("14", 21);
            levelDictionary.Add("14+", 22);
            levelDictionary.Add("15", 23);

            songs = (SongDto[])JsonConvert.DeserializeObject(Program.apiOperator.Get("https://www.diving-fish.com/api/maimaidxprober/music_data"), typeof(SongDto[]));
            for (int i = 0; i < 24; i++)
            {
                levels.Add(new List<SongDto>());
            }

            foreach (SongDto song in songs)
            {
                foreach (string level in song.Levels)
                {
                    int j;
                    levelDictionary.TryGetValue(level, out j);
                    levels[j].Add(song);
                }
            }

            subCommands.Add(new RandomCommand() { levelDictionary = this.levelDictionary, levels = this.levels, songs = this.songs, maiCommand = this });
            subCommands.Add(new InfoCommand() { levelDictionary = this.levelDictionary, levels = this.levels, songs = this.songs, aliasJObject = aliasJObject, maiCommand = this });
            subCommands.Add(new AliasCommand() { maiCommand = this});
            subCommands.Add(new BestCommand() { maiCommand = this });
            subCommands.Add(new PlateCommand());

            foreach (MaiCommand maiCommand in subCommands)
                maiCommand.Initialize();
            Console.WriteLine("MaiCommand Initialized");
        }

        public override Task Initialize()
        {
            Program.DateChanged += Reload;
            headCommand = new Regex(@"^mai\s");
            Start();
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            foreach (MaiCommand subCommand in subCommands)
            {
                if (subCommand.headCommand.IsMatch(command) && subCommand.headCommand.Replace(command, "") != string.Empty)
                {
                    command = subCommand.headCommand.Replace(command, "");
                    subCommand.Parse(command, source);
                }
            }
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