using System;
using System.Linq;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands;

public class RapeCommand : GroupMemberCommandBase
{
    public RapeCommand()
    {
        CommandHead = new Regex("^透|^日|^操|^干|^日批");
        DirectCommandHead = new Regex("^透|^日|^操|^干|^日批");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("rape", "1");
    }

    private bool SendMessage(long memberId, CqGroupMessagePostContext source)
    {
        var memberInformation = Program.Session.GetGroupMemberInformation(source.GroupId, memberId);

        if (memberInformation == null || memberInformation.Status == CqActionStatus.Failed)
        {
            GroupMemberCommandInstance.RemoveMember(memberId, source.GroupId);
            return false;
        }

        var nickname = memberInformation.GroupNickname == ""
            ? memberInformation.Nickname
            : memberInformation.GroupNickname;

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + ApiOperator.Instance.UrlToImage(GetQqAvatarUrl(memberId)).ToBase64()),
            $"您把 {nickname} ({memberId}) 狠狠地操了一顿"
        ]);

        return true;
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        var id = long.Parse(command);

        if (id == source.Sender.UserId)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId), "哇 还有水仙"
            ]);
            return;
        }

        if (id == BotConfiguration.Instance.BotQqNumber)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId), "🥺"
            ]);
            return;
        }

        if (!GroupMemberCommandInstance.Groups.TryGetValue(new GroupMemberCommand.Group(source.GroupId),
                out var group))
            return;

        if (group.Members.Contains(new GroupMemberCommand.GroupMember(id)))
        {
            if (!SendMessage(id, source)) ParseWithArgument(command, source);
        }
        else
        {
            MemberNotHaveChatErrorHelp(source);
        }
    }

    public override void Parse(CqGroupMessagePostContext source)
    {
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
            Parse(source);
    }
}