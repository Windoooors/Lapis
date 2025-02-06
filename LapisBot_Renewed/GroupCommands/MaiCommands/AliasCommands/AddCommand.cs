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

namespace LapisBot_Renewed.GroupCommands.MaiCommands.AliasCommands
{
    public class AddCommand : MaiCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^add\s");
            DirectCommand = new Regex(@"^添加别名\s");
            DefaultSettings.SettingsName = "添加别名";
            CurrentGroupCommandSettings = DefaultSettings.Clone();

            MaiCommandCommand.AddCommand = this;
            
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
                LocalAlias.Instance.AliasCollection.Aliases =
                    JsonConvert.DeserializeObject<List<Alias>>(
                        File.ReadAllText(AppContext.BaseDirectory + "local_aliases.json"));
            
            return Task.CompletedTask;
        }

        private static void Save()
        {
            File.WriteAllText(AppContext.BaseDirectory + "local_aliases.json",
                JsonConvert.SerializeObject(LocalAlias.Instance.AliasCollection.Aliases));
            Console.WriteLine("Local aliases have been saved");
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            if (command.Split(" ").Length > 0)
            {
                var songIndicatorString = MaiCommandCommand.GetSongIndicatorString(command);

                if (songIndicatorString == null)
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        new CqMessage
                        {
                            new CqTextMsg("添加失败！找不到歌曲！")
                        });
                    return Task.CompletedTask;
                }

                var matchedSongs = MaiCommandCommand.GetSongs(songIndicatorString);
                var intendedAliasString = Regex.Replace(command, songIndicatorString, "", RegexOptions.IgnoreCase);
                if (intendedAliasString != "")
                    intendedAliasString = intendedAliasString.Substring(1, intendedAliasString.Length - 1);

                if (matchedSongs.Length > 1)
                {
                    string ids = string.Empty;
                    List<int> idsList = new List<int>();
                    for (int i = 0; i < matchedSongs.Length; i++)
                    {
                        ids += "ID " + matchedSongs[i].Id + " - " + matchedSongs[i].Title + " [" + matchedSongs[i].Type + "]";
                        if (i != matchedSongs.Length - 1)
                            ids += "\n";
                        idsList.Add(matchedSongs[i].Id);
                    }

                    Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                    {
                        new CqReplyMsg(source.MessageId), new CqTextMsg(
                            " 该别称有多首歌曲匹配：\n" + ids + "\n*发送 \"lps mai alias add ID " + idsList[0] + " " +
                            intendedAliasString + "\" 指令即可为歌曲 " +
                            matchedSongs[0].Title + " [" + matchedSongs[0].Type +
                            "] 添加别名")
                    });
                    
                    return Task.CompletedTask;
                }
                
                if (intendedAliasString == "")
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        new CqMessage
                        {
                            new CqTextMsg("没有别名吗...那我怎么知道要添加什么啊喵！") //喵
                        });
                }
                else
                {
                    if (matchedSongs.Length == 0)
                    {
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                            new CqMessage
                            {
                                new CqTextMsg("添加失败！找不到歌曲！")
                            });
                    }
                    else if (matchedSongs.Length > 1)
                    {
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                            new CqMessage
                            {
                                new CqTextMsg("添加失败！无法确定是哪个歌曲！")
                            });
                    }
                    else
                    {
                        Action action = () =>
                        {
                            var id = matchedSongs[0].Id;

                            var success = !MaiCommandCommand.GetAliasById(id).Aliases.Contains(intendedAliasString) &&
                                          LocalAlias.Instance.Add(id, intendedAliasString);
                            if (success)
                            {
                                Program.Session.SendGroupMessageAsync(source.GroupId,
                                    new CqMessage
                                    {
                                        new CqTextMsg("添加成功！")
                                    });
                                Save();
                            }
                            else
                                Program.Session.SendGroupMessageAsync(source.GroupId,
                                    new CqMessage
                                    {
                                        new CqTextMsg("已存在此别名")
                                    });
                        };
                        TaskHandleQueue.HandlableTask task = new();
                        task.whenConfirm = action;
                        task.whenCancel = () =>
                        {
                            Program.Session.SendGroupMessageAsync(source.GroupId,
                                new CqMessage
                                {
                                    new CqTextMsg("别名添加已取消！")
                                });
                        };
                        var success = TaskHandleQueue.Singleton.AddTask(task);

                        if (success)
                            Program.Session.SendGroupMessageAsync(source.GroupId,
                                new CqMessage
                                {
                                    new CqTextMsg("你正在尝试为歌曲 \"" + matchedSongs[0].Title + "\" " + "添加别名 \"" +
                                                  intendedAliasString + "\"" +
                                                  "\n发送 \"l handle confirm\" 以确认，发送 \"l handle cancel\" 以取消")
                                });
                        else
                            Program.Session.SendGroupMessageAsync(source.GroupId,
                                new CqMessage
                                {
                                    new CqTextMsg("当前已有代办事项！请处理后再试！")
                                });
                    }
                }
            }
            else
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqTextMsg("没有参数吗...那我怎么知道要添加什么啊喵！")
                    });
            }

            return Task.CompletedTask;
        }
    }

    public class AliasCollection
    {
        public List<MaiCommand.Alias> Aliases = new();

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

            Aliases.Add(new MaiCommand.Alias { Id = id, Aliases = new List<string> { aliasString } });
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

        public bool Remove(int id, string alias)
        {
            if (!AliasCollection.ContainsId(id)) return false;
            if (AliasCollection.GetAlias(id).Aliases.Contains(alias))
            {
                AliasCollection.GetAlias(id).Aliases.Remove(alias);
                return true;
            }

            return false;
        }

        public bool RemoveAll(int id)
        {
            if (!AliasCollection.ContainsId(id)) return false;
            else
            {
                AliasCollection.Remove(id);
                return true;
            }
        }

        public List<string> Get(int id)
        {
            if (!AliasCollection.ContainsId(id)) return null;
            else return AliasCollection.GetAlias(id).Aliases;
        }

        public int[] GetIds()
        {
            return AliasCollection.GetIds();
        }

        private LocalAlias()
        {
        }

        public static LocalAlias Instance { get; } = new();
    }
}