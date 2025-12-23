using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Lapis.Settings;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class UpdateCommand : MaiCommandBase
{
    public UpdateCommand()
    {
        CommandHead = "update";
        DirectCommandHead = "update|更新";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("update", "1");
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        var matchedUserBindData = BindCommand.UserBindDataList.Find(data => data.QqId == source.Sender.UserId);

        if (matchedUserBindData == null || matchedUserBindData.AimeId == 0 ||
            matchedUserBindData.DivingFishImportToken == null)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("您的信息未绑定完全\n请访问 https://setchin.com/lapis/docs/ 以了解更多")
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
            responseString = ApiOperator.Instance.Post(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                "get_user_music_data",
                new UserMusicDataRequestDto(matchedUserBindData.AimeId, MaiCommandInstance.Songs.Last().Id), 60);
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

        var rawMusicData = JsonConvert.DeserializeObject<RawMusicDataDto>(responseString);

        var musicDataList = new List<MusicData>();

        foreach (var rawData in rawMusicData.RawMusicDataDetailArray)
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

            var musicData = new MusicData
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
            var uploadResponseString = ApiOperator.Instance.Post(BotConfiguration.Instance.DivingFishUrl,
                "api/maimaidxprober/player/update_records", uploadContent,
                [new KeyValuePair<string, string>("Import-Token", matchedUserBindData.DivingFishImportToken)], 60);

            var uploadResponse = JsonConvert.DeserializeObject<UploadRecordsResponseDto>(uploadResponseString);

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

    private class UserMusicDataRequestDto(long aimeId, int range)
    {
        public long AimeId = aimeId;
        public int Range = range;
    }

    private class MusicData
    {
        [JsonProperty("achievements")] public float Achievements;
        [JsonProperty("dxScore")] public int DxScore;
        [JsonProperty("fc")] public string Fc;
        [JsonProperty("fs")] public string Fs;
        [JsonProperty("level_index")] public int LevelIndex;
        [JsonProperty("title")] public string Title;
        [JsonProperty("type")] public string Type;
    }

    private class RawMusicDataDto
    {
        [JsonProperty("Code")] public int Code { get; set; }

        [JsonProperty("MusicData")] public RawMusicDataDetailDto[] RawMusicDataDetailArray { get; set; }
    }

    private class RawMusicDataDetailDto
    {
        [JsonProperty("musicId")] public int Id { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        [JsonProperty("achievement")] public int Achievement { get; set; }
        [JsonProperty("deluxscoreMax")] public int DxScore { get; set; }
        [JsonProperty("comboStatus")] public int ComboStatus { get; set; }
        [JsonProperty("syncStatus")] public int SyncStatus { get; set; }
    }
}