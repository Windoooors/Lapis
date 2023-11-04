using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Newtonsoft.Json;

namespace LapisBot_Renewed
{
    public class DoSomethingWithHimCommand : GroupCommand
    {
        public Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
        public Dictionary<string, DoSomethingWithHimCommand> subCommands = new Dictionary<string, DoSomethingWithHimCommand>();

        public override void Initialize()
        {
            groups.Clear();
            subCommands.Clear();
            headCommand = new Regex("");

            if (System.IO.File.Exists(Environment.CurrentDirectory + "/groups.json"))
                groups = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/groups.json"));

            subCommands.Add("fuck", new GetHimFuckedCommand() { groups = groups });
            subCommands.Add("marry", new MarryHimCommand() { groups = groups });

            foreach (KeyValuePair<string, DoSomethingWithHimCommand> doSomethingWithHimCommand in subCommands)
                doSomethingWithHimCommand.Value.Initialize();
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            foreach (KeyValuePair<string, DoSomethingWithHimCommand> subCommand in subCommands)
            {
                if (subCommand.Value.headCommand != null && subCommand.Value.headCommand.IsMatch(command))
                {
                    subCommand.Value.Parse(command, source, false);
                    subCommand.Value.Parse(command, source);
                }
                else if (subCommand.Value.subHeadCommand != null && subCommand.Value.subHeadCommand.IsMatch(command))
                {
                    subCommand.Value.Parse(command, source, true);
                }
            }
        }

        public override void Unload()
        {
            if (groups.Count != 0)
                System.IO.File.WriteAllText(Environment.CurrentDirectory + "/groups.json", JsonConvert.SerializeObject(groups));
            foreach (KeyValuePair<string, DoSomethingWithHimCommand> subCommand in subCommands)
                subCommand.Value.Unload();
            Console.WriteLine("Data of groups have been saved.");
        }

        public override void ParseWithoutPreparse(string command, GroupMessageReceiver source)
        {
            if (groups.ContainsKey(source.GroupId))
            {
                var memberList = new List<string>();
                groups.TryGetValue(source.GroupId, out memberList);
                if (!memberList.Contains(source.Sender.Id) && source.Sender.Id != "3358897233" && source.Sender.Id != "2739176241")
                    memberList.Add(source.Sender.Id);
                groups.Remove(source.GroupId);
                groups.Add(source.GroupId, memberList);
            }
            else
                groups.Add(source.GroupId, new List<string>() { source.Sender.Id });

            foreach (KeyValuePair<string, DoSomethingWithHimCommand> subCommand in subCommands)
                subCommand.Value.groups = groups;
        }
    }
}
