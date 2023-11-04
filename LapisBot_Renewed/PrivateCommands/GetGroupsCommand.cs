using System;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using System.Text.RegularExpressions;

namespace LapisBot_Renewed
{
    public class GetGroupsCommand : PrivateCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"^groups$");
        }

        public override void Parse(string command, FriendMessageReceiver source)
        {
            if (source.FriendId == "2794813909")
            {
                var message = string.Empty;
                foreach (Mirai.Net.Data.Shared.Group group in Program.bot.Groups.Value)
                    message += group.Id + " - " + group.Name + "\n";
                MessageManager.SendFriendMessageAsync(source.Sender, message.TrimEnd());
            }
        }
    }
}
