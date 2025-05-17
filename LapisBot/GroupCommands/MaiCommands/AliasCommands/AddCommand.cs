using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LapisBot.GroupCommands.MaiCommands.AliasCommands;

public class AddCommand : AliasCommandBase
{
    public AddCommand()
    {
        CommandHead = new Regex("^add");
        DirectCommandHead = new Regex("^添加别名");

        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("aliasadd", "1");

        MaiCommandInstance.AddCommand = this;
    }

    public override void Initialize()
    {
        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data/local_aliases.json")))
            LocalAlias.Instance.AliasCollection.Aliases =
                JsonConvert.DeserializeObject<List<Alias>>(
                    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data/local_aliases.json")));
        
        foreach (var alias in MaiCommandInstance.SongAliases)
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

    private static void Save()
    {
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/local_aliases.json"),
            JsonConvert.SerializeObject(LocalAlias.Instance.AliasCollection.Aliases));
        Program.Logger.LogInformation("Local aliases have been saved");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        if (command.Split(" ").Length > 0)
        {
            var songIndicatorString = MaiCommandInstance.GetSongIndicatorString(command);

            if (songIndicatorString == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("添加失败！找不到歌曲！")
                ]);
                return;
            }

            var matchedSongs = MaiCommandInstance.GetSongs(songIndicatorString);
            var intendedAliasString = Regex.Replace(command, songIndicatorString, "", RegexOptions.IgnoreCase);
            if (intendedAliasString != "")
                intendedAliasString = intendedAliasString.Substring(1, intendedAliasString.Length - 1);

            if (matchedSongs.Length > 1)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg(GetMultiAliasesMatchedInformationString(matchedSongs, "alias add",
                        intendedAliasString, "添加别名"))
                ]);

                return;
            }

            if (intendedAliasString == "")
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("没有别名吗...那我怎么知道要添加什么啊喵！") //喵
                ]);
            }
            else
            {
                if (matchedSongs.Length == 0)
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                    [
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("添加失败！找不到歌曲！")
                    ]);
                }
                else if (matchedSongs.Length > 1)
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId, [
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg(GetMultiAliasesMatchedInformationString(matchedSongs, "alias add",
                            intendedAliasString, "添加别名"))
                    ]);
                }
                else
                {
                    var action = () =>
                    {
                        var id = matchedSongs[0].Id;

                        var success = !MaiCommandInstance.GetAliasById(id).Aliases.Contains(intendedAliasString) &&
                                      LocalAlias.Instance.Add(id, intendedAliasString);
                        if (success)
                        {
                            Program.Session.SendGroupMessageAsync(source.GroupId,
                            [
                                new CqReplyMsg(source.MessageId),
                                new CqTextMsg("添加成功！")
                            ]);
                            Save();
                        }
                        else
                        {
                            Program.Session.SendGroupMessageAsync(source.GroupId,
                            [
                                new CqReplyMsg(source.MessageId),
                                new CqTextMsg("已存在此别名")
                            ]);
                        }
                    };
                    TaskHandleQueue.HandlableTask task = new();
                    task.whenConfirm = action;
                    task.whenCancel = () =>
                    {
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                            new CqMessage
                            {
                                new CqReplyMsg(source.MessageId),
                                new CqTextMsg("别名添加已取消！")
                            });
                    };
                    var success = TaskHandleQueue.Singleton.AddTask(task);

                    if (success)
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                        [
                            new CqReplyMsg(source.MessageId),
                            new CqTextMsg(
                                $"你正在尝试为歌曲 \"{matchedSongs[0].Title}\" [{matchedSongs[0].Type}] 添加别名 \"{intendedAliasString}\""
                                + "\n发送 \"l handle confirm\" 以确认，发送 \"l handle cancel\" 以取消")
                        ]);
                    else
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                        [
                            new CqReplyMsg(source.MessageId),
                            new CqTextMsg("当前已有代办事项！请处理后再试！")
                        ]);
                }
            }
        }
        else
        {
            Program.Session.SendGroupMessageAsync(source.GroupId,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("没有参数吗...那我怎么知道要添加什么啊喵！")
            ]);
        }
    }
}

public class AliasCollection
{
    public List<MaiCommandBase.Alias> Aliases = [];

    public bool ContainsId(int id)
    {
        foreach (var alias in Aliases)
            if (alias.Id == id)
                return true;

        return false;
    }

    public void Add(int id, string aliasString)
    {
        if (ContainsId(id))
        {
            GetAlias(id).Aliases.Add(aliasString);
            return;
        }

        Aliases.Add(new MaiCommandBase.Alias { Id = id, Aliases = new List<string> { aliasString } });
    }

    public MaiCommandBase.Alias GetAlias(int id)
    {
        foreach (var alias in Aliases)
            if (alias.Id == id)
                return alias;

        return null;
    }

    public void Remove(int id)
    {
        Aliases.RemoveAt(id);
    }

    public int[] GetIds()
    {
        var ids = new List<int>();
        foreach (var alias in Aliases) ids.Add(alias.Id);

        return ids.ToArray();
    }
}

public class LocalAlias
{
    public readonly AliasCollection AliasCollection = new();

    private LocalAlias()
    {
    }

    public static LocalAlias Instance { get; } = new();

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
        AliasCollection.Remove(id);
        return true;
    }

    public List<string> Get(int id)
    {
        if (!AliasCollection.ContainsId(id)) return null;
        return AliasCollection.GetAlias(id).Aliases;
    }

    public int[] GetIds()
    {
        return AliasCollection.GetIds();
    }
}