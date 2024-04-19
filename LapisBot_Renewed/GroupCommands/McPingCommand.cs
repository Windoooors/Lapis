using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using mcswlib.ServerStatus;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Client;
using System.IO;
using mcswlib.ServerStatus.ServerInfo;
using System.Drawing;

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

        public override async Task Parse(string command, GroupMessageReceiver source)
        {
            try
            {
                var address = command.Split(":")[0];
                var port = ushort.Parse(command.Split(":")[1]);
                var factory = new ServerStatusFactory();
                var inst = factory.Make(address, port);
                var res = inst.Updater;
                //Console.WriteLine("Result: " + res.Ping());
                await MessageManager.SendGroupMessageAsync(source.GroupId,
                    ServerInformationMessage(res.Ping(), source.Sender));
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
                    await MessageManager.SendGroupMessageAsync(source.GroupId,
                        ServerInformationMessage(res, source.Sender));
                }
                catch
                {
                    await MessageManager.SendGroupMessageAsync(source.GroupId,
                        new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该服务器") });
                }
            }
        }

        private readonly Regex _regex = new Regex("§.");

        private MessageChain ServerInformationMessage(ServerInfoBase server, Mirai.Net.Data.Shared.Member sender)
        {
            //var _information = server.Ping();
            var plainMessage = new PlainMessage(_regex.Replace(server.RawMotd, string.Empty) + "\n" + "版本：" +
                                                _regex.Replace(server.MinecraftVersion, string.Empty) + "\n" + "人数：" +
                                                server.CurrentPlayerCount + "/" + server.MaxPlayerCount);

            var messageChain = new MessageChain()
            {
                new AtMessage(sender.Id), new PlainMessage(" "),
                new ImageMessage() { Base64 = ImgToBase64String(server.FavIcon) }, plainMessage
            };
            return messageChain;
        }

        private static string ImgToBase64String(Image bmp)
        {
            try
            {
                var ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                var arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch
            {
                return null;
            }
        }
    }
}
