using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;

namespace Lapis.Commands.PrivateCommands;

public class BilibiliForwardParserCommand : PrivateCommand
{
    public override void RespondWithoutParsingCommand(string command, CqPrivateMessagePostContext source)
    {
        foreach (var item in source.Message)
            if (item is CqJsonMsg jsonMsg)
                if (GetUrl(jsonMsg.Data, out var url))
                    SendMessage(source, [
                        new CqReplyMsg(source.MessageId),
                        url
                    ]);
    }

    private bool GetUrl(string jsonString, out string url)
    {
        try
        {
            var parsedResult = JsonConvert.DeserializeObject<JsonMessageDto>(jsonString);

            var result = parsedResult.Meta.Detail.Url is not (null or "");

            url = parsedResult.Meta.Detail.Url;
            return result;
        }
        catch
        {
            url = null;
            return false;
        }
    }

    private class JsonMessageDto
    {
        [JsonProperty("meta")] public MetaDto Meta { get; set; }

        public class MetaDto
        {
            [JsonProperty("detail_1")] public DetailDto Detail { get; set; }

            public class DetailDto
            {
                [JsonProperty("qqdocurl")] public string Url { get; set; }
            }
        }
    }
}