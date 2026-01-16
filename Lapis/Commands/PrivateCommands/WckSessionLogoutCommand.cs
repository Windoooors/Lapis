using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.MaiCommands;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Newtonsoft.Json;

namespace Lapis.Commands.PrivateCommands;

public class WckSessionLogoutCommand : PrivateCommand
{
    public WckSessionLogoutCommand()
    {
        CommandHead = "logout";
        DirectCommandHead = "logout";
    }

    public override void Parse(string originalPlainMessage, CqPrivateMessagePostContext source)
    {
        try
        {
            if (!WckCommandBase.TryGetSessionId(source, out var sessionId)) return;

            var parameters = new Dictionary<string, string>
            {
                { "session_id", sessionId }
            };

            var responseString = ApiOperator.Instance.Delete(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                "v1/sessions", parameters, 120);

            var responseObject = JsonConvert.DeserializeObject<SessionLogoutResponseDto>(responseString.Result);

            if (responseObject.Code != 200)
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("登出失败！")
                ]);
                return;
            }

            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("登出成功！")
            ]);
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException { StatusCode: HttpStatusCode.BadRequest })
            {
                SendMessage(source, [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("登出失败！")
                ]);
                return;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
        }
    }

    private class SessionLogoutResponseDto
    {
        public int Code { get; set; }
    }
}