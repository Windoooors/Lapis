using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Operations.ApiOperation;
using LapisBot.Settings;
using Newtonsoft.Json;

namespace LapisBot.Commands.UniversalCommands;

public class UserBindData
{
    public long AimeId;
    public string DivingFishImportToken;
    public long QqId;
}

public class BindCommand : UniversalCommand
{
    public static List<UserBindData> UserBindDataList = new();

    public BindCommand()
    {
        CommandHead = new Regex("^bind");
        DirectCommandHead = new Regex("^bind|^绑定");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("bind", "1");
    }

    public override void Initialize()
    {
        if (File.Exists($"{AppContext.BaseDirectory}data/bind_data.json"))
            UserBindDataList =
                JsonConvert.DeserializeObject<UserBindData[]>(
                    File.ReadAllText($"{AppContext.BaseDirectory}data/bind_data.json")).ToList();
    }

    public override void ParseWithArgument(string command, CqMessagePostContext source)
    {
        var wechatArgumentRegex = new Regex(@"wechat\s");
        var divingFishArgumentRegex = new Regex(@"divingfish\s");
        if (wechatArgumentRegex.IsMatch(command.ToLower()))
        {
            BindWeChat(wechatArgumentRegex.Replace(command, string.Empty), source);
            return;
        }

        if (divingFishArgumentRegex.IsMatch(command.ToLower()))
        {
            BindDivingFish(divingFishArgumentRegex.Replace(command, string.Empty), source);
            return;
        }

        HelpCommand.Instance.ArgumentErrorHelp(source);
    }

    private void BindWeChat(string command, CqMessagePostContext source)
    {
        var matchedUserBindData = UserBindDataList.Find(data => data.QqId == source.UserId);

        if (matchedUserBindData == null)
        {
            matchedUserBindData = new UserBindData();
            matchedUserBindData.QqId = source.UserId;
            UserBindDataList.Add(matchedUserBindData);
        }

        if (command == "unbind")
        {
            matchedUserBindData.AimeId = 0;

            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("解绑成功！")
            ]);

            File.WriteAllText($"{AppContext.BaseDirectory}data/bind_data.json",
                JsonConvert.SerializeObject(UserBindDataList.ToArray()));

            return;
        }

        string responseString;
        try
        {
            responseString = ApiOperator.Instance.Post(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                "get_aime_id",
                new AimeIdRequestDto(command), 60);
        }
        catch (Exception exception)
        {
            if (exception is TaskCanceledException or HttpRequestException)
            {
                SendMessage(source, [
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
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("绑定出现问题，可能是您的二维码已过期")
            ]);

            return;
        }

        matchedUserBindData.AimeId = response.AimeId;

        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("绑定成功！\n请及时撤回您的二维码扫描结果或立刻重新获取一个新的二维码以避免泄漏")
        ]);

        File.WriteAllText($"{AppContext.BaseDirectory}data/bind_data.json",
            JsonConvert.SerializeObject(UserBindDataList.ToArray()));
    }

    private void BindDivingFish(string command, CqMessagePostContext source)
    {
        var matchedUserBindData = UserBindDataList.Find(data => data.QqId == source.UserId);

        if (matchedUserBindData == null)
        {
            matchedUserBindData = new UserBindData();
            matchedUserBindData.QqId = source.UserId;
            UserBindDataList.Add(matchedUserBindData);
        }

        if (command == "unbind")
        {
            matchedUserBindData.DivingFishImportToken = null;

            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("解绑成功！")
            ]);

            File.WriteAllText($"{AppContext.BaseDirectory}data/bind_data.json",
                JsonConvert.SerializeObject(UserBindDataList.ToArray()));

            return;
        }

        matchedUserBindData.DivingFishImportToken = command;

        File.WriteAllText($"{AppContext.BaseDirectory}data/bind_data.json",
            JsonConvert.SerializeObject(UserBindDataList.ToArray()));

        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("绑定成功！\n请及时撤回以避免泄漏")
        ]);
    }

    private class AimeIdRequestDto(string wechatQrCode)
    {
        public string WechatQrCode = wechatQrCode;
    }

    private class AimeIdResponseDto
    {
        [JsonProperty] public long AimeId;

        [JsonProperty] public int Code;
    }
}