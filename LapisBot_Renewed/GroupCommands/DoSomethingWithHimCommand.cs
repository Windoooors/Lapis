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

        public override Task Initialize()
        {
            groups.Clear();
            subCommands.Clear();
            headCommand = new Regex("");
            defaultSettings.SettingsName = "";
            _groupCommandSettings = defaultSettings.Clone();
            if (System.IO.File.Exists(Environment.CurrentDirectory + "/groups.json"))
                groups = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/groups.json"));

            subCommands.Add(new GetHimFuckedCommand() { groups = groups });
            subCommands.Add(new MarryHimCommand() { groups = groups });

            foreach (DoSomethingWithHimCommand doSomethingWithHimCommand in subCommands)
            {
                doSomethingWithHimCommand.Initialize();
                doSomethingWithHimCommand.parentCommand = this;
            }
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source, bool isSubParse)
        {
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            foreach (DoSomethingWithHimCommand subCommand in subCommands)
            {
                if (subCommand.headCommand != null && subCommand.headCommand.IsMatch(command))
                {
                    command = subCommand.headCommand.Replace(command, "");
                    subCommand.PreParse(command, source);
                    return Task.CompletedTask;
                }
                else if (subCommand.subHeadCommand != null && subCommand.subHeadCommand.IsMatch(command))
                {
                    command = subCommand.subHeadCommand.Replace(command, "");
                    subCommand.PreParse(command, source, true);
                    return Task.CompletedTask;
                }
            }
            Program.helpCommand.Parse("", source);
            return Task.CompletedTask;
        }

        public override Task Unload()
        {
            if (groups.Count != 0)
                System.IO.File.WriteAllText(Environment.CurrentDirectory + "/groups.json", JsonConvert.SerializeObject(groups));
            foreach (DoSomethingWithHimCommand subCommand in subCommands)
                subCommand.Unload();
            Console.WriteLine("Data of groups have been saved.");
            return Task.CompletedTask;
        }

        public override Task ParseWithoutPreparse(string command, GroupMessageReceiver source)
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

            foreach (DoSomethingWithHimCommand subCommand in subCommands)
                subCommand.groups = groups;
            return Task.CompletedTask;
        }
    }
}
