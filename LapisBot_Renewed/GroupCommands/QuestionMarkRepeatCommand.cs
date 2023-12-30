using System;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;

namespace LapisBot_Renewed.GroupCommands
{
    public class QuestionMarkRepeatCommand : GroupCommand
    {
        public override Task Initialize()
        {
            //headCommand = new Regex(@"^？|^?|^¿");
            return Task.CompletedTask;
        }

        public override Task RespondWithoutParsingCommand(string command, GroupMessageReceiver source)
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
            return Task.CompletedTask;
        }
    }
}
