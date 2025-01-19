using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using mcswlib.ServerStatus;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Client;
using System.IO;
using mcswlib.ServerStatus.ServerInfo;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace LapisBot_Renewed.GroupCommands
{
    public class MotdDto
    {
        [JsonProperty("text")] public string Text;
    }

    public class McPingCommand : GroupCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^mcping\s");
            DirectCommand = new Regex(@"^mcping\s|^ping\s");
            DefaultSettings.SettingsName = "Minecraft 查询";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings");
            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                                       " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        public override async Task Parse(string command, CqGroupMessagePostContext source)
        {
            try
            {
                var address = command.Split(":")[0];
                var port = ushort.Parse(command.Split(":")[1]);
                var factory = new ServerStatusFactory();
                var inst = factory.Make(address, port);
                var res = inst.Updater;
                //Console.WriteLine("Result: " + res.Ping());
                await (
                    Program.Session.SendGroupMessageAsync(source.GroupId, ServerInformationMessage(res.Ping(), source.Sender)));
            }
            catch
            {
                try
                {
                    var client = new ClientRequest("8.8.8.8");
                    client.Questions.Add(new Question(Domain.FromString("_minecraft._tcp." + command), RecordType.SRV));
                    client.RecursionDesired = true;
                    var response = await client.Resolve();
                    var record = (ServiceResourceRecord)response.AnswerRecords[0];
                    var factory = new ServerStatusFactory();
                    var inst = factory.Make(record.Target.ToString(), record.Port);
                    var res = inst.Updater.Ping();
                    //Console.WriteLine("Result: " + res.Ping());
                    await (
                        Program.Session.SendGroupMessageAsync(source.GroupId, ServerInformationMessage(res, source.Sender)));
                }
                catch
                {
                    await (
                        Program.Session.SendGroupMessageAsync(source.GroupId,
                            [new CqReplyMsg(source.MessageId), new CqTextMsg("未找到该服务器")]));
                }
            }
        }

        private readonly Regex _regex = new Regex("§.");

        private CqMessage ServerInformationMessage(ServerInfoBase server, CqMessageSender sender)
        {
            //var _information = server.Ping();
            var plainMessage = new CqTextMsg(_regex.Replace(server.RawMotd, string.Empty) + "\n" + "版本：" +
                                             _regex.Replace(server.MinecraftVersion, string.Empty) + "\n" + "人数：" +
                                             server.CurrentPlayerCount + "/" + server.MaxPlayerCount);

            var messageChain = new CqMessage()
            {
                new CqAtMsg(sender.UserId), new CqTextMsg(""),
                //new CqImageMsg("base64://" + ImgToBase64String(server.FavIcon)), plainMessage
            };
            return messageChain;
        }
    }
}
