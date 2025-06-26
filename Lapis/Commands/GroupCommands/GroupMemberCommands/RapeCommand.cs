using System;
using System.Linq;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
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
        var parsed = long.TryParse(command, out var id);
        if (!parsed)
        {
            HelpCommand.Instance.ArgumentErrorHelp(source);
            return;
        }

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

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        switch (command) //私货
        {
            case "日小乌":
                ParseWithArgument("200509301", source);
                break;
            case "日色老师":
                ParseWithArgument("2794813909", source);
                break;
            case "日空老师":
                ParseWithArgument("3522656010", source);
                break;
            case "日全家不死老师":
            case "日草老师":
                ParseWithArgument("1792975423", source);
                break;
            case "日慧敏姐":
                ParseWithArgument("1784234439", source);
                break;
            case "日乐家君":
                ParseWithArgument("2575663823", source);
                break;
            case "日秋招老师":
                ParseWithArgument("1306717258", source);
                break;
            case "日保研老师":
            case "日笑老师":
                ParseWithArgument("1837582042", source);
                break;
            case "日Asagi":
            case "日 Asagi":
            case "日 asagi":
            case "日asagi":
                ParseWithArgument("2975985647", source);
                break;
            case "日烤学妹":
                ParseWithArgument("1684931081", source);
                break;
            case "日柴老师":
                ParseWithArgument("2039151191", source);
                break;
            case "日小礼":
            case "日群主":
                ParseWithArgument("1281502717", source);
                break;
        }
    }
}