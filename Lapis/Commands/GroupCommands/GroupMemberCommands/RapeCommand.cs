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
            $"您被 {nickname} ({memberId}) 狠狠地操了一顿"
        ]);

        return true;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        var regex = new Regex("^(被(群友)?(.*)透|被(.*)日(批)?|被(.*)操|被(.*)干)$");

        var match = regex.Match(command);
        
        var validGroups = match.Groups
            .Cast<Group>()
            .Skip(1)
            .Where(g => g.Success)
            .Select(g => g.Value).ToArray();

        var targetedMemberName = validGroups[^1] == "批" ? validGroups[^2] : validGroups[^1];

        targetedMemberName = targetedMemberName.Trim();

        if (!targetedMemberName.Equals(string.Empty))
            ParseWithArgument([targetedMemberName], source);
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
            $"您把 {nickname} ({memberId}) 狠狠地操了一顿"
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
            ParseWithArgument([regex.Replace(command, "", 1)], source);
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
        if
            (long.TryParse(arguments[0], out var id) && id == BotConfiguration.Instance.BotQqNumber)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId), BotReply
            ]);
            return;
        }

        var memberFound = GroupMemberCommandInstance.TryGetMember(arguments[0], source.GroupId, out var members);

        if (!memberFound)
        {
            SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    GetMultiSearchResultInformationString(arguments[0], CommandString, FunctionString, source.GroupId)
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