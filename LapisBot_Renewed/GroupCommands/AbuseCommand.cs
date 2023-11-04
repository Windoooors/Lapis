using System;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data;
using Mirai.Net.Sessions.Http.Managers;

namespace LapisBot_Renewed
{
    public class AbuseCommand : GroupCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"^骂我$");
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 别骂了 好好过好每一天吧 ( ´▽｀)\nGreetings from Lapis Build 2023072903!\n二号机账号由 桃子亲 （3231743481） 提供 ( ´▽｀)") });
        }
    }
}
