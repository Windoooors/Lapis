using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;
using Newtonsoft.Json;

namespace Lapis.Commands.UniversalCommands;

public class UserBindData
{
    public string DivingFishImportToken;
    public long QqId;
}

public class BindCommand : UniversalCommand
{
    public static List<UserBindData> UserBindDataList = new();

    public BindCommand()
    {
        CommandHead = "bind";
        DirectCommandHead = "bind|绑定";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("bind", "1");
        IntendedArgumentCount = 2;
    }

    public override void Initialize()
    {
        if (File.Exists($"{AppContext.BaseDirectory}data/bind_data.json"))
            UserBindDataList =
                JsonConvert.DeserializeObject<UserBindData[]>(
                    File.ReadAllText($"{AppContext.BaseDirectory}data/bind_data.json")).ToList();
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage, CqMessagePostContext source)
    {
        if (arguments.Length < IntendedArgumentCount)
        {
            BindDivingFish(arguments[0], source);
            return;
        }

        var divingFishArgumentRegex = new Regex("divingfish");

        if (divingFishArgumentRegex.IsMatch(arguments[0].ToLower()))
        {
            BindDivingFish(divingFishArgumentRegex.Replace(arguments[1], string.Empty, 1), source);
            return;
        }

        HelpCommand.Instance.ArgumentErrorHelp(source);
    }

    private void BindDivingFish(string command, CqMessagePostContext source)
    {
        var matchedUserBindData = UserBindDataList.Find(data => data.QqId == source.UserId);

        if (matchedUserBindData == null || matchedUserBindData.QqId == 0)
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
}