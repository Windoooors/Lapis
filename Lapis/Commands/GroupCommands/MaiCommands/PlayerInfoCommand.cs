using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class PlayerInfoCommand : WckCommandBase
{
    public PlayerInfoCommand()
    {
        CommandHead = "me|myinfo|pinfo";
        DirectCommandHead = "myinfo|pinfo|me";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("me", "1");
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        var sessionValid = TryGetSessionId(source, out var sessionId);

        if (!sessionValid)
            return;

        string responseString;

        try
        {
            var parameters = new Dictionary<string, string>
            {
                { "session_id", sessionId }
            };
            var response = ApiOperator.Instance.Get(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                "v1/user_regions", parameters, 60);

            responseString = response.Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("与服务器通信时出现问题")
                ]);
                return;
            }

            var content = JsonConvert.DeserializeObject<PlayInfoResponseDto>(responseString);

            if (content.Code is not 200)
            {
                HelpCommand.Instance.UnexpectedErrorHelp(source);
                return;
            }

            var stringBuilder = new StringBuilder().AppendLine(
                $"玩家名: {content.UserName}");
            stringBuilder.AppendLine(
                $"Rating: {content.PlayerRating}");

            stringBuilder.AppendLine($"B35 Rating: {content.PlayerOldRating}");
            stringBuilder.AppendLine($"B15 Rating: {content.PlayerNewRating}");
            stringBuilder.AppendLine($"PC: {content.PlayCount}");
            stringBuilder.AppendLine($"当前大版本 PC: {content.CurrentVersionPlayCount}");
            stringBuilder.AppendLine($"首次游玩时间: {content.FirstPlayDate.ToString("yyyy-MM-dd HH:mm:ss")}");
            stringBuilder.AppendLine($"上次登录时间: {content.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss")}");
            stringBuilder.AppendLine(
                $"上次登出时间: {content.LastLogoutDate.ToString("yyyy-MM-dd HH:mm:ss")}");
            stringBuilder.AppendLine($"上次游玩地区: {content.LastRegionName}\n");

            stringBuilder.AppendLine("游玩履历：");
            foreach (var region in content.UserRegions)
            {
                stringBuilder.AppendLine($"    {region.RegionName}");
                stringBuilder.AppendLine($"    PC: {region.PlayCount}");
                stringBuilder.AppendLine($"    首次登录时间: {region.CreatedDate}");
            }

            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(stringBuilder.ToString().Trim())
            ]);
        }
        catch (Exception)
        {
            HelpCommand.Instance.UnexpectedErrorHelp(source);
        }
    }

    private class PlayInfoResponseDto
    {
        [JsonProperty] public int Code { get; set; }

        [JsonProperty] public string LastRegionName { get; set; } = "";

        [JsonProperty] public UserRegionDto[] UserRegions { get; set; }
        [JsonProperty] public string UserName { get; set; }
        [JsonProperty] public int PlayerRating { get; set; }
        [JsonProperty] public int PlayerNewRating { get; set; }
        [JsonProperty] public int PlayerOldRating { get; set; }
        [JsonProperty] public int PlayCount { get; set; }
        [JsonProperty] public int CurrentVersionPlayCount { get; set; }
        [JsonProperty] public DateTime LastLoginDate { get; set; }
        [JsonProperty] public DateTime LastLogoutDate { get; set; }
        [JsonProperty] public DateTime FirstPlayDate { get; set; }
    }

    private class UserRegionDto
    {
        [JsonProperty] public string RegionName { get; set; }
        [JsonProperty] public DateTime CreatedDate { get; set; }
        [JsonProperty] public int PlayCount { get; set; }
    }
}