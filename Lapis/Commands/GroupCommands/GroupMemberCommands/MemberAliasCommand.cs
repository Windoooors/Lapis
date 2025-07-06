using System.Collections.Generic;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.GroupMemberCommands.MemberAliasCommands;
using Lapis.Miscellaneous;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands;

public abstract class MemberAliasCommandBase : GroupMemberCommandBase
{
    public static MemberAliasCommand MemberAliasCommandInstance;
}

public class MemberAliasCommand : MemberAliasCommandBase
{
    public MemberAliasCommand()
    {
        SubCommands = [new MemberAliasAddCommand()];
        CommandHead = "alias";
        DirectCommandHead = "malias|群友别名|查群友别名";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("malias", "1");
        MemberAliasCommandInstance = this;
    }

    public override void Parse(CqGroupMessagePostContext source, long[] mentionedUserIds)
    {
        ParseWithArgument("", source, mentionedUserIds);
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source,
        long[] mentionedUserIds)
    {
        command = command.Trim();

        var memberFound =
            GroupMemberCommandInstance.TryGetMember(command == "" ? mentionedUserIds[0].ToString() : command,
                source.GroupId, out var members);
        if (!memberFound)
        {
            var message =
                GetMultiSearchResultInformationString(command, "alias", "别名", source.GroupId);

            SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    message
                ]
            );
        }

        if (members.Length == 1)
        {
            SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    GetAliasesInText(GroupMemberCommandInstance.GetAliasById(members[0].Id, source.GroupId),
                        source.GroupId)
                ]
            );
            return;
        }

        SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                GetMultiAliasesMatchedInformationString(members, "alias", "别名", source.GroupId)
            ]
        );
    }

    private string GetAliasesInText(Alias alias, long groupId)
    {
        if (!TryGetNickname(alias.Id, groupId, out var nickname)) return "该群友已退群！";
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"群友 {nickname} 有如下别称：");
        if (alias.Aliases.Count != 0)
        {
            var hashSet = new HashSet<string>(alias.Aliases);

            foreach (var aliasString in hashSet) stringBuilder.AppendLine(aliasString);

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }
        else
        {
            stringBuilder = new StringBuilder($"群友 {nickname} 没有别称");
        }

        return stringBuilder.ToString();
    }
}