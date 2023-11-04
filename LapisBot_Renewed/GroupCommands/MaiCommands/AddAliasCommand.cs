using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Manganese.Text;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Shared;
using System;
using static LapisBot_Renewed.MaiCommand;
using System.Threading.Tasks;

namespace LapisBot_Renewed
{
    public class AddAliasCommand : AliasCommand
    {
        public AliasCommand aliasCommand;
        public InfoCommand infoCommand;
        public MaiCommand maiCommand;

        public override Task Initialize()
        {
            headCommand = new Regex(@"^add\s");
            if (System.IO.File.Exists(Environment.CurrentDirectory + "/alias.json"))
            {
                songAliases = JsonConvert.DeserializeObject<List<Alias>>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/alias.json"));

                if (maiCommand.songAliases.Count != 0)
                {
                    foreach (Alias alias in songAliases)
                    {
                        if (GetAliasFromGenericSongAliasesById(alias.id) != null)
                        {
                            var aliases = GetAliasFromGenericSongAliasesById(alias.id).aliases;
                            var newAliases = new List<string>();
                            foreach (string _alias in aliases)
                            {
                                newAliases.Add(_alias);
                            }
                            foreach (string _alias in alias.aliases)
                            {
                                var i = 0;
                                foreach (string __alias in aliases)
                                {
                                    i++;
                                    if (_alias == __alias)
                                    {
                                        break;
                                    }
                                    if (i == aliases.Count)
                                    {
                                        newAliases.Add(_alias);
                                    }
                                }
                            }
                            GetAliasFromGenericSongAliasesById(alias.id).aliases = newAliases;
                        }
                        else
                        {
                            maiCommand.songAliases.Add(alias);
                        }
                    }
                }
                else
                {
                    foreach (Alias alias in songAliases)
                    {
                        maiCommand.songAliases.Add(alias);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Alias GetAliasFromGenericSongAliasesById(int id)
        {
            foreach (Alias alias in maiCommand.songAliases)
            {
                if (alias.id == id)
                {
                    return alias;
                }
            }
            return null;
        }

        public bool isRepeated(Alias alias, string aliasString)
        {
            foreach (string _alias in alias.aliases)
            {
                if (_alias == aliasString)
                    return true;
            }
            return false;
        }

        public override void Unload()
        {
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "/alias.json", JsonConvert.SerializeObject(songAliases));
            Console.WriteLine("Alias data have been saved");
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+\s");
            var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
            var _command = string.Empty;
            var __command = string.Empty;
            var id = -1;
            var index = -1;
            if (idRegex.IsMatch(command))
            {
                _command = idRegex.Match(command).Value;
                id = idHeadRegex.Replace(_command, string.Empty).ToInt32();
                index = maiCommand.GetSongIndexById(id);
                __command = idRegex.Replace(command, string.Empty);
            }
            else
            {
                index = maiCommand.GetSongIndexByTitleUsingStartsWith(command);
                if (index != -1)
                {
                    id = maiCommand.songs[index].Id;
                    __command = command.Replace(maiCommand.songs[index].Title + " ", string.Empty);
                }
                else
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                    return;
                }
            }
            if (index != -1)
            {
                if (__command != "")
                {
                    var alias = new Alias() { aliases = new List<string>(), id = id };
                    if (maiCommand.GetAliasIndexById(id) != -1)
                    {
                        alias = maiCommand.songAliases[maiCommand.GetAliasIndexById(id)];
                    }
                    if (alias.aliases.Count != 0)
                    {
                        if (!isRepeated(alias, __command))
                        {
                            foreach(Alias ___alias in songAliases)
                            {
                                if (___alias.id == id)
                                {
                                    if (___alias.aliases.Count != 0)
                                    {
                                        //___alias.aliases.Add(__command);
                                        alias.aliases.Add(__command);
                                    }
                                    else
                                    {
                                        var __alias = new Alias() { id = id, aliases = new List<string>() };
                                        //__alias.aliases.Add(__command);
                                        songAliases.Add(__alias);
                                        alias.aliases.Add(__command);
                                    }
                                }
                            }

                        }
                        else
                        {
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 别名重复！") });
                            return;
                        }
                    }
                    else
                    {
                        var _alias = new Alias() { id = id, aliases = new List<string>() };
                        _alias.aliases.Add(__command);
                        maiCommand.songAliases.Add(_alias);
                        songAliases.Add(_alias);
                    }
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 已添加别名 “" + __command + "” 到歌曲 " + maiCommand.songs[maiCommand.GetSongIndexById(id)].Title + " [" + maiCommand.songs[maiCommand.GetSongIndexById(id)].Type + "]") });
                }
                else
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 请输入别名！") });
                }
            }
            else
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
        }
    }
}
