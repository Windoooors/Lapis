using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Newtonsoft.Json;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class WckCommandBase : MaiCommandBase // Wck stands for WahlapConnectiveKits
{
    public static bool TryGetSessionId(CqMessagePostContext source, out string sessionId)
    {
        try
        {
            var parameters = new Dictionary<string, string>
            {
                { "qq_id", source.UserId.ToString() }
            };

            var response = ApiOperator.Instance.Get(BotConfiguration.Instance.WahlapConnectiveKitsUrl,
                "v1/sessions", parameters, 120);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"Unexpected status code: {response.StatusCode}", null,
                    response.StatusCode);

            var responseObject = JsonConvert.DeserializeObject<SessionDto>(response.Result);

            if (responseObject.Code != 200)
            {
                sessionId = null;
                return false;
            }

            sessionId = responseObject.SessionId;
            return true;
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException { StatusCode: HttpStatusCode.BadRequest })
            {
                HelpCommand.Instance.SendNotLoggedInHelp(source);
                sessionId = null;
                return false;
            }

            HelpCommand.Instance.UnexpectedErrorHelp(source);
            sessionId = null;
            return false;
        }
    }

    protected class SessionDto
    {
        public int Code { get; set; } = 0;
        public string SessionId { get; set; } = null;
    }
}