using System.Linq;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;
using Microsoft.EntityFrameworkCore;

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
            using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;

            db.GroupMembersDataSet.ForEachAsync(x =>
                x.RapedTimes = 0);

            db.SaveChanges();
        };
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;

        var groupMembers =
            db.GroupMembersDataSet.Where(x =>
                x.GroupId == source.GroupId).ToArray();

        var memberList = groupMembers.ToList();

        memberList.Sort((x, y) => x.RapedTimes > y.RapedTimes ? -1 : 1);

        var stringBuilder = new StringBuilder("本周排行：\n");

        var ranked = false;

        var lines = 0;

        for (var i = 0;
             i < memberList.Count && memberList[i].RapedTimes != 0 &&
             TryGetNickname(memberList[i].QqId, source.GroupId, out var nickname);
             i++)
        {
            stringBuilder.AppendLine($"{i + 1}.群友 {nickname} ({memberList[i].QqId}) 被透 {
                memberList[i].RapedTimes
            } 次");

            lines++;

            ranked = true;
        }

        var sendForwardMsg = lines >= 10;

        SendMessage(source.GroupId,
        [
            new CqReplyMsg(source.MessageId),
            ranked ? new CqTextMsg(stringBuilder.ToString().Trim()) : new CqTextMsg("暂时没有人北朝哈！")
        ], sendForwardMsg);
    }
}