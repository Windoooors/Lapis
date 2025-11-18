using System;
using System.Linq;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands;

public class BeingRapedCommand : RapeCommandBase
{
    public BeingRapedCommand()
    {
        EulaSettingsIdentifierPair = new SettingsIdentifierPair("being_raped", "2");

        CommandHead = "被(群友)?(透|日(批)?|操|干)";
        DirectCommandHead = "被(群友)?(透|日(批)?|操|干)";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("being_raped", "1");
        IntendedArgumentCount = 1;
        BotReply = "😈";
        CommandString = "被透";
        FunctionString = "被群友透";
    }

    protected override bool SendRapeMessage(GroupMemberCommand.GroupMember member, CqGroupMessagePostContext source)
    {
        if (!TryGetNickname(member.Id, source.GroupId, out var nickname)) return false;

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + ApiOperator.Instance.UrlToImage(GetQqAvatarUrl(member.Id)).ToBase64()),
            $"群友 {nickname} ({member.Id}) 透到你啦(*¯︶¯*)"
        ]);

        return true;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("lite_command", "1"), source.GroupId))
            return;

        if (!SettingsPool.GetValue(new SettingsIdentifierPair("lite_command", "1"), source.GroupId))
            return;

        var originalCommandString = command;

        var regex = new Regex("^(被(群友)?(.*)(透|日(批)?|操|干))$");

        if (!regex.IsMatch(command))
            return;

        var match = regex.Match(command);

        var targetedMemberName = match.Groups[3].ToString();

        targetedMemberName = targetedMemberName.Trim();

        if (!targetedMemberName.Equals(string.Empty))
            ProcessRapeWithArguments([targetedMemberName], originalCommandString, source, false);
    }
}

public class RapeCommand : RapeCommandBase
{
    public RapeCommand()
    {
        EulaSettingsIdentifierPair = new SettingsIdentifierPair("rape", "2");

        CommandHead = "透(群友)?|日(批)?|操|干";
        DirectCommandHead = "透(群友)?|日(批)?|操|干";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("rape", "1");
        IntendedArgumentCount = 1;
        BotReply = "🥺";
        CommandString = "透";
        FunctionString = "透群友";
    }

    protected override bool SendRapeMessage(GroupMemberCommand.GroupMember member, CqGroupMessagePostContext source)
    {
        if (!TryGetNickname(member.Id, source.GroupId, out var nickname)) return false;

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + ApiOperator.Instance.UrlToImage(GetQqAvatarUrl(member.Id)).ToBase64()),
            $"群友 {nickname} ({member.Id}) 被你透啦(*¯︶¯*)"
        ]);

        member.RapedTimes++;

        return true;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("lite_command", "1"), source.GroupId))
            return;

        var originalCommandString = command;

        var regex = new Regex($"^({DirectCommandHead})");
        var regexWithEndingSpace = new Regex(@$"^({DirectCommandHead})\s");
        if (regex.IsMatch(command) && !regexWithEndingSpace.IsMatch(command) &&
            regex.Replace(command, "", 1).Trim() != "")
            ProcessRapeWithArguments([regex.Replace(command, "", 1)], originalCommandString, source, false);
    }
}

public abstract class RapeCommandBase : GroupMemberCommandBase
{
    protected string BotReply;
    protected string CommandString;

    protected SettingsIdentifierPair EulaSettingsIdentifierPair;
    protected string FunctionString;

    protected virtual bool SendRapeMessage(GroupMemberCommand.GroupMember member, CqGroupMessagePostContext source)
    {
        return false;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        ProcessRapeWithArguments(arguments, originalPlainMessage, source, true);
    }

    private bool MemberAgreedToUse(CqGroupMessagePostContext source)
    {
        if (!GroupMemberCommandInstance.TryGetMember(source.Sender.UserId.ToString(),
                out var memberInvokingCommand, source))
            return false;

        if (memberInvokingCommand[0].AgreedWithEula)
            return true;

        TaskHandleQueue.HandleableTask task = new(source.Sender.UserId, () =>
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("您已拒绝！")
            ]);
        }, () =>
        {
            GroupMemberCommandInstance.AgreeWithEula(source.Sender.UserId, source.GroupId);

            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("您已同意！")
            ]);
        });
        var success = TaskHandleQueue.Instance.AddTask(task, source.GroupId, source.Sender.UserId);

        if (success)
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(
                    "您需要同意才能使用群友互动功能" +
                    "\n 发送 \"lps handle confirm\" 以同意，或者发送 \"lps handle cancel\" 以拒绝")
            ]);
        else
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("您当前已有代办事项！请处理后再试！")
            ]);
        return false;
    }

    protected void ProcessRapeWithArguments(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source,
        bool sendNotFoundMessage)
    {
        if (SettingsPool.GetValue(EulaSettingsIdentifierPair, source.GroupId) && !MemberAgreedToUse(source))
            return;

        if
            (long.TryParse(arguments[0], out var id) && id == BotConfiguration.Instance.BotQqNumber)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId), BotReply
            ]);
            return;
        }

        var memberFound =
            GroupMemberCommandInstance.TryGetMember(arguments[0], out var members, source,
                new CommandBehaviorInformationDataObject(CommandString, FunctionString, null, false, true),
                sendNotFoundMessage,
                SettingsPool.GetValue(EulaSettingsIdentifierPair, source.GroupId));

        if (!memberFound)
            return;

        if (members.Length != 1)
        {
            SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    GetMultiAliasesMatchedInformationString(members,
                        new CommandBehaviorInformationDataObject(CommandString, FunctionString, null, false, true),
                        source.GroupId)
                ]
            );
            return;
        }

        if (members[0].Id == source.Sender.UserId)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId), "哇 还有水仙"
            ]);
            return;
        }

        if (!SendRapeMessage(members[0], source)) ParseWithArgument(arguments, originalPlainMessage, source);
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        if (SettingsPool.GetValue(EulaSettingsIdentifierPair, source.GroupId) && !MemberAgreedToUse(source))
            return;

        if (!GroupMemberCommandInstance.Groups.TryGetValue(new GroupMemberCommand.Group(source.GroupId),
                out var group) ||
            group.Members
                .Where(x => x.AgreedWithEula || !SettingsPool.GetValue(EulaSettingsIdentifierPair, source.GroupId))
                .Select(x => x).ToArray().Length <= 1)
        {
            MemberNotEnoughErrorHelp(source);
            return;
        }

        var memberArray = group.Members
            .Where(x => x.AgreedWithEula || !SettingsPool.GetValue(EulaSettingsIdentifierPair, source.GroupId))
            .Select(x => x).ToArray();

        var i = new Random().Next(0, memberArray.Length);

        while (memberArray[i].Id == source.Sender.UserId)
            i = new Random().Next(0, memberArray.Length);

        if (!SendRapeMessage(memberArray[i], source))
            Parse(originalPlainMessage, source);
    }
}