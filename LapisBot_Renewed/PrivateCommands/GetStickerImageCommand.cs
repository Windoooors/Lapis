using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace LapisBot_Renewed
{
    public class GetStickerImageCommand : PrivateCommand
    {
        public override Task Initialize()
        {
            return Task.CompletedTask;
        }

        public override Task ParseWithoutPreparse(string command, CqPrivateMessagePostContext source)
        {
            foreach (var element in source.Message)
            {
                if (!(element is CqImageMsg))
                    continue;
                Program.Session.SendPrivateMessage(source.Sender.UserId,
                [
                    new CqTextMsg("若 GIF 表情无法保存，您可以访问以下链接来下载该 GIF 表情：" + ((CqImageMsg)element).Url.ToString().Replace("https", "http")),
                    CqImageMsg.FromStream(Program.apiOperator.ImageToMemoryStream(((CqImageMsg)element).Url.ToString().Replace("https", "http")))
                ]);
            }
            return Task.CompletedTask;
        }
    }
}
