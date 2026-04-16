using System.Collections.Generic;
using System.Linq;
using System.Text;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.GroupMemberCommands;
using Lapis.Operations.DatabaseOperation;
using Microsoft.EntityFrameworkCore;

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

    public bool TryGetNickname(long id, long groupId, out string nickname)
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

    public bool TryGetNickname(long id, out string nickname)
    {
        var memberInformation = Program.Session.GetStrangerInformation(id);

        if (memberInformation == null || memberInformation.Status == CqActionStatus.Failed)
        {
            nickname = null;
            return false;
        }

        nickname = memberInformation.Nickname;
        return true;
    }

    public string GetMultiAliasesMatchedInformationString(GroupMember[] members,
        CommandBehaviorInformationDataObject behaviorInformation,
        long groupId)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("该别称有多位群友匹配：");

        foreach (var member in members)
        {
            if (!TryGetNickname(member.QqId, groupId, out var nickname))
                continue;
            stringBuilder.AppendLine($"{member.QqId} - {nickname}");
        }

        var j = 0;
        var exampleNickname = "";
        for (var i = 0; i < members.Length; i++)
        {
            if (!TryGetNickname(members[i].QqId, groupId, out exampleNickname))
                continue;
            j = i;
            break;
        }

        var memberId = members[j].QqId;

        stringBuilder.Append(
            behaviorInformation.ExtraParameterStrings == null
                ? behaviorInformation.PassiveToContentSubject
                    ? $"*发送 \"lps {behaviorInformation.CommandString} {memberId}\" 指令即可{behaviorInformation.FunctionString}"
                    : $"*发送 \"lps {behaviorInformation.CommandString} {memberId}\" 指令即可查询群友 {exampleNickname} ({memberId}) 的{behaviorInformation.FunctionString}"
                : $"*发送 \"lps {behaviorInformation.CommandString} {memberId} {
                    behaviorInformation.ExtraParameterStrings.Aggregate(
                        new StringBuilder(), (builder, item) => builder.Append(item).Append(' ')
                    ).ToString().Trim()
                }\" 指令即可{(behaviorInformation.ContentModification ? "为群友" : "查询群友")} {exampleNickname} ({memberId}) {(behaviorInformation.ContentModification ? "" : "的")}{behaviorInformation.FunctionString}"
        );

        return stringBuilder.ToString();
    }

    public string GetMultiSearchResultInformationString(string keyword,
        CommandBehaviorInformationDataObject behaviorInformation,
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
                var exampleNickname = "";
                for (var i = 0; i < searchResult.MembersMatchedByAlias.Count; i++)
                {
                    if (!TryGetNickname(searchResult.MembersMatchedByAlias.Keys.ToArray()[i].QqId, groupId,
                            out exampleNickname))
                        continue;
                    j = i;
                    break;
                }

                var memberId = searchResult.MembersMatchedByAlias.Keys.ToArray()[j].QqId;

                stringBuilder.Append(
                    behaviorInformation.ExtraParameterStrings == null
                        ? behaviorInformation.PassiveToContentSubject
                            ? $"*发送 \"lps {behaviorInformation.CommandString} {memberId}\" 指令即可{behaviorInformation.FunctionString}"
                            : $"*发送 \"lps {behaviorInformation.CommandString} {memberId}\" 指令即可查询群友 {exampleNickname} ({memberId}) 的{behaviorInformation.FunctionString}"
                        : $"*发送 \"lps {behaviorInformation.CommandString} {memberId} {
                            behaviorInformation.ExtraParameterStrings.Aggregate(
                                new StringBuilder(), (builder, item) => builder.Append(item).Append(' ')
                            ).ToString().Trim()
                        }\" 指令即可{(behaviorInformation.ContentModification ? "为群友" : "查询群友")} {exampleNickname} ({memberId}) {(behaviorInformation.ContentModification ? "" : "的")}{behaviorInformation.FunctionString}"
                );
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
    public GroupMemberCommand()
    {
        SubCommands =
        [
            new MarryCommand(), new RapeCommand(), new MemberAliasCommand(), new SearchMemberCommand(),
            new BeingRapedCommand(), new RapeRankCommand()
        ];
        GroupMemberCommandInstance = this;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;
        DatabaseHandler.Instance.GroupMemberDatabaseOperator.AddMember(source.UserId, source.GroupId, db);
    }

    public void RemoveMember(long id, long groupId)
    {
        using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;
        DatabaseHandler.Instance.GroupMemberDatabaseOperator.RemoveMember(id, groupId, db);
    }

    public void AgreeWithEula(long userId, long groupId)
    {
        using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;

        var member = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetMember(userId, groupId, db);

        if (member != null)
        {
            member.AgreedEula = true;
            db.SaveChanges();
        }
    }

    public bool TryGetMember(string userIdentificationString, out GroupMember[] members,
        CqGroupMessagePostContext source,
        CommandBehaviorInformationDataObject behaviorInformation = null, bool sendMessage = false,
        bool findAgreedWithEula = false)
    {
        var groupId = source.GroupId;

        members = [];

        /*if (!GroupMemberCommandInstance.Groups.TryGetValue(new Group(groupId),
                     out var group))
        {
            members = [];

            if (sendMessage)
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    GetMultiSearchResultInformationString(userIdentificationString, behaviorInformation, groupId,
                        findAgreedWithEula)
                ]);
            return false;
        }*/

        using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;

        if (long.TryParse(userIdentificationString, out var userNumber))
        {
            var foundMember = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetMember(userNumber, groupId, db);

            if (foundMember != null)
            {
                members = [foundMember];

                if (findAgreedWithEula)
                    members = members.Where(x => x.AgreedEula).Select(x => x).ToArray();

                if (members.Length == 0)
                {
                    if (sendMessage)
                        SendMessage(source,
                        [
                            new CqReplyMsg(source.MessageId),
                            GetMultiSearchResultInformationString(userIdentificationString, behaviorInformation,
                                groupId,
                                findAgreedWithEula)
                        ]);
                    return false;
                }

                return true;
            }
        }

        var memberHashset = new HashSet<GroupMember>();

        var dataset = db.MemberAliasesDataSet.Include(x => x.Aliases).ToList();

        var aliases =
            dataset.Where(x => x.Aliases.Exists(y => y.Alias.ToLower() == userIdentificationString?.ToLower()) &&
                               x.GroupId == groupId)
                .ToArray();

        foreach (var singleAlias in aliases)
            memberHashset.Add(
                DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetMember(singleAlias.QqId, groupId, db));

        if (memberHashset.Count != 0)
        {
            members = memberHashset.ToArray();

            if (findAgreedWithEula)
                members = members.Where(x => x.AgreedEula).Select(x => x).ToArray();

            if (members.Length == 0)
            {
                if (sendMessage)
                    SendMessage(source,
                    [
                        new CqReplyMsg(source.MessageId),
                        GetMultiSearchResultInformationString(userIdentificationString, behaviorInformation, groupId,
                            findAgreedWithEula)
                    ]);
                return false;
            }

            return true;
        }

        var searchResult = SearchMemberCommand.SearchMemberCommandInstance.Search(userIdentificationString, groupId);

        var searchedMembers = searchResult.MembersMatchedByAlias.Keys.ToHashSet();

        if (searchedMembers.Count == 1)
        {
            members = searchedMembers.ToArray();

            if (findAgreedWithEula)
                members = members.Where(x => x.AgreedEula).Select(x => x).ToArray();

            if (members.Length == 0)
            {
                if (sendMessage)
                    SendMessage(source,
                    [
                        new CqReplyMsg(source.MessageId),
                        GetMultiSearchResultInformationString(userIdentificationString, behaviorInformation, groupId,
                            findAgreedWithEula)
                    ]);
                return false;
            }

            return true;
        }

        members = [];
        if (sendMessage)
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                GetMultiSearchResultInformationString(userIdentificationString, behaviorInformation, groupId,
                    findAgreedWithEula)
            ]);
        return false;
    }

    public MemberAlias GetAliasById(long id, long groupId)
    {
        using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;

        var aliases = db.MemberAliasesDataSet.Include(x => x.Aliases)
            .FirstOrDefault(x => x.GroupId == groupId && x.QqId == id);

        return aliases;
    }
}