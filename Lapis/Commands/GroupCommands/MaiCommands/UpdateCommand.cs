using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class UpdateCommand : WckCommandBase
{
    public UpdateCommand()
    {
        CommandHead = "update";
        DirectCommandHead = "update|更新";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("update", "1");
        IntendedArgumentCount = 1;
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        Process(true, source);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        var argument = arguments[0];

        if (!bool.TryParse(argument, out var isTrue))
        {
            SendMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("未知的参数")
            ]);
            return;
        }

        var sendToDivingFish = isTrue;

        Process(sendToDivingFish, source);
    }

    private void Process(bool sendToDivingFish, CqGroupMessagePostContext source)
    {
        var sessionValid = TryGetSessionId(source, out var sessionId);

        if (!sessionValid)
            return;

        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("正在尝试更新成绩，请稍等")
        ]);

        string responseString;

        try
        {
            var parameters = new Dictionary<string, string>
            {
                { "session_id", sessionId },
                { "range_to", "200000" },
                { "range_from", "0" }
            };

            var response = ApiOperator.Instance.Get(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                "v1/user_music_data", parameters, 240);

            responseString = response.Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("与服务器通信时出现问题")
                ]);
                return;
            }
        }
        catch (Exception)
        {
            HelpCommand.Instance.UnexpectedErrorHelp(source);
            return;
        }

        var rawMusicData = JsonConvert.DeserializeObject<WckMusicDataResponseDto>(responseString);

        if (rawMusicData.Code != 200)
        {
            HelpCommand.Instance.UnexpectedErrorHelp(source);
            return;
        }

        Upsert(source.Sender.UserId, rawMusicData.MusicData);

        if (sendToDivingFish)
        {
            var matchedUserBindData = BindCommand.UserBindDataList.Find(data => data.QqId == source.Sender.UserId);

            if (matchedUserBindData == null || matchedUserBindData.DivingFishImportToken == null)
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg(
                        $"您未绑定 DivingFish 分数上传 Token\n" +
                        $"分数数据已被缓存至 {BotConfiguration.Instance.BotName}\n" +
                        $"请访问 https://setchin.com/lapis/docs/ 以了解更多")
                ]);
                return;
            }

            var uploadContent = ConvertData(rawMusicData.MusicData, source.Sender.UserId);

            UploadToDivingFish(uploadContent, source, matchedUserBindData);
        }
    }

    private void Upsert(long qqId, WckMusicDataResponseItemDto[] rawMusicData)
    {
        DatabaseHandler.Instance.SongMetaDatabaseOperator.UpsertScores(
            rawMusicData.Select(x => new ChartScoreData(x, qqId)).ToArray());
    }

    private void UploadToDivingFish(DivingFishMusicDataDto[] uploadContent, CqGroupMessagePostContext source,
        UserBindData matchedUserBindData)
    {
        try
        {
            var uploadRequestResult = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/player/update_records", uploadContent,
                [
                    new KeyValuePair<string, string>("Import-Token", matchedUserBindData.DivingFishImportToken),
                    new KeyValuePair<string, string>("Developer-Token", BotConfiguration.Instance.DivingFishDevToken)
                ], 60);

            if (uploadRequestResult.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"Unexpected status code: {uploadRequestResult.StatusCode}", null,
                    uploadRequestResult.StatusCode);

            var uploadResponse = JsonConvert.DeserializeObject<UploadRecordsResponseDto>(uploadRequestResult.Result);

            if (uploadResponse.Status == "error")
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("您的水鱼成绩导入 Token 不正确或已过期，请尝试重新绑定水鱼账户")
                ]);
                return;
            }

            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg($"更新成功！\n本次一共更新了 {uploadContent.Length} 张谱面的数据")
            ]);
        }
        catch (Exception exception)
        {
            if (exception is TaskCanceledException)
            {
                DivingFishErrorHelp(source);
                return;
            }

            if (exception is HttpRequestException)
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("您的水鱼成绩导入 Token 不正确或已过期，请尝试重新绑定水鱼账户\n" +
                                  $"分数数据已被缓存至 {BotConfiguration.Instance.BotName}\n")
                ]);
                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
        }
    }

    private static DivingFishMusicDataDto[] ConvertData(WckMusicDataResponseItemDto[] rawMusicData, long qqId)
    {
        return rawMusicData
            .Select(x =>
                new ChartScoreData(x, qqId)
                {
                    Song = MaiCommandInstance.GetSongById(x.Id) ?? new SongMetaData
                    {
                        Title = "滚木"
                    }
                }.ToDivingFishDto()).ToArray();
    }

    private class UploadRecordsResponseDto
    {
        [JsonProperty("status")] public string Status { get; set; }
    }

    public class WckMusicDataResponseDto
    {
        [JsonProperty] public int Code { get; set; }

        [JsonProperty] public WckMusicDataResponseItemDto[] MusicData { get; set; }
    }
}