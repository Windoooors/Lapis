using System;
using System.Collections.Generic;
using System.IO;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands;

public class TooLongDontReadCommand : GroupCommand
{
    public static TooLongDontReadCommand Instance;

    private readonly List<(long groupId, List<long> blockedIds)> _blockList =
        File.Exists(Path.Combine(AppContext.BaseDirectory, "data/tldr_block_list.json"))
            ? JsonConvert.DeserializeObject<List<(long groupId, List<long> blockedIds)>>(
                File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data/tldr_block_list.json")))
            : [];

    private readonly List<long> _excludedMessageIds =
        File.Exists(Path.Combine(AppContext.BaseDirectory, "data/tldr_exclusion_list.json"))
            ? JsonConvert.DeserializeObject<List<long>>(
                File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data/tldr_exclusion_list.json")))
            : [];

    public TooLongDontReadCommand()
    {
        CommandHead = "tldr";
        DirectCommandHead = "tldr|总结聊天记录";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("tldr", "1");
        IntendedArgumentCount = 2;
        Instance = this;
    }

    public override void Initialize()
    {
        Program.DateChanged += (_, _) => { Unload(); };
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (string.Equals(arguments[0], "block", StringComparison.InvariantCultureIgnoreCase))
        {
        }
    }

    public void StartSummarizing(CqGroupMessagePostContext source)
    {
        SendMessage(source, [new CqReplyMsg(source.MessageId), "请稍等！正在处理中"]);

        /*if (_blockList.Find(x => x.groupId == source.GroupId).blockedIds?.Contains(source.Sender.UserId) ?? false)
        {
            CqMessage messageToBeExcluded;
            if (source.Message.Find(x => x is CqReplyMsg) is CqReplyMsg replyMessage)
            {
                var getMessageSession = Program.Session.GetMessage(replyMessage.Id ?? 0);
                if ((getMessageSession?.Status ?? CqActionStatus.Failed) == CqActionStatus.Okay)
                    messageToBeExcluded = getMessageSession?.Message;
            }
        }*/
    }

    public void ExcludeMessage(long? messageId, long groupId)
    {
        if (!SettingsPool.GetValue(ActivationSettingsSettingsIdentifier, groupId))
            return;

        if (messageId is not null)
            _excludedMessageIds.Add(messageId.Value);
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(ActivationSettingsSettingsIdentifier, source.GroupId))
            return;

        var directoryPath = Path.Combine(AppContext.BaseDirectory, "data/tldr");
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var path = Path.Combine(AppContext.BaseDirectory, $"data/tldr/{source.GroupId}.json");

        var messageChain = source.Message;

        long repliedMessageId = 0;
        var atIds = new List<long>();
        var plainMessage = "";
        var hasPicture = false;

        foreach (var message in messageChain)
            switch (message)
            {
                case CqReplyMsg replyMessage:
                    repliedMessageId = replyMessage.Id ?? 0;
                    break;
                case CqAtMsg atMessage:
                    if (!atMessage.IsAtAll)
                        atIds.Add(atMessage.Target);
                    break;
                case CqTextMsg textMessage:
                    plainMessage = textMessage.Text;
                    break;
                case CqImageMsg imageMessage:
                    hasPicture = true;
                    break;
            }

        var messageItem = new MessageItem(source.UserId, plainMessage, source.MessageId, repliedMessageId,
            atIds.ToArray(), hasPicture);

        File.AppendAllLines(path, [JsonConvert.SerializeObject(messageItem)]);
    }

    public override void Unload()
    {
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/tldr_block_list.json"),
            JsonConvert.SerializeObject(_blockList));
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/tldr_exclusion_list.json"),
            JsonConvert.SerializeObject(_excludedMessageIds));
        Program.Logger.LogInformation("Too Long Don't Read data have been saved.");
    }

    private class MessageItem(
        long senderId,
        string plainMessage,
        long messageId,
        long repliedMessageId = 0,
        long[] atIds = null,
        bool hasPicture = false)
    {
        public long[] AtIds = atIds ?? [];
        public bool HasPicture = hasPicture;
        public long MessageId = messageId;
        public string MessageInPlainText = plainMessage;
        public long RepliedMessageId = repliedMessageId;
        public long SenderId = senderId;
        public DateTime TimeStamp = DateTime.Now;
    }
}