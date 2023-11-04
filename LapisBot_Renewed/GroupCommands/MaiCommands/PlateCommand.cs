using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Manganese.Text;

namespace LapisBot_Renewed
{
    public class PlateCommand : MaiCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"是什么将");
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            MessageManager.SendGroupMessageAsync(source.GroupId, "你是我的欧尼将🥺");
        }
    }
}
