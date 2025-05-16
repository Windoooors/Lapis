using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LapisBot.GroupCommands.StickerCommands;

namespace LapisBot.GroupCommands;

public abstract class StickerCommandBase : GroupCommand
{
    public static StickerCommand StickerCommandInstance;
}

public class StickerCommand : StickerCommandBase
{
    public StickerCommand()
    {
        CommandHead = new Regex("^sticker");
        SubCommands =
        [
            new FortuneCommand(),
            new ObituaryCommand()
        ];
        StickerCommandInstance = this;
    }

    public override void Initialize()
    {
        foreach (var command in SubCommands) command.Initialize();
    }

    public override void Unload()
    {
        foreach (var command in SubCommands) command.Unload();
    }
}