using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.ImageGenerators;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class PlayCountTop50Command : MaiCommandBase
{
    public PlayCountTop50Command()
    {
        CommandHead = "pc50";
        DirectCommandHead = "pc50";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("pc50", "1");
        IntendedArgumentCount = 1;
    }

    private void Process(long id, CqGroupMessagePostContext source, bool useAvatar)
    {
        var success = MaiScoreOperator.TryGetPc50(id, out var bestDto);

        if (!success)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "未找到用户记录"]);
            return;
        }

        var imageGenerator = new BestImageGenerator();

        var compressed = SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);
        
        var image = imageGenerator.Generate(bestDto, source.Sender.UserId.ToString(), useAvatar, compressed);
        
        SendMessage(source, [new CqReplyMsg(source.MessageId), new CqImageMsg("base64://" + image)]);
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        Process(source.UserId, source, true);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage, CqGroupMessagePostContext source)
    {
        var isGroupMember =
            GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[0],
                out var groupMembers, source) && groupMembers.Length == 1;

        var isQqId = long.TryParse(arguments[0], out var qqId);

        if (isGroupMember && groupMembers.Length == 1)
        {
            Process(groupMembers[0].QqId, source, false);
            return;
        }
        if (isQqId)
        {
            Process(qqId, source, false);
            return;
        }
        
        SendMessage(source, [new CqReplyMsg(source.MessageId), "未找到用户记录"]);
    }
}