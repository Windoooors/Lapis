using System.Linq;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands;

public class RapeRankCommand : GroupMemberCommandBase
{
    public RapeRankCommand()
    {
        CommandHead = "rrank";
        DirectCommandHead = "rrank|透群友排行榜";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("rape_rank", "1");
        IntendedArgumentCount = 1;
    }

    public override void Initialize()
    {
        Program.WeekChanged += (_, _) =>
        {
            foreach (var group in GroupMemberCommandInstance.Groups)
            foreach (var member in group.Members)
                member.RapedTimes = 0;
        };
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        if (!GroupMemberCommandInstance.Groups.TryGetValue(new GroupMemberCommand.Group(source.GroupId), out var group))
            return;

        var memberList = group.Members.ToList();

        memberList.Sort((x, y) => x.RapedTimes > y.RapedTimes ? -1 : 1);

        var stringBuilder = new StringBuilder("本周排行：\n");

        var ranked = false;

        for (var i = 0;
             i < memberList.Count && memberList[i].RapedTimes != 0 &&
             TryGetNickname(memberList[i].Id, source.GroupId, out var nickname);
             i++)
        {
            stringBuilder.AppendLine($"{i + 1}.群友 {nickname} ({memberList[i].Id}) 被透 {
                memberList[i].RapedTimes
            } 次");

            ranked = true;
        }

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            ranked ? new CqTextMsg(stringBuilder.ToString().Trim()) : new CqTextMsg("暂时没有人北朝哈！")
        ]);
    }
}