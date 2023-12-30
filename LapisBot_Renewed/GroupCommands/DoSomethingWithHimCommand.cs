using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class DoSomethingWithHimCommand : GroupCommand
    {
        public Dictionary<string, List<string>> Groups = new Dictionary<string, List<string>>();

        public override Task Initialize()
        {
            Groups.Clear();
            SubCommands.Clear();
            HeadCommand = new Regex("");
            DefaultSettings.SettingsName = "";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (System.IO.File.Exists(Environment.CurrentDirectory + "/groups.json"))
                Groups = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/groups.json"));

            SubCommands.Add(new GetHimFuckedCommand() { Groups = Groups });
            SubCommands.Add(new MarryHimCommand() { Groups = Groups });

            foreach (DoSomethingWithHimCommand doSomethingWithHimCommand in SubCommands)
            {
                doSomethingWithHimCommand.Initialize();
                doSomethingWithHimCommand.ParentCommand = this;
            }
            return Task.CompletedTask;
        }

        public override Task SubParse(string command, GroupMessageReceiver source)
        {
            return Task.CompletedTask;
        }

        public override Task Unload()
        {
            if (Groups.Count != 0)
                System.IO.File.WriteAllText(Environment.CurrentDirectory + "/groups.json", JsonConvert.SerializeObject(Groups));
            foreach (DoSomethingWithHimCommand subCommand in SubCommands)
                subCommand.Unload();
            Console.WriteLine("Data of groups have been saved.");
            return Task.CompletedTask;
        }

        public override Task RespondWithoutParsingCommand(string command, GroupMessageReceiver source)
        {
            if (Groups.ContainsKey(source.GroupId))
            {
                var memberList = new List<string>();
                Groups.TryGetValue(source.GroupId, out memberList);
                if (!memberList.Contains(source.Sender.Id) && source.Sender.Id != "3358897233" && source.Sender.Id != "2739176241")
                    memberList.Add(source.Sender.Id);
                Groups.Remove(source.GroupId);
                Groups.Add(source.GroupId, memberList);
            }
            else
                Groups.Add(source.GroupId, new List<string>() { source.Sender.Id });

            foreach (DoSomethingWithHimCommand subCommand in SubCommands)
                subCommand.Groups = Groups;
            return Task.CompletedTask;
        }
    }
}
