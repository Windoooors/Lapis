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
    public class HelpCommand : GroupCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"^help$");
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" http://wiki.setchin.com") });
        }
    }
}
