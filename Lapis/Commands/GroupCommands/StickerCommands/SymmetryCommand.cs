using System.IO;
using System.Linq;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Operations.ApiOperation;
using Lapis.Operations.ImageOperation;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.StickerCommands;

public class SymmetryCommand : StickerCommandBase
{
    public SymmetryCommand()
    {
        CommandHead = "symmetry|sym|对称";
        DirectCommandHead = "对称";

        IntendedArgumentCount = 1;

        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("symmetry", "1");
    }

    private bool _processing = false;

    private const int MaxPictureTotalPixelCount = 5120 * 2560;

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        ParseWithArgument(["左"], originalPlainMessage, source);
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        switch (command)
        {
            case "左对称":
                ParseWithArgument(["左"], command, source);
                break;
            case "右对称":
                ParseWithArgument(["右"], command, source);
                break;
            case "上对称":
                ParseWithArgument(["上"], command, source);
                break;
            case "下对称":
                ParseWithArgument(["下"], command, source);
                break;
        }
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (_processing)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "当前有图片正在处理中，请稍后再试"]);
            
            return;
        }
        
        _processing = true;
        
        var symmetryType = SymmetryType.None;

        if (arguments.Length >= 1)
        {
            symmetryType = arguments[0] switch
            {
                "左" => SymmetryType.Left,
                "右" => SymmetryType.Right,
                "上" => SymmetryType.Up,
                "下" => SymmetryType.Down,
                _ => SymmetryType.None
            };

            if (symmetryType == SymmetryType.None)
            {
                SendMessage(source, [new CqReplyMsg(source.MessageId), "请发送图片翻转参数"]);
                _processing = false;
                return;
            }
        }

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
        
        using var resultImage = image.Clone();

        switch (symmetryType)
        {
            case SymmetryType.Left:
            {
                image.Crop(image.Width / 2, image.Height);

                image.FlipHorizontally();

                resultImage.DrawImage(image, image.Width, 0, CompositeOperator.Src);

                break;
            }
            case SymmetryType.Right:
            {
                image.Crop(image.Width / 2, image.Height, image.Width / 2, 0);

                image.FlipHorizontally();

                resultImage.DrawImage(image, 0, 0, CompositeOperator.Src);

                break;
            }
            case SymmetryType.Up:
            {
                image.Crop(image.Width, image.Height / 2);

                image.FlipVertically();

                resultImage.DrawImage(image, 0, image.Height, CompositeOperator.Src);

                break;
            }
            case SymmetryType.Down:
            {
                image.Crop(image.Width, image.Height / 2, 0, image.Height / 2);

                image.FlipVertically();

                resultImage.DrawImage(image, 0, 0, CompositeOperator.Src);

                break;
            }
        }
        
        image.Dispose();

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqImageMsg("base64://" + resultImage.ToBase64(false, true))
        ]);
        
        _processing = false;
    }

    private enum SymmetryType
    {
        Left,
        Right,
        Up,
        Down,
        None
    }
}