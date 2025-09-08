using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.GroupMemberCommands;
using Lapis.Commands.GroupCommands.GroupMemberCommands.MemberAliasCommands;
using Lapis.Miscellaneous;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands;

public abstract class GroupMemberCommandBase : GroupCommand
{
    public static GroupMemberCommand GroupMemberCommandInstance;

    protected void MemberNotEnoughErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source, [
            new CqReplyMsg(source.MessageId), $"最近发言的人数太少了，{BotConfiguration.Instance.BotName} 找不到你的对象 _(:_」∠)_"
        ]);
    }

    protected void MemberNotHaveChattedErrorHelp(CqGroupMessagePostContext source)
    {
        SendMessage(source, [
            new CqReplyMsg(source.MessageId), "该群友最近未发言！"
        ]);
    }

    protected string GetQqAvatarUrl(long id)
    {
        return $"http://q.qlogo.cn/headimg_dl?dst_uin={id}&spec=640&img_type=jpg";
    }

    protected bool TryGetNickname(long id, long groupId, out string nickname)
    {
        var memberInformation = Program.Session.GetGroupMemberInformation(groupId, id);

        if (memberInformation == null || memberInformation.Status == CqActionStatus.Failed)
        {
            GroupMemberCommandInstance.RemoveMember(id, groupId);
            nickname = null;
            return false;
        }

        nickname = memberInformation.GroupNickname == ""
            ? memberInformation.Nickname
            : memberInformation.GroupNickname;
        return true;
    }

    protected string GetMultiAliasesMatchedInformationString(GroupMemberCommand.GroupMember[] members, string command,
        long groupId)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("该别称有多位群友匹配：");

        foreach (var member in members)
        {
            if (!TryGetNickname(member.Id, groupId, out var nickname))
                continue;
            stringBuilder.AppendLine($"{member.Id} - {nickname}");
        }

        var j = 0;
        for (var i = 0; i < members.Length; i++)
        {
            if (!TryGetNickname(members[i].Id, groupId, out _))
                continue;
            j = i;
            break;
        }

        stringBuilder.Append(
            $"*发送 \"lps {command} {members[j].Id}\" 指令即可使用目标功能");

        return stringBuilder.ToString();
    }

    protected string GetMultiSearchResultInformationString(string keyword, string command,
        long groupId, bool findAgreedWithEula = false)
    {
        var searchResult = SearchMemberCommand.SearchMemberCommandInstance.Search(keyword, groupId,
            findAgreedWithEula);

        var stringBuilder = new StringBuilder();

        if (searchResult.MembersMatchedByAlias.Count >= 30)
        {
            stringBuilder.AppendLine("搜索结果过多，请提供更多关键词");
        }
        else
        {
            stringBuilder = SearchMemberCommand.SearchMemberCommandInstance.GetMultiSearchResults(
                SearchMemberCommand.SearchMemberCommandInstance.Search(keyword, groupId, findAgreedWithEula), groupId
            );

            if (searchResult.MembersMatchedByAlias.Count != 0)
            {
                var j = 0;
                for (var i = 0; i < searchResult.MembersMatchedByAlias.Count; i++)
                {
                    if (!TryGetNickname(searchResult.MembersMatchedByAlias.Keys.ToArray()[i].Id, groupId,
                            out _))
                        continue;
                    j = i;
                    break;
                }

                stringBuilder.Append(
                    $"*发送 \"lps {command} {searchResult.MembersMatchedByAlias.Keys.ToArray()[j].Id}\" 指令即可使用目标功能");
            }
            else
            {
                stringBuilder.Clear();
                stringBuilder = new StringBuilder("未找到该群友");
            }
        }

        return stringBuilder.ToString();
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
        SubCommands =
        [
            new MarryCommand(), new RapeCommand(), new MemberAliasCommand(), new SearchMemberCommand(),
            new BeingRapedCommand()
        ];
        GroupMemberCommandInstance = this;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        Groups.Add(new Group(source.GroupId));
        if (!Groups.TryGetValue(new Group(source.GroupId), out var group))
            return;

        group.Members.Add(new GroupMember(source.UserId));
        if (!group.Members.TryGetValue(new GroupMember(source.UserId), out _))
            return;
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

    public bool AgreeWithEula(long userId, long groupId)
    {
        if (!GroupMemberCommandInstance.Groups.TryGetValue(new Group(groupId), out var group))
            return false;

        foreach (var groupMember in group.Members)
            if (groupMember.Id == userId)
            {
                groupMember.AgreedWithEula = true;
                return true;
            }

        return false;
    }

    public bool TryGetMember(string userIdentificationString, long groupId, out GroupMember[] members,
        bool findAgreedWithEula = false)
    {
        members = [];

        if (!GroupMemberCommandInstance.Groups.TryGetValue(new Group(groupId),
                out var group))
        {
            members = [];
            return false;
        }

        if (long.TryParse(userIdentificationString, out var userNumber) &&
            group.Members.TryGetValue(new GroupMember(userNumber), out var foundMember))
        {
            members = [foundMember];

            if (findAgreedWithEula)
                members = members.Where(x => x.AgreedWithEula).Select(x => x).ToArray();
            
            return members.Length > 0;
        }

        var memberHashset = new HashSet<GroupMember>();

        foreach (var singleMember in group.Members)
        {
            var alias = GetAliasById(singleMember.Id, groupId);

            if (alias == null)
                continue;

            foreach (var aliasString in alias.Aliases)
                if (aliasString.Equals(userIdentificationString))
                    memberHashset.Add(singleMember);
        }

        if (memberHashset.Count != 0)
        {
            members = memberHashset.ToArray();
            
            if (findAgreedWithEula)
                members = members.Where(x => x.AgreedWithEula).Select(x => x).ToArray();

            return members.Length > 0;
        }

        var searchResult = SearchMemberCommand.SearchMemberCommandInstance.Search(userIdentificationString, groupId);

        var searchedMembers = searchResult.MembersMatchedByAlias.Keys.ToHashSet();

        if (searchedMembers.Count == 1)
        {
            members = searchedMembers.ToArray();
            
            if (findAgreedWithEula)
                members = members.Where(x => x.AgreedWithEula).Select(x => x).ToArray();

            return members.Length > 0;
        }

        members = [];
        return false;
    }

    public Alias GetAliasById(long id, long groupId)
    {
        var aliases = MemberAliasManager.Instance.GetMemberAliasManager(groupId).AliasCollection.Aliases;

        foreach (var alias in aliases)
            if (alias.Id == id)
                return alias;

        return new Alias
        {
            Id = id,
            Aliases = []
        };
    }

    public class GroupMember(long id)
    {
        public bool AgreedWithEula;
        
        public long Id { get; } = id;

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
        public long GroupId = groupId;
        public HashSet<GroupMember> Members = [];

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