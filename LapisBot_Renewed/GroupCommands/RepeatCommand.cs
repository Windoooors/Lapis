using System;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Data.Messages;

namespace LapisBot_Renewed
{
    public class RepeatCommand : GroupCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"^repeat\s");
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            MessageManager.SendGroupMessageAsync(source.GroupId, command);
        }
    }
}
