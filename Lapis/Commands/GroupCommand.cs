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

    protected void SendMessage(long groupId, CqMessage message, bool sendForwardedHistory= false)
    {
        if (!(!SettingsPool.GetValue(new SettingsIdentifierPair("mute", "1"), groupId) || this is SettingsCommand))
            return;

        if (message[0] is CqReplyMsg replyMessage)
            TooLongDontReadCommand.Instance.ExcludeMessage(replyMessage.Id, groupId);

        if (sendForwardedHistory)
        {
            var msg = new CqForwardMessage
            {
                Capacity = 1
            };
            msg.Add(new CqForwardMessageNode(BotConfiguration.Instance.BotName, BotConfiguration.Instance.BotQqNumber,
                message));

            Program.Session.SendGroupForwardMessage(groupId, msg);
        }
        else
        {
            Program.Session.SendGroupMessage(groupId, message);
        }
    }
}