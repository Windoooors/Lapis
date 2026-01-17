using System;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Newtonsoft.Json;

namespace Lapis.Commands.PrivateCommands;

public class WckSessionLoginCommand : PrivateCommand // Wck stands for WahlapConnectiveKits
{
    public WckSessionLoginCommand()
    {
        CommandHead = "login";
        DirectCommandHead = "login";
        IntendedArgumentCount = 1;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqPrivateMessagePostContext source)
    {
        try
        {
            var responseString = ApiOperator.Instance.Post(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                "v1/sessions", new SessionLoginRequestDto
                {
                    WechatQrCode = arguments[0],
                    QqId = source.Sender.UserId
                }, 120);

            var responseObject = JsonConvert.DeserializeObject<SessionLoginResponseDto>(responseString.Result);

            if (responseObject.Code != 200)
            {
                switch (responseObject.Code)
                {
                    case 402:
                        SendMessage(source, [
                            new CqReplyMsg(source.MessageId),
                            new CqTextMsg("登录失败！可能是因为此账号没有登出")
                        ]);
                        break;
                    case 403:
                        SendMessage(source, [
                            new CqReplyMsg(source.MessageId),
                            new CqTextMsg("登录失败！此二维码已过期")
                        ]);
                        break;
                }

                return;
            }

            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("登录成功！\n使用查询功能完毕后请记得登出，否则无法登入游戏\n如要登出，请在此发送指令 \"logout\"")
            ]);
        }
        catch (Exception)
        {
            HelpCommand.Instance.UnexpectedErrorHelp(source);
        }
    }

    private class SessionLoginResponseDto
    {
        [JsonProperty] public int Code { get; set; }
    }

    private class SessionLoginRequestDto
    {
        [JsonProperty] public string WechatQrCode { get; set; }
        [JsonProperty] public long QqId { get; set; }
    }
}