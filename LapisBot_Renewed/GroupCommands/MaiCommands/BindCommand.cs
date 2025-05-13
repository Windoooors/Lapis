using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Net.Http;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Collections;
using LapisBot_Renewed.Operations.ApiOperation;
using LapisBot_Renewed.Settings;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class UserBindData
    {
        public long QqId;
        public long AimeId;
        public string DivingFishImportToken;
    }
    
    public class BindCommand : MaiCommandBase
    {
        public static List<UserBindData> UserBindDataList = new();

        public BindCommand()
        {
            CommandHead = new Regex("^bind");
            DirectCommandHead = new Regex("^bind|^绑定");
            ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("bind", "1");
        }

        public override Task Initialize()
        {
            if (File.Exists($"{AppContext.BaseDirectory}bind_data.json"))
                UserBindDataList =
                    JsonConvert.DeserializeObject<UserBindData[]>(
                        File.ReadAllText($"{AppContext.BaseDirectory}bind_data.json")).ToList();
            
            return Task.CompletedTask;
        }

        public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
        {
            var wechatArgumentRegex = new Regex(@"wechat\s");
            var divingFishArgumentRegex = new Regex(@"divingfish\s");
            if (wechatArgumentRegex.IsMatch(command.ToLower()))
            {
                BindWeChat(wechatArgumentRegex.Replace(command, string.Empty), source);
                return Task.CompletedTask;
            }
            if (divingFishArgumentRegex.IsMatch(command.ToLower()))
            {
                BindDivingFish(divingFishArgumentRegex.Replace(command, string.Empty), source);
                return Task.CompletedTask;
            }
            HelpCommand.Instance.ArgumentErrorHelp(source);
            return Task.CompletedTask;
        }
        
        public void BindWeChat(string command, CqGroupMessagePostContext source)
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

                return;
            }
            
            string responseString;
            try
            {
                responseString = ApiOperator.Instance.Post(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                    "get_aime_id",
                    new AimeIdRequestDto(command));
            }
            catch(Exception exception)
            {
                if (exception is HttpRequestException)
                {
                    Program.Session.SendGroupMessage(source.GroupId, [
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("与服务器的通信出现问题")
                    ]);
                    return;
                }
                
                HelpCommand.Instance.ArgumentErrorHelp(source);
                return;
            }

            var response = JsonConvert.DeserializeObject<AimeIdResponseDto>(responseString);

            if (response.Code == 101)
            {
                Program.Session.SendGroupMessage(source.GroupId, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("绑定出现问题，可能是您的二维码已过期")
                ]);

                return;
            }

            matchedUserBindData.AimeId = response.AimeId;
            
            Program.Session.SendGroupMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("绑定成功！\n请及时撤回您的二维码扫描结果或立刻重新获取一个新的二维码")
            ]);

            File.WriteAllText($"{AppContext.BaseDirectory}bind_data.json",
                JsonConvert.SerializeObject(UserBindDataList.ToArray()));
        }

        public void BindDivingFish(string command, CqGroupMessagePostContext source)
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

                return;
            }

            matchedUserBindData.DivingFishImportToken = command;
            
            File.WriteAllText($"{AppContext.BaseDirectory}bind_data.json",
                JsonConvert.SerializeObject(UserBindDataList.ToArray()));
            
            Program.Session.SendGroupMessage(source.GroupId, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("绑定成功！\n请及时撤回")
            ]);
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
