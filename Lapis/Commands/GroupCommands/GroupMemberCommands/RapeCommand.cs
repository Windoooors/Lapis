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
        CommandHead = "被(群友)?透|被日(批)?|被操|被干";
        DirectCommandHead = "被(群友)?透|被日(批)?|被操|被干";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("beingraped", "1");
        IntendedArgumentCount = 1;
        BotReply = "😈";
        CommandString = "被日";
        FunctionString = "屌";
    }

    protected override bool SendMessage(long memberId, CqGroupMessagePostContext source)
    {
        if (!TryGetNickname(memberId, source.GroupId, out var nickname)) return false;

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + ApiOperator.Instance.UrlToImage(GetQqAvatarUrl(memberId)).ToBase64()),
            $"群友 {nickname} ({memberId}) 透到你啦(*¯︶¯*)"
        ]);

        return true;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        var regex = new Regex("^(被(群友)?(.*)透|被(.*)日(批)?|被(.*)操|被(.*)干)$");

        if (!regex.IsMatch(command))
            return;

        var match = regex.Match(command);

        var validGroups = match.Groups
            .Cast<Group>()
            .Skip(1)
            .Where(g => g.Success)
            .Select(g => g.Value).ToArray();

        var targetedMemberName = validGroups[^1] == "批" ? validGroups[^2] : validGroups[^1];

        targetedMemberName = targetedMemberName.Trim();

        if (!targetedMemberName.Equals(string.Empty))
            ProcessRapeWithArguments([targetedMemberName], source, false);
    }
}

public class RapeCommand : RapeCommandBase
{
    public RapeCommand()
    {
        CommandHead = "透(群友)?|日(批)?|操|干";
        DirectCommandHead = "透(群友)?|日(批)?|操|干";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("rape", "1");
        IntendedArgumentCount = 1;
        BotReply = "🥺";
        CommandString = "日";
        FunctionString = "逼";
    }

    protected override bool SendMessage(long memberId, CqGroupMessagePostContext source)
    {
        if (!TryGetNickname(memberId, source.GroupId, out var nickname)) return false;

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + ApiOperator.Instance.UrlToImage(GetQqAvatarUrl(memberId)).ToBase64()),
            $"群友 {nickname} ({memberId}) 被你透啦(*¯︶¯*)"
        ]);

        return true;
    }


    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        var regex = new Regex($"^({DirectCommandHead})");
        var regexWithEndingSpace = new Regex(@$"^({DirectCommandHead})\s");
        if (regex.IsMatch(command) && !regexWithEndingSpace.IsMatch(command) &&
            regex.Replace(command, "", 1).Trim() != "")
            ProcessRapeWithArguments([regex.Replace(command, "", 1)], source, false);
    }
}

public abstract class RapeCommandBase : GroupMemberCommandBase
{
    protected string BotReply;
    protected string CommandString;
    protected string FunctionString;

    protected virtual bool SendMessage(long memberId, CqGroupMessagePostContext source)
    {
        return false;
    }

    public override void ParseWithArgument(string[] arguments, CqGroupMessagePostContext source)
    {
        ProcessRapeWithArguments(arguments, source, true);
    }

    private bool MemberAgreedToUse(CqGroupMessagePostContext source)
    {
        if (!GroupMemberCommandInstance.TryGetMember(source.Sender.UserId.ToString(), source.GroupId,
                out var memberInvokingCommand))
            return false;

        if (memberInvokingCommand[0].AgreedToUseRapeCommand)
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
            GroupMemberCommandInstance.AgreeToUseRapeCommand(source.Sender.UserId, source.GroupId);

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
                    "您需要同意才能使用透群友功能（您同意后，其他同意使用该功能的群友也能透到您。如果您是本群第一个同意该功能的，您需要等待下一位群友同意使用该功能）" +
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

    protected void ProcessRapeWithArguments(string[] arguments, CqGroupMessagePostContext source,
        bool sendNotFoundMessage)
    {
        if (!MemberAgreedToUse(source))
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
            GroupMemberCommandInstance.TryGetMember(arguments[0], source.GroupId, out var members, true);

        if (!memberFound)
        {
            if (sendNotFoundMessage)
                SendMessage(source,
                    [
                        new CqReplyMsg(source.MessageId),
                        GetMultiSearchResultInformationString(arguments[0], CommandString, FunctionString,
                            source.GroupId, true, false)
                    ]
                );
            return;
        }

        if (members.Length != 1)
        {
            SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    GetMultiAliasesMatchedInformationString(members, CommandString, FunctionString, source.GroupId)
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

        if (!SendMessage(members[0].Id, source)) ParseWithArgument(arguments, source);
    }

    public override void Parse(CqGroupMessagePostContext source)
    {
        if (!MemberAgreedToUse(source))
            return;

        if (!GroupMemberCommandInstance.Groups.TryGetValue(new GroupMemberCommand.Group(source.GroupId),
                out var group) ||
            group.Members.Where(x => x.AgreedToUseRapeCommand).Select(x => x).ToArray().Length <= 1)
        {
            MemberNotEnoughErrorHelp(source);
            return;
        }

        var memberArray = group.Members.Where(x => x.AgreedToUseRapeCommand).Select(x => x).ToArray();

        var i = new Random().Next(0, memberArray.Length);

        while (memberArray[i].Id == source.Sender.UserId)
            i = new Random().Next(0, memberArray.Length);

        var memberId = memberArray[i].Id;

        if (!SendMessage(memberId, source))
            Parse(source);
    }
}