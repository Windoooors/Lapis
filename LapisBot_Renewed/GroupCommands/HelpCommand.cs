using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Settings;

namespace LapisBot_Renewed.GroupCommands
{
    public class HelpCommand : GroupCommand
    {
        public static HelpCommand Instance;

        public HelpCommand()
        {
            CommandHead = new Regex("^help");
            DirectCommandHead = new Regex("^help");
            ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("help", "1");
            Instance = this;
        }

        public override Task Parse(CqGroupMessagePostContext source)
        {
            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("请访问链接以查询 Lapis 的使用方法：https://www.setchin.com/lapis_docs.html")
            });
            return Task.CompletedTask;
        }
        
        public void ArgumentErrorHelp(CqGroupMessagePostContext source)
        {
            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("参数错误\n请访问链接以查询 Lapis 的使用方法：https://www.setchin.com/lapis_docs.html")
            });
        }
        
        public void UnexpectedErrorHelp(CqGroupMessagePostContext source)
        {
            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("出现了未知的错误")
            });
        }
    }
}
