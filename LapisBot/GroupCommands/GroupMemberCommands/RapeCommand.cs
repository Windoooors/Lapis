using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Operations.ApiOperation;
using LapisBot.Settings;

namespace LapisBot.GroupCommands.GroupMemberCommands;

public class RapeCommand : MemberCommandBase
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
            MemberCommandInstance.RemoveMember(memberId, source.GroupId);
            return false;
        }

        var nickname = memberInformation.GroupNickname == ""
            ? memberInformation.Nickname
            : memberInformation.GroupNickname;

        Program.Session.SendGroupMessage(source.GroupId,
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
            Program.Session.SendGroupMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId), "哇 还有水仙"
            ]);
            return;
        }

        if (id == BotConfiguration.Instance.BotQqNumber)
        {
            Program.Session.SendGroupMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId), "🥺"
            ]);
            return;
        }

        if (!MemberCommandInstance.Groups.TryGetValue(new MemberCommand.Group(source.GroupId),
                out var group))
            return;

        if (group.Members.Contains(new MemberCommand.GroupMember(id)))
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
        if (!MemberCommandInstance.Groups.TryGetValue(new MemberCommand.Group(source.GroupId),
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