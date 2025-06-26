using Lapis.Commands.GroupCommands.StickerCommands;

namespace Lapis.Commands.GroupCommands;

public abstract class StickerCommandBase : GroupCommand
{
    public static StickerCommand StickerCommandInstance;
}

public class StickerCommand : StickerCommandBase
{
    public StickerCommand()
    {
        CommandHead = "sticker";
        SubCommands =
        [
            new FortuneCommand(),
            new ObituaryCommand()
        ];
        StickerCommandInstance = this;
    }
}