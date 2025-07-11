using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands;

public abstract class Command
{
    public string CommandHead;
    public string DirectCommandHead;
    public Command[] SubCommands = [];
    public SettingsIdentifierPair ActivationSettingsSettingsIdentifier { get; protected init; } = new();
    public int IntendedArgumentCount { get; protected init; }

    public virtual void Unload()
    {
    }

    public void StartUnloading()
    {
        foreach (var command in SubCommands) command.StartUnloading();

        Unload();
    }

    public void StartInitializing()
    {
        Initialize();

        foreach (var command in SubCommands) command.StartInitializing();
    }

    public virtual void Initialize()
    {
    }

    protected static void SendMessage(CqMessagePostContext source, CqMessage message)
    {
        if (source is CqGroupMessagePostContext groupSource)
            Program.Session.SendGroupMessage(groupSource.GroupId, message);
        if (source is CqPrivateMessagePostContext privateSource)
            Program.Session.SendPrivateMessage(privateSource.Sender.UserId, message);
    }
}