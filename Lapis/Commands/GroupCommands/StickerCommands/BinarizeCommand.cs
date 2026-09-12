using System.Linq;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.StickerCommands;

public class BinarizeCommand : StickerCommandBase
{
    private const int MaxPictureTotalPixelCount = 5120 * 5120;

    private bool _processing;

    public BinarizeCommand()
    {
        CommandHead = "bin|binarize|二值化";
        DirectCommandHead = "二值化";

        IntendedArgumentCount = 1;

        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("binarize", "1");
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        if (_processing)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "当前有图片正在处理中，请稍后再试"]);

            return;
        }

        _processing = true;

        var message = source.Message.FirstOrDefault(x => x is CqReplyMsg
        {
            Id: not null
        });

        if (message is not CqReplyMsg replyMessage)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "请回复包含图片内容的消息"]);
            _processing = false;
            return;
        }

        var repliedMessage = Program.Session.GetMessage(replyMessage.Id ?? 0)?.Message;

        if (repliedMessage?.FirstOrDefault(x => x is CqImageMsg) is not CqImageMsg imageMessage)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "请回复包含图片内容的消息"]);
            _processing = false;
            return;
        }

        var imageDownloaded = ApiOperator.Instance.TryUrlToImage(imageMessage.Url?.ToString() ?? "",
            out var image, MaxPictureTotalPixelCount);

        if (!imageDownloaded)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "图片尺寸过大"]);
            _processing = false;
            return;
        }
        
        image.Binarize();

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + image.ToBase64(false, true, false))
        ]);
        
        image.Dispose();

        _processing = false;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        switch (command)
        {
            case "乔打架化":
                Parse(command, source);
                break;
        }
    }
}