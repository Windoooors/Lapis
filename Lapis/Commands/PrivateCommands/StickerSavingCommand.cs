using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace Lapis.Commands.PrivateCommands;

public class StickerSavingCommand : PrivateCommand
{
    public override void RespondWithoutParsingCommand(string command, CqPrivateMessagePostContext source)
    {
        foreach (var item in source.Message)
            if (item is CqImageMsg imageMsg)
                Program.Session.SendPrivateMessage(source.Sender.UserId, [
                    new CqReplyMsg(source.MessageId),
                    imageMsg.Url != null ? imageMsg.Url.ToString() : "获取图像时出现错误，请重试"
                ]);
    }
}