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
using MineStatLib;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using DNS;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Client;

namespace LapisBot_Renewed
{
    public class MotdDto
    {
        [JsonProperty("text")]
        public string Text;
    }

    public class McPingCommand : GroupCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"^mcping\s");
        }

        public override async void Parse(string command, GroupMessageReceiver source)
        {
            try
            {
                var address = command.Split(":")[0];
                var port = ushort.Parse(command.Split(":")[1]);
                var server = new MineStat(address, port);
                await MessageManager.SendGroupMessageAsync(source.GroupId, ServerInformationMessage(server, source.Sender));
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
                    var server = new MineStat(record.Target.ToString(), record.Port);
                    await MessageManager.SendGroupMessageAsync(source.GroupId, ServerInformationMessage(server, source.Sender));
                }
                catch
                {
                    await MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 未找到该服务器") });
                }
            }
        }

        Regex regex = new Regex("§.");

        public MessageChain ServerInformationMessage(MineStat server, Mirai.Net.Data.Shared.Member sender)
        {
            var plainMessage = new PlainMessage(regex.Replace(JsonConvert.DeserializeObject<MotdDto>(server.Motd).Text, string.Empty) + "\n" + "版本：" + regex.Replace(server.Version, string.Empty) + "\n" + "人数：" + server.CurrentPlayers + "/" + server.MaximumPlayers);
            var messageChain = new MessageChain() { new AtMessage(sender.Id), new PlainMessage(" "), new ImageMessage() { Path = Program.apiOperator.BytesToPng(Environment.CurrentDirectory + "/temp", "servericon.png", server.FaviconBytes) }, plainMessage };
            return messageChain;
        }
    }
}
