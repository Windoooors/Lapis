using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.MaiCommands.AliasCommands;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class PlayerInfoCommand : MaiCommandBase
{
    public PlayerInfoCommand()
    {
        CommandHead = "me|myinfo|pinfo";
        DirectCommandHead = "myinfo|pinfo|me";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("me", "1");
        IntendedArgumentCount = 1;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage, CqGroupMessagePostContext source)
    {
        var isGroupMember = GroupMemberCommandBase.GroupMemberCommandInstance.TryGetMember(arguments[0],
            out var groupMembers, source) && groupMembers.Length == 1;
        
        if (!isGroupMember)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("未找到该群友或该群友未绑定舞萌微信账户")
            ]);
            return;
        }
        
        var matchedUserBindData = BindCommand.UserBindDataList.Find(data => data.QqId == groupMembers[0].Id);

        if (matchedUserBindData == null || matchedUserBindData.AimeId == 0)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("未找到该群友或该群友未绑定舞萌微信账户")
            ]);
            return;
        }

        ProcessAndSendMessage(source, matchedUserBindData.AimeId);
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        var matchedUserBindData = BindCommand.UserBindDataList.Find(data => data.QqId == source.Sender.UserId);

        if (matchedUserBindData == null || matchedUserBindData.AimeId == 0)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("您的信息未绑定完全\n请访问 https://setchin.com/lapis/docs/ 以了解更多")
            ]);
            return;
        }

        ProcessAndSendMessage(source, matchedUserBindData.AimeId);
    }

    private void ProcessAndSendMessage(CqGroupMessagePostContext source, long userId)
    {
        string responseString;
        
        try
        {
            responseString = ApiOperator.Instance.Post(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                "get_user_data",
                new PlayInfoRequestDto{UserId = userId}, 60);
        }
        catch (Exception exception)
        {
            if (exception is TaskCanceledException or HttpRequestException)
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("与服务器通信时出现问题")
                ]);
                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
            return;
        }
        
        var content = JsonConvert.DeserializeObject<PlayInfoResponseDto>(responseString);

        if (content.Code is not (1 or 2))
        {
            HelpCommand.Instance.UnexpectedErrorHelp(source);
            return;
        }

        var stringBuilder = new StringBuilder().AppendLine(
            $"玩家名字: {(content.Code == 1 ? content.UserDetailData.UserName : content.UserBriefData.UserName)}");
        stringBuilder.AppendLine(
            $"Rating: {(content.Code == 1 ? content.UserDetailData.PlayerRating : content.UserBriefData.PlayerRating)}");
        
        if (content.Code == 1)
        {
            stringBuilder.AppendLine($"B35 Rating: {content.UserDetailData.PlayerOldRating}");
            stringBuilder.AppendLine($"B15 Rating: {content.UserDetailData.PlayerNewRating}");
            stringBuilder.AppendLine($"PC: {content.UserDetailData.PlayCount}");
            stringBuilder.AppendLine($"当前大版本 PC: {content.UserDetailData.CurrentVersionPlayCount}");
            stringBuilder.AppendLine($"首次游玩时间: {content.UserDetailData.FirstPlayDate.ToString("yyyy-MM-dd HH:mm:ss")}");
            stringBuilder.AppendLine($"上次登录时间: {content.UserDetailData.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss")}");
            stringBuilder.AppendLine($"上次登出时间: {content.UserDetailData.LastLogoutDate.ToString("yyyy-MM-dd HH:mm:ss")}");
            stringBuilder.AppendLine($"上次游玩地区: {content.UserDetailData.LastRegionName}\n");
        }
        else if (content.Code == 2)
        {
            stringBuilder.AppendLine("获取二维码可获取更多玩家信息\n");
        }

        stringBuilder.AppendLine("游玩履历：");
        foreach (var region in content.UserRegions)
        {
            stringBuilder.AppendLine($"    {region.RegionName}");
            stringBuilder.AppendLine($"    PC: {region.PlayCount}");
            stringBuilder.AppendLine($"    首次登录时间: {region.CreatedDate}");
        }

        stringBuilder.AppendLine();

        if (userId < 11000000)
            stringBuilder.AppendLine("我趣！老资历！");
        if (userId >= 12500000)
            stringBuilder.AppendLine("我趣！小资历！");
        
        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(stringBuilder.ToString().Trim())
        ]);
    }

    private class PlayInfoRequestDto
    {
        public long UserId;
    }

    private class PlayInfoResponseDto
    {
        public int Code;
        public UserBriefDataDto UserBriefData;
        public UserDetailDataDto UserDetailData;

        public UserRegionDto[] UserRegions;
    }

    public class UserRegionDto
    {
        [JsonProperty("regionId")] public int RegionId { get; set; }
        [JsonProperty("regionName")]  public string RegionName;
        [JsonProperty("created")] public DateTime CreatedDate { get; set; }
        [JsonProperty("playCount")] public int PlayCount { get; set; }
    }
    
    public class UserDetailDataDto
    {
        [JsonProperty("userName")] public string UserName { get; set; }
        [JsonProperty("playerRating")] public int PlayerRating { get; set; }
        [JsonProperty("playerNewRating")] public int PlayerNewRating { get; set; }
        [JsonProperty("playerOldRating")] public int PlayerOldRating { get; set; }
        [JsonProperty("playCount")] public int PlayCount { get; set; }
        [JsonProperty("currentPlayCount")] public int CurrentVersionPlayCount { get; set; }
        [JsonProperty("lastLoginDate")] public DateTime LastLoginDate { get; set; }
        [JsonProperty("lastPlayDate")] public DateTime LastLogoutDate { get; set; }
        [JsonProperty("firstPlayDate")] public DateTime FirstPlayDate { get; set; }
        [JsonProperty("lastRegionName")] public string LastRegionName = "";
    }

    public class UserBriefDataDto
    {
        [JsonProperty("userName")] public string UserName { get; set; }
        [JsonProperty("playerRating")] public int PlayerRating { get; set; }
    }
}