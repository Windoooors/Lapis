using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Miscellaneous;
using Lapis.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands.MemberAliasCommands;

public class MemberAliasAddCommand : MemberAliasCommandBase
{
    public MemberAliasAddCommand()
    {
        CommandHead = "add";
        DirectCommandHead = "添加群友别名";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("maliasadd", "1");
        IntendedArgumentCount = 2;
    }

    public override void ParseWithArgument(string[] arguments, CqGroupMessagePostContext source)
    {
        if (arguments.Length < IntendedArgumentCount)
        {
            HelpCommand.Instance.ArgumentErrorHelp(source);
            return;
        }

        GroupMemberCommand.GroupMember[] members = [];
        long intendedUserId = 0;

        if (!GroupMemberCommandInstance.TryGetMember(arguments[0], source.GroupId, out members))
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                "未指定群友！"
            ]);
            return;
        }

        if (members.Length > 1)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                "无法确定是哪个群友！"
            ]);
            return;
        }

        if (members.Length == 1)
            intendedUserId = members[0].Id;

        if (arguments[1] == "")
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                "未指定别名！"
            ]);
            return;
        }

        var intendedAlias = arguments[1];

        var groupedMemberAliasManager = MemberAliasManager.Instance.GetMemberAliasManager(source.GroupId);
        var action = () =>
        {
            var id = intendedUserId;

            var success =
                groupedMemberAliasManager.Add(id, intendedAlias);
            if (success)
            {
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("添加成功！")
                ]);
                MemberAliasManager.Instance.Save();
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

        if (!TryGetNickname(intendedUserId, source.GroupId, out var nickname))
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("该群友已退群")
            ]);
            return;
        }

        if (success)
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(
                    $"你正在尝试为群友 \"{nickname}\" 添加别名 \"{intendedAlias}\""
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

public class MemberAliasManager
{
    private static readonly string SavePath = Path.Combine(AppContext.BaseDirectory, "data/member_aliases.json");

    private readonly HashSet<GroupedMemberAliasManager> _memberAliasManagers =
        File.Exists(SavePath)
            ? JsonConvert.DeserializeObject<HashSet<GroupedMemberAliasManager>>(
                File.ReadAllText(SavePath))
            : [];

    public static MemberAliasManager Instance { get; } = new();

    public GroupedMemberAliasManager GetMemberAliasManager(long groupId)
    {
        var manager = _memberAliasManagers.ToList().Find(x => x.GroupId == groupId);
        if (manager is null || manager.GroupId == 0)
        {
            _memberAliasManagers.Add(new GroupedMemberAliasManager(groupId));
            return GetMemberAliasManager(groupId);
        }

        return manager;
    }

    public void Save()
    {
        File.WriteAllText(SavePath,
            JsonConvert.SerializeObject(_memberAliasManagers));
        Program.Logger.LogInformation("Member aliases have been saved");
    }
}

public class GroupedMemberAliasManager(long groupId)
{
    public AliasCollection AliasCollection = new();

    public long GroupId { get; } = groupId;

    public bool Add(long id, string alias)
    {
        if (AliasCollection.TryGetAlias(id, out var aliasOut) && aliasOut.Aliases.Contains(alias))
            return false;

        AliasCollection.Add<Alias>(id, alias);
        return true;
    }

    public bool Remove(long id, string alias)
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

    public override int GetHashCode()
    {
        return GroupId.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is GroupedMemberAliasManager other && GroupId == other.GroupId;
    }
}