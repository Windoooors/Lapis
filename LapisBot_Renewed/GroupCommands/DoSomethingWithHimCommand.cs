using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;
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
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (System.IO.File.Exists(Environment.CurrentDirectory + "/groups.json"))
                Groups = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(
                    System.IO.File.ReadAllText(Environment.CurrentDirectory + "/groups.json"));

            SubCommands.Add(new GetHimFuckedCommand() { Groups = Groups, ParentCommand = this });
            SubCommands.Add(new MarryHimCommand() { Groups = Groups, ParentCommand = this });

            foreach (DoSomethingWithHimCommand doSomethingWithHimCommand in SubCommands)
                doSomethingWithHimCommand.Initialize();

            return Task.CompletedTask;
        }

        public override Task SubParse(string command, CqGroupMessagePostContext source)
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

        public bool IsGroupContainsMember(long groupId, long memberId)
        {
            var group = Program.Session.GetGroupMemberList(groupId);
            if (group == null)
                return false;
            var memberList = group.Members;

            foreach (var member in memberList)
            {
                if (member.UserId == memberId)
                    return true;
            }
            return false;
        }
        
        public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
        {
            if (Groups.ContainsKey(source.GroupId.ToString()))
            {
                var memberList = new List<string>();
                Groups.TryGetValue(source.GroupId.ToString(), out memberList);
                if (!memberList.Contains(source.Sender.UserId.ToString()) && source.Sender.UserId != 3358897233 && source.Sender.UserId != 2739176241)
                    memberList.Add(source.Sender.UserId.ToString());
                Groups.Remove(source.GroupId.ToString());
                Groups.Add(source.GroupId.ToString(), memberList);
            }
            else
                Groups.Add(source.GroupId.ToString(), new List<string>() { source.Sender.UserId.ToString() });

            foreach (DoSomethingWithHimCommand subCommand in SubCommands)
                subCommand.Groups = Groups;
            return Task.CompletedTask;
        }
    }
}
