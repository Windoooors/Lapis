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
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        var sessionValid = TryGetSessionId(source, out var sessionId);

        if (!sessionValid)
            return;

        var matchedUserBindData = BindCommand.UserBindDataList.Find(data => data.QqId == source.Sender.UserId);

        if (matchedUserBindData == null || matchedUserBindData.DivingFishImportToken == null)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("您未绑定 DivingFish 分数上传 Token\n请访问 https://setchin.com/lapis/docs/ 以了解更多")
            ]);
            return;
        }

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
                { "range", MaiCommandInstance.Songs.Last().Id.ToString() }
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

        var musicDataList = new List<DivingFishMusicDataItemDto>();

        foreach (var rawData in rawMusicData.MusicData)
        {
            var fcString = rawData.ComboStatus switch
            {
                0 => "", 1 => "fc", 2 => "fcp", 3 => "ap", 4 => "app", _ => ""
            };

            var fsString = rawData.SyncStatus switch
            {
                0 => "",
                5 => "sync",
                1 => "fs",
                2 => "fsp",
                3 => "fsd",
                4 => "fsdp",
                _ => ""
            };

            SongDto song;

            try
            {
                song = MaiCommandInstance.GetSong(rawData.Id);
            }
            catch
            {
                continue;
            }

            if (rawData.Id >= 100000)
                rawData.Level -= 10;

            var musicData = new DivingFishMusicDataItemDto
            {
                Achievements = (float)rawData.Achievement / 10000,
                DxScore = rawData.DxScore,
                Fs = fsString,
                Fc = fcString,
                LevelIndex = rawData.Level,
                Title = song.Title,
                Type = song.Type.ToUpper()
            };

            musicDataList.Add(musicData);
        }

        var uploadContent = musicDataList.ToArray();

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
                new CqTextMsg($"更新成功！\n本次一共更新了 {musicDataList.Count} 张谱面的数据")
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
                    new CqTextMsg("您的水鱼成绩导入 Token 不正确或已过期，请尝试重新绑定水鱼账户")
                ]);
                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
        }
    }

    private class UploadRecordsResponseDto
    {
        [JsonProperty("status")] public string Status { get; set; }
    }

    private class DivingFishMusicDataItemDto
    {
        [JsonProperty("achievements")] public float Achievements;
        [JsonProperty("dxScore")] public int DxScore;
        [JsonProperty("fc")] public string Fc;
        [JsonProperty("fs")] public string Fs;
        [JsonProperty("level_index")] public int LevelIndex;
        [JsonProperty("title")] public string Title;
        [JsonProperty("type")] public string Type;
    }

    private class WckMusicDataResponseDto
    {
        public int Code { get; set; }

        public WckMusicDataResponseItemDto[] MusicData { get; set; }
    }

    private class WckMusicDataResponseItemDto
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public int Achievement { get; set; }
        public int DxScore { get; set; }
        public int ComboStatus { get; set; }
        public int SyncStatus { get; set; }
    }
}