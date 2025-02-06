using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace LapisBot_Renewed
{
    public class GetGroupsCommand : PrivateCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^groups$");
            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqPrivateMessagePostContext source)
        {
            if (source.Sender.UserId == 2794813909)
            {
                var message = string.Empty;
                var result = Program.Session.GetGroupList();
                if (result == null)
                    return Task.CompletedTask;
                foreach (CqGroup group in result.Groups)
                    message += group.GroupId + " - " + group.GroupName + "\n";
                Program.Session.SendPrivateMessage(source.Sender.UserId, new CqMessage
                    { message.TrimEnd() });
            }
            return Task.CompletedTask;
        }
    }
}
