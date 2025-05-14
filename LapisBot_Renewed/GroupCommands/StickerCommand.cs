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
        StickerCommandInstance = this;
    }

    public override Task Initialize()
    {
        SubCommands.Clear();
        SubCommands.Add(new FortuneCommand());
        SubCommands.Add(new ObituaryCommand());

        foreach (var stickerCommand in SubCommands) stickerCommand.Initialize();

        return Task.CompletedTask;
    }
}