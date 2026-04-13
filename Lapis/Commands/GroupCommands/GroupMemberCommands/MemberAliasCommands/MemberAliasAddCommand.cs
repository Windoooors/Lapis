using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands.MemberAliasCommands;

public class MemberAliasAddCommand : MemberAliasCommandBase
{
    public MemberAliasAddCommand()
    {
        CommandHead = "add";
        DirectCommandHead = "添加群友别名";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("malias_add", "1");
        IntendedArgumentCount = 2;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (arguments.Length < IntendedArgumentCount)
        {
            HelpCommand.Instance.ArgumentErrorHelp(source);
            return;
        }

        GroupMember[] members = [];
        long intendedUserId = 0;

        if (!GroupMemberCommandInstance.TryGetMember(arguments[0], out members, source,
                new CommandBehaviorInformationDataObject("malias add", "添加别名", [arguments[1]], true),
                true)
           )
            return;

        if (members.Length > 1)
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(
                    GroupMemberCommandInstance.GetMultiAliasesMatchedInformationString(members,
                        new CommandBehaviorInformationDataObject("malias add", "添加别名", [arguments[1]], true),
                        source.GroupId))
            ]);

            return;
        }

        if (members.Length == 1)
            intendedUserId = members[0].QqId;

        if (arguments[1] == "")
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                "未指定别名！"
            ]);
            return;
        }

        var intendedAlias = arguments[1];

        var action = () =>
        {
            var id = intendedUserId;

            using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;

            var success =
                DatabaseHandler.Instance.GroupMemberDatabaseOperator.AddAlias(intendedAlias, source.GroupId, id, db);
            if (success)
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("添加成功！")
                ]);
            else
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("已存在此别名")
                ]);
        };

        TaskHandleQueue.HandleableTask task = new(intendedUserId, () =>
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("别名添加已取消！")
            ]);
        }, action);

        var success = TaskHandleQueue.Instance.AddTask(task, source.GroupId, intendedUserId);

        if (!TryGetNickname(intendedUserId, source.GroupId, out var nickname))
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("该群友已退群")
            ]);
            return;
        }

        if (success)
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(
                    $"您正在尝试为群友 \"{nickname}\" 添加别名 \"{intendedAlias}\""
                    + $"\n需要群友 \"{nickname}\" 发送 \"lps handle confirm\" 以确认，或者发送 \"lps handle cancel\" 以取消")
            ]);
        else
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("该群友当前已有代办事项！请待其处理后再试！")
            ]);
    }
}