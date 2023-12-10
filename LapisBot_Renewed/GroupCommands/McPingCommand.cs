using System;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using mcswlib.ServerStatus;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using DNS;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Client;
using System.IO;
using Microsoft.VisualBasic;
using mcswlib.ServerStatus.ServerInfo;
using System.Drawing;
using System.Buffers.Text;

namespace LapisBot_Renewed
{
    public class MotdDto
    {
        [JsonProperty("text")]
        public string Text;
    }

    public class McPingCommand : GroupCommand
    {
        public override Task Initialize()
        {
            headCommand = new Regex(@"^mcping\s");
            directCommand = new Regex(@"^mcping\s|^ping\s");
            defaultSettings.SettingsName = "Minecraft 查询";
            _groupCommandSettings = defaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings");
            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
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
            await MessageManager.SendGroupMessageAsync(source.GroupId, ServerInformationMessage(res.Ping(), source.Sender));
            }
            catch
            {
                try
                {
                    var client = new ClientRequest("8.8.8.8");
                    client.Questions.Add(new Question(Domain.FromString("_minecraft._tcp." + command), RecordType.SRV));
                    client.RecursionDesired = true;
                    IResponse response = await client.Resolve();
                    var record = (ServiceResourceRecord)response.AnswerRecords[0];
                    var factory = new ServerStatusFactory();
                    var inst = factory.Make(record.Target.ToString(), record.Port);
                    var res = inst.Updater.Ping();
                    //Console.WriteLine("Result: " + res.Ping());
                    await MessageManager.SendGroupMessageAsync(source.GroupId, ServerInformationMessage(res, source.Sender));
                }
                catch
                {
                    await MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该服务器") });
                }
            }
        }

        Regex regex = new Regex("§.");

        public MessageChain ServerInformationMessage(ServerInfoBase server, Mirai.Net.Data.Shared.Member sender)
        {
            //var _information = server.Ping();
            var plainMessage = new PlainMessage(regex.Replace(server.RawMotd, string.Empty) + "\n" + "版本：" + regex.Replace(server.MinecraftVersion, string.Empty) + "\n" + "人数：" + server.CurrentPlayerCount + "/" + server.MaxPlayerCount);
            var _stream = new MemoryStream();

            var messageChain = new MessageChain() { new AtMessage(sender.Id), new PlainMessage(" "), new ImageMessage() { Base64 = ImgToBase64String(server.FavIcon) }, plainMessage };
            return messageChain;
        }

        public static string ImgToBase64String(Bitmap bmp)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
