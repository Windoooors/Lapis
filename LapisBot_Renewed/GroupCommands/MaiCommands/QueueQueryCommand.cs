using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class QueueQueryCommand : MaiCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^有多少人$|^查人数$|^礼貌问几$|^几爷$|^几神$");
            DirectCommand = new Regex(@"^有多少人$|^查人数$|^礼貌问几$|^几爷$|^几神$");
            DefaultSettings.SettingsName = "城市英雄人数查询";
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

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            if (!(source.GroupId == "783365408" || source.GroupId == "753589269"))
                return Task.CompletedTask;

            var url = "https://otp.sega-nmsl.love/queue/current?qth=65f33b2abc3adf9f524c&showPassed=true";
            var content = Program.apiOperator.Get(url);
            var queue = JsonConvert.DeserializeObject<QueueDto>(content);
            
            if (queue.StatusCode == -1)
            {
                MessageManager.SendGroupMessageAsync(source.GroupId, "查询失败！");
                return Task.CompletedTask;
            }

            string text = "目前有 " + queue.DataItems.Length + " 人正在排卡\n\n";
            string textRight = string.Empty;
            string textLeft = string.Empty;
            foreach (DataItemDto dataItem in queue.DataItems)
            {
                var tempText = string.Empty;
                tempText += "    " + "第 " + dataItem.UserData.Rank + " 位：";
                tempText += dataItem.UserData.UserName + "\n";
                var tempPassedText = string.Empty;
                if (dataItem.UserData.Passed)
                    tempPassedText = "是";
                else
                    tempPassedText = "否";
                tempText += "        " + "是否过号：" + tempPassedText + "\n";
                tempText += "        " + "排卡创建时间：" + dataItem.CreatingTime.TimeOfDay + "\n";
                tempText += "        " + "上次排卡更新时间：" + dataItem.UpdatingTime.TimeOfDay + "\n\n";
                if (dataItem.UserData.IsRight)
                    textRight += tempText;
                else
                    textLeft += tempText;
            }

            if (textRight == string.Empty)
                textRight = "    无人排卡\n";
            
            if (textLeft == string.Empty)
                textLeft = "    无人排卡\n\n";

            text += "左机：\n" + textLeft;
            
            text += "右机：\n" + textRight;

            text = text.Substring(0, text.Length - 1);

            MessageManager.SendGroupMessageAsync(source.GroupId, text);
            return Task.CompletedTask;
        }

        private class QueueDto
        {
            [JsonProperty("statusCode")] public int StatusCode;
            [JsonProperty("data")] public DataItemDto[] DataItems;
        }

        private class DataItemDto
        {
            [JsonProperty("createdAt")] public DateTime CreatingTime;
            [JsonProperty("updatedAt")] public DateTime UpdatingTime;
            [JsonProperty("data")] public UserDataDto UserData;

            public class UserDataDto
            {
                [JsonProperty("queueId")] public int Rank;
                [JsonProperty("isRight")] public bool IsRight;
                [JsonProperty("passed")] public bool Passed;
                [JsonProperty("name")] public string UserName;
            }
        }
    }
}