using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class AliasAddCommand : MaiCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^alias add\s");
            DirectCommand = new Regex(@"^添加别名\s");
            DefaultSettings.SettingsName = "添加别名";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            if (File.Exists(AppContext.BaseDirectory + "local_aliases.json"))
                LocalAlias.Singleton.AliasCollection.Aliases =
                    JsonConvert.DeserializeObject<List<Alias>>(File.ReadAllText(AppContext.BaseDirectory + "local_aliases.json"));

            return Task.CompletedTask;
        }

        public override Task Unload()
        {
            File.WriteAllText(AppContext.BaseDirectory + "local_aliases.json",
                JsonConvert.SerializeObject(LocalAlias.Singleton.AliasCollection.Aliases)); 
            Console.WriteLine("Local aliases have been saved");
            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            var cmds = command.Split(" ");

            if(cmds.Length > 0)
            {

                var oname = "";
                for(int i = 1;i < cmds.Length;i++)
                {
                    oname+=cmds[i] + " ";
                }

                oname = oname.Substring(0, oname.Length - 1);


                if(oname == "")
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                    [
                        new CqTextMsg("没有别名吗...那我怎么知道要添加什么啊喵！") //喵
                    ]);
                }
                else
                {
                    var songs = MaiCommandCommand.GetSongs(cmds[0]);

                    if(songs == null || songs.Length == 0)
                    {
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                        [
                            new CqTextMsg("添加失败！找不到歌曲！")
                        ]);
                    }
                    else if(songs.Length > 1)
                    {
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                        [
                            new CqTextMsg("添加失败！无法确定是哪个歌曲！")
                        ]);
                    }
                    else
                    {
                        var id = songs[0].Id;
                        
                        var success = !MaiCommandCommand.GetAliasById(id).Aliases.Contains(oname) && LocalAlias.Singleton.Add(id,oname);
                        if (success)
                            Program.Session.SendGroupMessageAsync(source.GroupId,
                            [
                                new CqTextMsg("添加成功！")
                            ]);
                        else
                            Program.Session.SendGroupMessageAsync(source.GroupId,
                            [
                                new CqTextMsg("已存在此别名")
                            ]);
                    }
                }
            }
            else
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqTextMsg("没有参数吗...那我怎么知道要添加什么啊喵！")
                ]);
            }

            return Task.CompletedTask;
        }
    }
    
    public class AliasCollection()
    {
        public List<MaiCommand.Alias> Aliases = [];

        public bool ContainsId(int id)
        {
            foreach (var alias in Aliases)
            {
                if (alias.Id == id)
                    return true;
            }

            return false;
        }

        public void Add(int id, string aliasString)
        {
            if (ContainsId(id))
            {
                GetAlias(id).Aliases.Add(aliasString);
                return;
            }

            Aliases.Add(new MaiCommand.Alias { Id = id, Aliases = [aliasString] });
        }

        public MaiCommand.Alias GetAlias(int id)
        {
            foreach (var alias in Aliases)
            {
                if (alias.Id == id)
                    return alias;
            }

            return null;
        }

        public void Remove(int id)
        {
            Aliases.RemoveAt(id);
        }
            
        public int[] GetIds()
        {
            var ids = new List<int>();
            foreach (var alias in Aliases)
            {
                ids.Add(alias.Id);
            }

            return ids.ToArray();
        }
    }
    
    public class LocalAlias
    {
        public readonly AliasCollection AliasCollection = new();

        public bool Add(int id, string alias)
        {
            if (AliasCollection.GetAlias(id) != null && AliasCollection.GetAlias(id).Aliases.Contains(alias))
                return false;

            AliasCollection.Add(id, alias);
            return true;
        }

        public bool Remove(int id,string alias)
        {
            if(!AliasCollection.ContainsId(id))return false;
            if(AliasCollection.GetAlias(id).Aliases.Contains(alias))
            {
                AliasCollection.GetAlias(id).Aliases.Remove(alias);
                return true;
            }
            return false;
        }
        public bool RemoveAll(int id)
        {
            if(!AliasCollection.ContainsId(id))return false;
            else
            {
                AliasCollection.Remove(id);
                return true;
            }
        }
        public List<string> Get(int id)
        {
            if(!AliasCollection.ContainsId(id))return null;
            else return AliasCollection.GetAlias(id).Aliases;
        }

        public int[] GetIds()
        {
            return AliasCollection.GetIds();
        }

        private LocalAlias(){}

        public static LocalAlias Singleton{get;} = new();
    }
}