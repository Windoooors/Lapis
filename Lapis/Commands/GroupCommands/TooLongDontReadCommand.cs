using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
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

    private bool _locked;

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

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        StartSummarizing(source, 300);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (string.Equals(arguments[0], "block", StringComparison.InvariantCultureIgnoreCase))
        {
        }

        if (uint.TryParse(arguments[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var messageCount))
        {
            StartSummarizing(source, messageCount);
            return;
        }

        HelpCommand.Instance.ArgumentErrorHelp(source);
    }

    private void StartSummarizing(CqGroupMessagePostContext source, uint messageCount)
    {
        if (_locked)
        {
            SendMessage(source,
                [new CqReplyMsg(source.MessageId), new CqTextMsg("当前已有请求正在处理中")]);

            return;
        }

        _locked = true;

        var directoryPath = Path.Combine(AppContext.BaseDirectory, "data/tldr");
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var path = Path.Combine(AppContext.BaseDirectory, $"data/tldr/{source.GroupId}.json");

        if (messageCount > 1000)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "预期的消息数量超出限制"]);
            _locked = false;
            return;
        }

        if (messageCount < 5)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "消息数量过少"]);
            _locked = false;
            return;
        }

        if (!File.Exists(path))
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "未找到消息记录"]);
            _locked = false;
            return;
        }

        SendMessage(source, [new CqReplyMsg(source.MessageId), "请稍等！正在处理中"]);
        var messageObjects = new List<MessageItem>();

        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var lineCount = 0;
        var position = fileStream.Length - 1;

        while (position >= 0 && lineCount < messageCount)
        {
            fileStream.Seek(position, SeekOrigin.Begin);
            if (fileStream.ReadByte() == '\n') lineCount++;

            position--;
        }

        if (position > 0 || lineCount == messageCount)
            fileStream.Seek(position + 2, SeekOrigin.Begin);
        else
            fileStream.Seek(0, SeekOrigin.Begin);

        using var sr = new StreamReader(fileStream, Encoding.UTF8);

        string line;

        while ((line = sr.ReadLine()) != null)
        {
            var obj = JsonConvert.DeserializeObject<MessageItem>(line);

            if (_excludedMessageIds.Contains(obj.MessageId))
                continue;

            messageObjects.Add(obj);
        }

        if (messageObjects.Count is 0 or 1)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "未找到消息记录"]);
            _locked = false;
            return;
        }

        var stringBuilder = new StringBuilder();

        var qqIdHashSet = new HashSet<long>();

        foreach (var messageObject in messageObjects)
        {
            qqIdHashSet.Add(messageObject.SenderId);

            foreach (var atId in messageObject.AtIds) qqIdHashSet.Add(atId);
        }

        var qqIdList = qqIdHashSet.ToList();

        messageObjects.Reverse();
        foreach (var messageObject in messageObjects)
        {
            stringBuilder.Append(
                $"本条消息ID: {messageObject.MessageId}: 用户 \"{qqIdList.IndexOf(messageObject.SenderId)}\" 在 {messageObject.TimeStamp.ToString(CultureInfo.InvariantCulture)}");

            if (messageObject.RepliedMessageId != 0)
                stringBuilder.Append($"回复了消息 ID 为 {messageObject.RepliedMessageId} 的消息，");

            if (messageObject.AtIds.Length != 0)
                stringBuilder.Append(messageObject.RepliedMessageId != 0 ? "并 @ 了" : "@ 了");

            for (var i = 0; i < messageObject.AtIds.Length; i++)
                stringBuilder.Append($" ID 为{
                    qqIdList.IndexOf(messageObject.AtIds[i])
                } 的用户 {(i == messageObject.AtIds.Length - 1 ? "" : "和")}");

            stringBuilder.Append($" 说 \"{messageObject.MessageInPlainText}\"");

            if (messageObject.HasPicture)
                stringBuilder.Append("，消息含有图片");

            stringBuilder.AppendLine();
        }

        var result = stringBuilder.ToString();

        var requestDto = new DeepSeekRequestDto(result);

        var response = ApiOperator.Instance.Post(BotConfiguration.Instance.DeepSeekUrl, "chat/completions", requestDto,
            new AuthenticationHeaderValue("Bearer", BotConfiguration.Instance.DeepSeekApiToken)
        );

        if (response.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException($"Unexpected status code: {response.StatusCode}", null,
                response.StatusCode);

        var responseDto = JsonConvert.DeserializeObject<DeepSeekResponseDto>(response.Result);

        var rawResponse = responseDto.ChoicesResult[0].Message.Content;

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId), new CqTextMsg(ReplaceIdsWithNames(rawResponse, qqIdList, source.GroupId))
        ]);

        _locked = false;
    }

    private string ReplaceIdsWithNames(string aiResponse, List<long> senderIds, long groupId)
    {
        var pattern = @"\{\{uid:(\d+)\}\}";

        return Regex.Replace(aiResponse, pattern, match =>
        {
            var idStr = match.Groups[1].Value;
            if (!(int.TryParse(idStr, out var userId) && senderIds.Count > userId)) return "滚木";

            var userAlias =
                GroupMemberCommandBase.GroupMemberCommandInstance.GetAliasById(senderIds[userId], groupId);
            if (userAlias.Aliases.Count > 0) return userAlias.Aliases.ToList()[0];

            if (GroupMemberCommandBase.GroupMemberCommandInstance.TryGetNickname(senderIds[userId], groupId,
                    out var nickname))
                return nickname;

            return "滚木";
        });
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
                case CqImageMsg:
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
        public readonly long[] AtIds = atIds ?? [];
        public readonly bool HasPicture = hasPicture;
        public readonly long MessageId = messageId;
        public readonly string MessageInPlainText = plainMessage;
        public readonly long RepliedMessageId = repliedMessageId;
        public readonly long SenderId = senderId;
        public readonly DateTime TimeStamp = DateTime.Now;
    }

    private class DeepSeekResponseDto
    {
        [JsonProperty("choices")] public Choices[] ChoicesResult { get; set; }

        public class Choices
        {
            [JsonProperty("message")] public DeepSeekRequestDto.DeepSeekMessageDto Message { get; set; }
        }
    }

    private class DeepSeekRequestDto(string content)
    {
        [JsonProperty("messages")]
        public DeepSeekMessageDto[] Message { get; set; } =
        [
            new("system",
                $"你是一个高效的对话分析机器人，名叫 {BotConfiguration.Instance.BotName}，擅长从原始聊天记录中提取核心信息并生成简洁、人性化的总结。请分析 user 提供的聊天记录，并生成一份直接面向用户的总结。\n1. 不要在输出中写入消息 ID 或时间戳，但要考虑时间顺序及消息回复逻辑（例如：聊天者在 hh:mm:ss 回复了消息 ID 为 ... 的消息，说...）。\n2. 严禁输出任何交互性用语（例如“好的，这是总结：”或“以上是对话内容”）。\n3. 在本对话中，用户将以 ID 形式出现（例如：12345）。当你总结时，如果需要提到具体某个人，请务必使用以下格式引用他们的 ID：{{{{uid:用户ID}}}}。（例如：如果 ID 为 12345 的人提出了建议，请写成“{{{{uid:12345}}}} 建议...”）\n4. 被 @ 的用户可能并不会在上下文中发出消息，请注意保留这些用户的 uid。\n6. 如果用户在聊天记录中赞美你，你可以在输出最后加一句简短的回应。\n5. 直接返回总结正文，使用流利的中文，除了上一条所说的 uid 标记格式，不要使用 Markdown 等标记语言的语法，返回纯文本。"
            ),
            new("user",
                content)
        ];

        [JsonProperty("model")] public string Model { get; set; } = "deepseek-chat";
        [JsonProperty("stream")] public bool Stream { get; set; }

        public class DeepSeekMessageDto(string role, string content)
        {
            [JsonProperty("content")] public string Content { get; set; } = content;
            [JsonProperty("role")] public string Role { get; set; } = role;
        }
    }
}