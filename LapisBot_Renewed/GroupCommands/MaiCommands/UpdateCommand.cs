using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Linq;
using System.Net.Http;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Collections;
using LapisBot_Renewed.GroupCommands.MaiCommands.AliasCommands;
using LapisBot_Renewed.Operations.ApiOperation;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class UpdateCommand : MaiCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex("^update$");
            DirectCommand = new Regex("^update$|^更新$");
            DefaultSettings.SettingsName = "舞萌歌曲数据更新";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");
            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            var matchedUserBindData = BindCommand.UserBindDataList.Find(data => data.QqId == source.Sender.UserId);

            if (matchedUserBindData == null || matchedUserBindData.AimeId == 0 ||
                matchedUserBindData.DivingFishImportToken == null)
            {
                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("您的信息未绑定完全\n请访问 https://setchin.com/lapis/docs/index.html#/maimaiDX/bind 以了解更多")
                ]);
                return Task.CompletedTask;
            }

            var responseString = "";

            try
            {
                responseString = ApiOperator.Instance.Post(BotSettings.Instance.WahlapConnectiveKitsUrl,
                    "get_user_music_data",
                    new UserMusicDataRequestDto(matchedUserBindData.AimeId, Instance.Songs.Last().Id));
            }
            catch (Exception exception)
            {
                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("出现未处理的错误")
                ]);
                return Task.CompletedTask;
            }

            var rawMusicData = JsonConvert.DeserializeObject<RawMusicDataDto>(responseString);

            var musicDataList = new List<MusicData>();
            
            foreach (var rawData in rawMusicData.RawMusicDataDetailArray)
            {
                var fcString = "";
                var fsString = "";
                
                switch (rawData.ComboStatus)
                {
                    case 0:
                        fcString = "";
                        break;
                    case 1:
                        fcString = "fc";
                        break;
                    case 2:
                        fcString = "fcp";
                        break;
                    case 3:
                        fcString = "ap";
                        break;
                    case 4:
                        fcString = "app";
                        break;
                }
                
                switch (rawData.SyncStatus)
                {
                    case 0:
                        fsString = "";
                        break;
                    case 5:
                        fsString = "sync";
                        break;
                    case 1:
                        fsString = "fs";
                        break;
                    case 2:
                        fsString = "fsp";
                        break;
                    case 3:
                        fsString = "fsd";
                        break;
                    case 4:
                        fsString = "fsdp";
                        break;
                }

                var song = new SongDto();

                try
                {
                    song = Instance.GetSong(rawData.Id);
                }
                catch
                {
                    continue;
                }

                if (rawData.Id >= 100000)
                    rawData.Level -= 10;

                var musicData = new MusicData()
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
                var uploadResponseString = ApiOperator.Instance.Post(BotSettings.Instance.DivingFishUrl,
                    "api/maimaidxprober/player/update_records", uploadContent,
                    [new KeyValuePair<string, string>("Import-Token", matchedUserBindData.DivingFishImportToken)]);
                
                var uploadResponse = JsonConvert.DeserializeObject<UploadRecordsResponseDto>(uploadResponseString);

                if (uploadResponse.Status == "error")
                {
                    Program.Session.SendGroupMessage(source.GroupId, [
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("您的水鱼成绩导入 Token 不正确或已过期，请尝试重新绑定水鱼账户")
                    ]);
                    return Task.CompletedTask;
                }

                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg($"更新成功！\n本次一共更新了 {musicDataList.Count} 张谱面的数据")
                ]);
            }
            catch (Exception exception)
            {
                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("出现未处理的错误")
                ]);
                return Task.CompletedTask;
            }


            return Task.CompletedTask;
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
            [JsonProperty("Code")]
            public int Code;
            
            [JsonProperty("MusicData")]
            public RawMusicDataDetailDto[] RawMusicDataDetailArray;
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
}
