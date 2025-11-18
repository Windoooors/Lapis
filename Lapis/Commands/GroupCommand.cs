using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands;
using Lapis.Commands.UniversalCommands;
using Lapis.Settings;

namespace Lapis.Commands;

public class GroupCommand : Command
{
    public virtual void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(originalPlainMessage, source);
    }

    public virtual void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(originalPlainMessage, source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
    }

    protected void SendMessage(long groupId, CqMessage message)
    {
        if (!(!SettingsPool.GetValue(new SettingsIdentifierPair("mute", "1"), groupId) || this is SettingsCommand))
            return;

        if (message[0] is CqReplyMsg replyMessage)
            TooLongDontReadCommand.Instance.ExcludeMessage(replyMessage.Id, groupId);
        Program.Session.SendGroupMessage(groupId, message);
    }
}