using System;
using System.Linq;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands;

public class RapeCommand : GroupMemberCommandBase
{
    public RapeCommand()
    {
        CommandHead = "透|日|操|干|日批";
        DirectCommandHead = "透|日|操|干|日批";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("rape", "1");
    }

    private bool SendMessage(long memberId, CqGroupMessagePostContext source)
    {
        if (!TryGetNickname(memberId, source.GroupId, out var nickname)) return false;

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + ApiOperator.Instance.UrlToImage(GetQqAvatarUrl(memberId)).ToBase64()),
            $"您把 {nickname} ({memberId}) 狠狠地操了一顿"
        ]);

        return true;
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source,
        long[] mentionedUserIds)
    {
        GroupMemberCommand.GroupMember[] members = [];

        if ((mentionedUserIds.Length == 1 && mentionedUserIds[0] == BotConfiguration.Instance.BotQqNumber) ||
            (long.TryParse(command, out var id) && id == BotConfiguration.Instance.BotQqNumber))
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId), "🥺"
            ]);
            return;
        }

        if (mentionedUserIds.Length == 1)
        {
            members = [new GroupMemberCommand.GroupMember(mentionedUserIds[0])];
        }
        else
        {
            var memberFound = GroupMemberCommandInstance.TryGetMember(command, source.GroupId, out members);

            if (!memberFound)
            {
                SendMessage(source,
                    [
                        new CqReplyMsg(source.MessageId),
                        GetMultiSearchResultInformationString(command, "日", "逼", source.GroupId)
                    ]
                );
                return;
            }

            if (members.Length != 1)
            {
                SendMessage(source,
                    [
                        new CqReplyMsg(source.MessageId),
                        GetMultiAliasesMatchedInformationString(members, "日", "逼", source.GroupId)
                    ]
                );
                return;
            }
        }

        if (members[0].Id == source.Sender.UserId)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId), "哇 还有水仙"
            ]);
            return;
        }

        if (!SendMessage(members[0].Id, source)) ParseWithArgument(command, source, mentionedUserIds);
    }

    public override void Parse(CqGroupMessagePostContext source, long[] mentionedUserIds)
    {
        if (mentionedUserIds.Length == 1)
        {
            ParseWithArgument("", source, mentionedUserIds);
            return;
        }

        if (!GroupMemberCommandInstance.Groups.TryGetValue(new GroupMemberCommand.Group(source.GroupId),
                out var group) || group.Members.Count <= 1)
        {
            MemberNotEnoughErrorHelp(source);
            return;
        }

        var memberArray = group.Members.ToArray();

        var i = new Random().Next(0, group.Members.Count);

        while (memberArray[i].Id == source.Sender.UserId)
            i = new Random().Next(0, group.Members.Count);

        var memberId = memberArray[i].Id;

        if (!SendMessage(memberId, source))
            Parse(source, mentionedUserIds);
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source,
        long[] mentionedUserIds)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        var regex = new Regex($"^({DirectCommandHead})");
        var regexWithEndingSpace = new Regex(@$"^({DirectCommandHead})\s");
        if (regex.IsMatch(command) && !regexWithEndingSpace.IsMatch(command) &&
            regex.Replace(command, "", 1).Trim() != "")
            ParseWithArgument(regex.Replace(command, "", 1), source, mentionedUserIds);
    }
}