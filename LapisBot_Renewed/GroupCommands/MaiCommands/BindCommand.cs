using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Collections;
using LapisBot_Renewed.GroupCommands.MaiCommands.AliasCommands;
using LapisBot_Renewed.Operations.ApiOperation;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class UserBindData
    {
        public long QqId;
        public long AimeId;
        public string DivingFishImportToken;
    }
    
    public class BindCommand : MaiCommand
    {
        public static List<UserBindData> UserBindDataList = new List<UserBindData>();

        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^bind\swechat\s");
            DirectCommand = new Regex(@"^bind\swechat\s|^绑定\swechat\s");
            SubHeadCommand = new Regex(@"^bind\sdivingfish\s");
            SubDirectCommand  = new Regex(@"^bind\sdivingfish\s|^绑定\sdivingfish\s");
            DefaultSettings.SettingsName = "舞萌绑定";
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

            if (File.Exists($"{AppContext.BaseDirectory}bind_data.json"))
                UserBindDataList =
                    JsonConvert.DeserializeObject<UserBindData[]>(
                        File.ReadAllText($"{AppContext.BaseDirectory}bind_data.json")).ToList();
            
            return Task.CompletedTask;
        }
        
        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            var matchedUserBindData = UserBindDataList.Find(data => data.QqId == source.Sender.UserId);

            if (matchedUserBindData == null)
            {
                matchedUserBindData = new UserBindData();
                matchedUserBindData.QqId = source.Sender.UserId;
                UserBindDataList.Add(matchedUserBindData);
            }
            
            if (command == "unbind")
            {
                matchedUserBindData.AimeId = 0;
                
                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("解绑成功！")
                ]);
                
                File.WriteAllText($"{AppContext.BaseDirectory}bind_data.json",
                    JsonConvert.SerializeObject(UserBindDataList.ToArray()));
                
                return Task.CompletedTask;
            }
            
            string responseString;
            try
            {
                responseString = ApiOperator.Instance.Post(BotSettings.Instance.WahlapConnectiveKitsUrl,
                    "get_aime_id",
                    new AimeIdRequestDto(command));
            }
            catch(Exception exception)
            {
                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("出现未处理的错误")
                ]);
                
                return Task.CompletedTask;
            }

            var response = JsonConvert.DeserializeObject<AimeIdResponseDto>(responseString);

            if (response.Code == 101)
            {
                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("绑定出现问题，可能是您的二维码已过期")
                ]);
                
                return Task.CompletedTask;
            }

            matchedUserBindData.AimeId = response.AimeId;
            
            Program.Session.SendGroupMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("绑定成功！\n请及时撤回您的二维码扫描结果或立刻重新获取一个新的二维码")
            ]);

            File.WriteAllText($"{AppContext.BaseDirectory}bind_data.json",
                JsonConvert.SerializeObject(UserBindDataList.ToArray()));

            return Task.CompletedTask;
        }

        public override Task SubParse(string command, CqGroupMessagePostContext source)
        {
            var matchedUserBindData = UserBindDataList.Find(data => data.QqId == source.Sender.UserId);

            if (matchedUserBindData == null)
            {
                matchedUserBindData = new UserBindData();
                matchedUserBindData.QqId = source.Sender.UserId;
                UserBindDataList.Add(matchedUserBindData);
            }
            
            if (command == "unbind")
            {
                matchedUserBindData.DivingFishImportToken = null;
                
                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("解绑成功！")
                ]);
                
                File.WriteAllText($"{AppContext.BaseDirectory}bind_data.json",
                    JsonConvert.SerializeObject(UserBindDataList.ToArray()));
                
                return Task.CompletedTask;
            }

            matchedUserBindData.DivingFishImportToken = command;
            
            File.WriteAllText($"{AppContext.BaseDirectory}bind_data.json",
                JsonConvert.SerializeObject(UserBindDataList.ToArray()));
            
            Program.Session.SendGroupMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("绑定成功！\n请及时撤回")
            ]);
            
            return Task.CompletedTask;
        }

        private class AimeIdRequestDto(string wechatQrCode)
        {
            public string WechatQrCode = wechatQrCode;
        }

        private class AimeIdResponseDto
        {
            public int Code;
            public long AimeId;
        }
    }
}
