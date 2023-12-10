using System;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LapisBot_Renewed
{
    public class GetGroupsCommand : PrivateCommand
    {
        public override Task Initialize()
        {
            headCommand = new Regex(@"^groups$");
            return Task.CompletedTask;
        }

        public override Task Parse(string command, FriendMessageReceiver source)
        {
            if (source.FriendId == "2794813909")
            {
                var message = string.Empty;
                foreach (Mirai.Net.Data.Shared.Group group in Program.bot.Groups.Value)
                    message += group.Id + " - " + group.Name + "\n";
                MessageManager.SendFriendMessageAsync(source.Sender, message.TrimEnd());
            }
            return Task.CompletedTask;
        }
    }
}
