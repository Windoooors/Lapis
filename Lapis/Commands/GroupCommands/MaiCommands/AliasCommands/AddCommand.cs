using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Miscellaneous;
using Lapis.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands.AliasCommands;

public class AddCommand : AliasCommandBase
{
    public AddCommand()
    {
        CommandHead = "add";
        DirectCommandHead = "添加别名";

        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("aliasadd", "1");
    }

    public override void Initialize()
    {
        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data/local_aliases.json")))
        {
            var alias =
                JsonConvert.DeserializeObject<List<Alias>>(
                    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data/local_aliases.json")));

            LocalAliasManager.Instance.AliasCollection.Aliases.AddRange(alias);
        }

        foreach (var alias in MaiCommandInstance.SongAliases)
        {
            var localAlias = LocalAliasManager.Instance;
            foreach (var e1 in localAlias.GetIds())
            {
                if (e1 != alias.Id)
                    continue;
                var a = LocalAliasManager.Instance.Get(e1);
                foreach (var aliasString in a)
                    if (alias.Aliases.Contains(aliasString))
                        alias.Aliases.Remove(aliasString);
            }
        }
    }

    private static void Save()
    {
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/local_aliases.json"),
            JsonConvert.SerializeObject(LocalAliasManager.Instance.AliasCollection.Aliases));
        Program.Logger.LogInformation("Local aliases have been saved");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source,
        long[] mentionedUserIds)
    {
        var isMatched = IdRegex.IsMatch(command);

        if (!isMatched)
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("添加失败！找不到歌曲！\nTips: 请使用歌曲 ID 来索引歌曲\n您可以通过 search 指令来获取歌曲 ID，例如 \"search FFT\"")
            ]);
            return;
        }

        var match = IdRegex.Match(command).ToString();
        var songId = int.Parse(IdHeadRegex.Replace(match, ""));

        command = IdRegex.Replace(command, "").Trim();

        var matchedSong = MaiCommandInstance.GetSong(songId);
        var intendedAliasString = command;
        if (intendedAliasString == "")
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("没有别名吗...那我怎么知道要添加什么啊喵！") //喵
            ]);
            return;
        }

        var action = () =>
        {
            var id = matchedSong.Id;

            var success = !MaiCommandInstance.GetAliasById(id).Aliases.Contains(intendedAliasString) &&
                          LocalAliasManager.Instance.Add(id, intendedAliasString);
            if (success)
            {
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("添加成功！")
                ]);
                Save();
            }
            else
            {
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("已存在此别名")
                ]);
            }
        };
        TaskHandleQueue.HandleableTask task = new();
        task.WhenConfirm = action;
        task.WhenCancel = () =>
        {
            SendMessage(source,
                new CqMessage
                {
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("别名添加已取消！")
                });
        };
        var success = TaskHandleQueue.Instance.AddTask(task, source.GroupId);

        if (success)
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(
                    $"你正在尝试为歌曲 \"{matchedSong.Title} [{matchedSong.Type}]\" 添加别名 \"{intendedAliasString}\""
                    + "\n发送 \"lps handle confirm\" 以确认，发送 \"lps handle cancel\" 以取消")
            ]);
        else
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("当前已有代办事项！请处理后再试！")
            ]);
    }
}

public class LocalAliasManager
{
    public readonly AliasCollection AliasCollection = new();

    public static LocalAliasManager Instance { get; } = new();

    public bool Add(int id, string alias)
    {
        if (AliasCollection.TryGetAlias(id, out var aliasOut) && aliasOut.Aliases.Contains(alias))
            return false;

        AliasCollection.Add<Alias>(id, alias);
        return true;
    }

    public bool Remove(int id, string alias)
    {
        if (!AliasCollection.TryGetAlias(id, out var aliasOut))
            return false;
        return aliasOut.Aliases.Remove(alias);
    }

    public bool RemoveAll(int index)
    {
        if (!AliasCollection.TryGetAlias(index, out var aliasOut)) return false;
        AliasCollection.Remove(index);
        return true;
    }

    public List<string> Get(long id)
    {
        return !AliasCollection.TryGetAlias(id, out var aliasOut)
            ? null
            : aliasOut.Aliases.ToList();
    }

    public long[] GetIds()
    {
        return AliasCollection.GetIds();
    }
}