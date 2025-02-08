using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class HelpCommand : GroupCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^help$");
            DirectCommand = new Regex(@"^help$");
            DefaultSettings.SettingsName = "帮助";
                        CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings");
                
            settingsList = Directory.GetFiles(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings")
                                    .Select(path => JsonConvert.DeserializeObject<GroupCommandSettings>(File.ReadAllText(path)))
                                    .ToList();
            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {

            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("请访问链接以查询 Lapis 的使用方法：https://www.setchin.com/lapis_docs.html")
            });
            return Task.CompletedTask;
        }

        public Task CoolDownParse(string command, CqGroupMessagePostContext source, DateTime dateTime)
        {

            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("使用太频繁啦！请等待 " + (dateTime - DateTime.Now).Seconds + " 秒后再试")
            });
            return Task.CompletedTask;
        }
    }
}
