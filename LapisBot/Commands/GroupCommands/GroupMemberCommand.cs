using System;
using System.Collections.Generic;
using System.IO;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Commands.GroupCommands.GroupMemberCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LapisBot.Commands.GroupCommands;

public abstract class GroupMemberCommandBase : GroupCommand
{
    protected static GroupMemberCommand GroupMemberCommandInstance;

    protected void MemberNotEnoughErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source, [
            new CqReplyMsg(source.MessageId), "最近发言的人数太少了，Lapis 找不到你的对象 _(:_」∠)_"
        ]);
    }

    protected void MemberNotHaveChatErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source, [
            new CqReplyMsg(source.MessageId), "该群友最近未发言！"
        ]);
    }

    protected string GetQqAvatarUrl(long id)
    {
        return $"http://q.qlogo.cn/headimg_dl?dst_uin={id}&spec=640&img_type=jpg";
    }
}

public class GroupMemberCommand : GroupMemberCommandBase
{
    public readonly HashSet<Group> Groups =
        File.Exists(Path.Combine(AppContext.BaseDirectory, "data/groups.json"))
            ? JsonConvert.DeserializeObject<HashSet<Group>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
                "data/groups.json")))
            : [];

    public GroupMemberCommand()
    {
        SubCommands = [new MarryCommand(), new RapeCommand()];
        GroupMemberCommandInstance = this;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        Groups.Add(new Group(source.GroupId));
        if (!Groups.TryGetValue(new Group(source.GroupId), out var group))
            return;

        group.Members.Add(new GroupMember(source.UserId));
        if (!group.Members.TryGetValue(new GroupMember(source.UserId), out var member))
            return;

        member.ChatCount++;
    }

    public bool RemoveMember(long id, long groupId)
    {
        if (Groups.TryGetValue(new Group(groupId), out var group))
            return group.Members.Remove(new GroupMember(id));
        return false;
    }

    public override void Initialize()
    {
        Program.DateChanged += DateChanged;
    }

    private void DateChanged(object sender, EventArgs e)
    {
        SaveData();
    }

    private void SaveData()
    {
        Program.Logger.LogInformation("Group member data have been saved");
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/groups.json"),
            JsonConvert.SerializeObject(Groups));
    }

    public override void Unload()
    {
        SaveData();
    }

    public class GroupMember(long id)
    {
        public long Id { get; } = id;
        public int ChatCount { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is GroupMember other && Id == other.Id;
        }
    }

    public class Group(long groupId)
    {
        public readonly long GroupId = groupId;
        public readonly HashSet<GroupMember> Members = [];

        public override int GetHashCode()
        {
            return GroupId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Group other && GroupId == other.GroupId;
        }
    }
}