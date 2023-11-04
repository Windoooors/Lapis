using System;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;

namespace LapisBot_Renewed
{
    public class QuestionMarkRepeatCommand : GroupCommand
    {
        public override void Initialize()
        {
            //headCommand = new Regex(@"^？|^?|^¿");
        }

        public override void ParseWithoutPreparse(string command, GroupMessageReceiver source)
        {
            switch (command)
            {
                case "?":
                    MessageManager.SendGroupMessageAsync(source.GroupId, "¿");
                    break;
                case "？":
                    MessageManager.SendGroupMessageAsync(source.GroupId, "¿");
                    break;
                case "¿":
                    MessageManager.SendGroupMessageAsync(source.GroupId, "?");
                    break;
            }
        }
    }
}
