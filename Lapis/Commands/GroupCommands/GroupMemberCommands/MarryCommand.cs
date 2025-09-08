using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands;

public class MarryCommand : GroupMemberCommandBase
{
    private SettingsIdentifierPair _eulaSettingsIdentifierPair = new SettingsIdentifierPair("marry", "2");
    
    public MarryCommand()
    {
        CommandHead = "娶(群友)?|嫁";
        DirectCommandHead = "娶(群友)?|嫁";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("marry", "1");
    }

    private bool MemberAgreedToUse(CqGroupMessagePostContext source)
    {
        if (!GroupMemberCommandInstance.TryGetMember(source.Sender.UserId.ToString(), source.GroupId,
                out var memberInvokingCommand))
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
                    "您需要同意才能使用娶群友功能（您同意后，其他同意使用该功能的群友也能娶到您。如果您是本群第一个同意该功能的，您需要等待下一位群友同意使用该功能）" +
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

    public override void Initialize()
    {
        Program.DateChanged += Refresh;
    }

    private void Refresh(object sender, EventArgs e)
    {
        CouplesOperator.Refresh();
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        if (SettingsPool.GetValue(_eulaSettingsIdentifierPair, source.GroupId) && !MemberAgreedToUse(source))
            return;

        var couple = CouplesOperator.GetCouple(source.Sender.UserId, source.GroupId);
        if (couple == null)
        {
            if (!GroupMemberCommandInstance.Groups.TryGetValue(new GroupMemberCommand.Group(source.GroupId),
                    out var group) ||
                group.Members.Where(x => x.AgreedWithEula || !SettingsPool.GetValue(_eulaSettingsIdentifierPair, source.GroupId)).Select(x => x).ToArray().Length -
                CouplesOperator.GetCouplesInGroup(source.GroupId).Length * 2 <= 1)
            {
                MemberNotEnoughErrorHelp(source);
                return;
            }

            var memberArray = group.Members.Where(x => x.AgreedWithEula || !SettingsPool.GetValue(_eulaSettingsIdentifierPair, source.GroupId)).Select(x => x).ToArray();

            var i = new Random().Next(0, memberArray.Length);

            while (memberArray[i].Id == source.Sender.UserId ||
                   CouplesOperator.IsMarried(memberArray[i].Id, source.GroupId))
                i = new Random().Next(0, memberArray.Length);

            var memberId = memberArray[i].Id;
            CouplesOperator.AddCouple(source.Sender.UserId, memberId, source.GroupId);

            Parse(originalPlainMessage, source);
            return;
        }

        if (!SendMessage(couple.BrideId, source))
            Parse(originalPlainMessage, source);
    }

    private bool SendMessage(long memberId, CqGroupMessagePostContext source)
    {
        if (!TryGetNickname(memberId, source.GroupId, out var nickname))
        {
            CouplesOperator.RemoveCouple(memberId, source.GroupId);
            return false;
        }

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + ApiOperator.Instance.UrlToImage(GetQqAvatarUrl(memberId)).ToBase64()),
            $"您今天的对象是 {nickname} ({memberId}) ！"
        ]);

        return true;
    }

    public override void Unload()
    {
        CouplesOperator.Save();
        Program.Logger.LogInformation("Couple data have been saved");
    }

    public static class CouplesOperator
    {
        private static readonly HashSet<CouplesInGroup> CouplesInGroupSet =
            File.Exists(Path.Combine(AppContext.BaseDirectory, "data/couples.json"))
                ? JsonConvert.DeserializeObject<HashSet<CouplesInGroup>>(File.ReadAllText(Path.Combine(
                    AppContext.BaseDirectory,
                    "data/couples.json")))
                : [];

        public static Couple GetCouple(long id, long groupId)
        {
            if (!CouplesInGroupSet.TryGetValue(new CouplesInGroup(groupId), out var couplesInGroup))
                return null;

            var couples = couplesInGroup.Couples;

            foreach (var couple in couples)
            {
                if (couple.GroomId == id)
                    return couple;
                if (couple.BrideId == id)
                    return new Couple(id, couple.GroomId);
            }

            return null;
        }

        public static Couple[] GetCouplesInGroup(long groupId)
        {
            if (!CouplesInGroupSet.TryGetValue(new CouplesInGroup(groupId), out var couplesInGroup))
                return [];
            return couplesInGroup.Couples.ToArray();
        }

        public static void AddCouple(long groomId, long brideId, long groupId)
        {
            if (!CouplesInGroupSet.TryGetValue(new CouplesInGroup(groupId), out var couplesInGroup))
            {
                CouplesInGroupSet.Add(new CouplesInGroup(groupId));
                AddCouple(groomId, brideId, groupId);
                return;
            }

            var couples = couplesInGroup.Couples;

            couples.Add(new Couple(groomId, brideId));
        }

        public static bool RemoveCouple(long id, long groupId)
        {
            if (!CouplesInGroupSet.TryGetValue(new CouplesInGroup(groupId), out var couplesInGroup))
                return false;

            var couples = couplesInGroup.Couples;

            foreach (var couple in couples)
                if (couple.GroomId == id || couple.BrideId == id)
                    return couples.Remove(couple);

            return false;
        }

        public static bool IsMarried(long id, long groupId)
        {
            if (!CouplesInGroupSet.TryGetValue(new CouplesInGroup(groupId), out var couplesInGroup))
                return false;

            var couples = couplesInGroup.Couples;

            foreach (var couple in couples)
                if (couple.GroomId == id || couple.BrideId == id)
                    return true;
            return false;
        }

        public static void Save()
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/couples.json"),
                JsonConvert.SerializeObject(CouplesInGroupSet));
        }

        public static void Refresh()
        {
            CouplesInGroupSet.Clear();
        }

        public class Couple(long groomId, long brideId)
        {
            public long GroomId { get; } = groomId;
            public long BrideId { get; } = brideId;

            public override bool Equals(object obj)
            {
                return obj is Couple other && ((other.GroomId == GroomId && other.BrideId == BrideId) ||
                                               (other.GroomId == BrideId && other.BrideId == GroomId));
            }

            public override int GetHashCode()
            {
                var max = Math.Max(GroomId, BrideId);
                var min = Math.Min(GroomId, BrideId);

                return HashCode.Combine(min, max);
            }
        }

        private class CouplesInGroup(long groupId)
        {
            public readonly HashSet<Couple> Couples = [];
            public long GroupId { get; } = groupId;

            public override bool Equals(object obj)
            {
                return obj is CouplesInGroup other && other.GroupId == GroupId;
            }

            public override int GetHashCode()
            {
                return GroupId.GetHashCode();
            }
        }
    }
}