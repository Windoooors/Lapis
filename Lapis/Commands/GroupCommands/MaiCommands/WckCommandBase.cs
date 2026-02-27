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
    public static bool TryGetSessionId(CqMessagePostContext source, out string sessionId, bool sendHelp = true)
    {
        try
        {
            return TryGetSessionIdCore(source.UserId, out sessionId);
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException { StatusCode: HttpStatusCode.BadRequest })
            {
                if (sendHelp) HelpCommand.Instance.SendNotLoggedInHelp(source);
                sessionId = null;
                return false;
            }

            if (sendHelp) HelpCommand.Instance.UnexpectedErrorHelp(source);
            sessionId = null;
            return false;
        }
    }

    public static bool TryGetSessionId(long qqId, out string sessionId)
    {
        try
        {
            return TryGetSessionIdCore(qqId, out sessionId);
        }
        catch
        {
            sessionId = null;
            return false;
        }
    }

    private static bool TryGetSessionIdCore(long userId, out string sessionId)
    {
        var parameters = new Dictionary<string, string>
        {
            { "qq_id", userId.ToString() }
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

    protected class SessionDto
    {
        [JsonProperty] public int Code { get; set; }
        [JsonProperty] public string SessionId { get; set; }
    }
}