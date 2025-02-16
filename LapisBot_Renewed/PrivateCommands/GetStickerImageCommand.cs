using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;

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
                    new CqMessage
                    {
                        new CqTextMsg("请访问以下链接来下载该表情：" +
                                      ((CqImageMsg)element).Url.ToString().Replace("https", "http")),
                    });
            }
            
            return Task.CompletedTask;
        }
    }
}
