using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Collections;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    
    public class AircadeCommand : MaiCommand
    {
        public class AircadeSettings : GroupCommandSettings
        {
            public string LocationId { get; set; }

            public AircadeSettings Clone(AircadeSettings infoSettings)
            {
                return JsonConvert.DeserializeObject<AircadeSettings>(JsonConvert.SerializeObject(infoSettings));
            }
        }
        
        public override Task GetDefaultSettings()
        {
            CurrentGroupCommandSettings = ((AircadeSettings)DefaultSettings).Clone((AircadeSettings)DefaultSettings);
            return Task.CompletedTask;
        }
        
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^左机有几爷$|^右机有几爷$|^[1-9]机有多少人$|^查人数$|^礼貌问几$|^几爷$|^几神$");
            DirectCommand = new Regex(@"^左机有几爷$|^右机有几爷$|^[1-9]机有多少人$|^查人数$|^礼貌问几$|^几爷$|^几神$");
            DefaultSettings = new AircadeSettings
            {
                Enabled = true,
                LocationId = "",
                DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" }, { "LocationId", "Location ID" } },
                SettingsName = "Aircade 人数查询"
            };
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
                settingsList.Add(JsonConvert.DeserializeObject<AircadeSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        private string _originalCommand = string.Empty;

        public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
        {
            _originalCommand = command;
            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            try
            {
                if (((AircadeSettings)CurrentGroupCommandSettings).LocationId == string.Empty)
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                    [
                        new CqTextMsg("查询失败！\n未设置 Location ID，请发送 \"几爷 settings 2 [Location ID]\" 以进行设置")
                    ]);
                    return Task.CompletedTask;
                }
                
                var indexRegex = new Regex(@"[1-9]机有多少人$");
                var numberRegex = new Regex("[1-9]");
                var machineIndex = 0;
                if (_originalCommand.Contains("右机"))
                    machineIndex = 1;
                if (indexRegex.IsMatch(_originalCommand))
                    machineIndex = Int32.Parse(numberRegex.Matches(_originalCommand)[0].ToString()) - 1;
                var url = "https://api.arcade-link.top/queue?locationId=" + ((AircadeSettings)CurrentGroupCommandSettings).LocationId + "&deviceId=" + machineIndex;
                var content = Program.apiOperator.Get(url);
                var queue = JsonConvert.DeserializeObject<QueueDto>(content);

                if (queue.StatusCode != 200)
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                    [
                        new CqTextMsg("查询失败！")
                    ]);
                    return Task.CompletedTask;
                }

                string text = "目前有 " + queue.DataItems.Length + " 人正在排卡\n\n";

                var queueList = queue.DataItems.ToList();
                queueList.Sort((x, y) => x.QueueId.CompareTo(y.QueueId));
                queue.DataItems = queueList.ToArray();
                
                foreach (DataItemDto dataItem in queue.DataItems)
                {
                    var tempText = string.Empty;
                    tempText += "    " + "第 " + dataItem.QueueId + " 位：";
                    tempText += dataItem.Nickname + "\n";
                    var tempPassedText = string.Empty;
                    if (dataItem.HasPassed)
                        tempPassedText = "是";
                    else
                        tempPassedText = "否";
                    tempText += "        " + "是否过号：" + tempPassedText + "\n";
                    text += tempText;
                }
                
                text = text.Substring(0, text.Length - 1);

                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqTextMsg(text)
                ]);
            }
            catch
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqTextMsg("查询失败！")
                ]);
            }
            return Task.CompletedTask;
        }

        public class QueueDto
        {
            [JsonProperty("code")] public int StatusCode;
            [JsonProperty("data")] public DataItemDto[] DataItems;
        }

        public class DataItemDto
        {
            [JsonProperty("queueId")] public int QueueId;
            [JsonProperty("machineId")] public int MachineId;
            [JsonProperty("hasPassed")] public bool HasPassed;
            [JsonProperty("nickname")] public string Nickname;
        }
    }
}