using System;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Xamarin.Forms.Internals;
using System.Numerics;

namespace LapisBot_Renewed
{
    public class MarryHimCommand : DoSomethingWithHimCommand
    {
        public Dictionary<string, List<KeyValuePair<string, string>>> couplesInGroups = new Dictionary<string, List<KeyValuePair<string, string>>>();
        //public Dictionary<string, string> couples;

        private void Reload(object sender, EventArgs e)
        {
            Start();
            if (groups.Count != 0)
                System.IO.File.WriteAllText(Environment.CurrentDirectory + "/groups.json", JsonConvert.SerializeObject(groups));
            Console.WriteLine("Data of groups have been saved.");
        }

        public void Start()
        {
            couplesInGroups.Clear();
        }

        public override void Initialize()
        {
            headCommand = new Regex(@"^娶群友$|^娶$|^嫁$");
            Program.DateChanged += Reload;
            Start();
            if (System.IO.File.Exists(Environment.CurrentDirectory + "/couples.json"))
            {
                couplesInGroups = JsonConvert.DeserializeObject<Dictionary<string, List<KeyValuePair<string, string>>>>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/couples.json"));
            }
        }

        public override void Unload()
        {
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "/couples.json", JsonConvert.SerializeObject(couplesInGroups));
            Console.WriteLine("Couples data have been saved.");
        }

        public override void ParseWithoutPreparse(string command, GroupMessageReceiver source)
        {

        }

        int unmarriedCount(List<KeyValuePair<string, string>> couples, List<string> memberList)
        {
            int count = memberList.Count;
            foreach (KeyValuePair<string, string> couple in couples)
            {
                count = count - 2;
                foreach (KeyValuePair<string, string> _couple in couples)
                {
                    if (couple.Key == _couple.Value)
                        count = count + 1;
                }
            }
            return count;
        }

        bool isMarried(string id, List<KeyValuePair<string,string>> couples)
        {
            foreach (KeyValuePair<string, string> keyValuePair in couples)
            {
                if (keyValuePair.Value == id || keyValuePair.Key == id)
                {
                    return true;
                }
            }
            return false;
        }
        
        public override void Parse(string command, GroupMessageReceiver source)
        {
            if (groups.Count != 0)
            {
                Random random = new Random();
                var _memberList = new List<string>();
                groups.TryGetValue(source.GroupId, out _memberList);
                if (couplesInGroups.ContainsKey(source.GroupId))
                {
                    var couples = new List<KeyValuePair<string, string>>();
                    couplesInGroups.TryGetValue(source.GroupId, out couples);
                    foreach (KeyValuePair<string, string> couple in couples)
                    {
                        if (couple.Key == source.Sender.Id)
                        {
                            var message = new MessageChain();
                            try
                            {
                                var memberName = GroupManager.GetMemberAsync(couple.Value, source.GroupId).Result.Name;
                                if (!OperatingSystem.IsMacOS())
                                {
                                    var image = Program.apiOperator.ImageToBase64("https://q.qlogo.cn/g?b=qq&nk=" + couple.Value + "&s=640");
                                    message = new MessageChain() {
                                        new AtMessage(){ Target = source.Sender.Id },
                                        new ImageMessage(){ Base64 = image },
                                        new PlainMessage("您的对象是 "),
                                        new PlainMessage(memberName + " (" + couple.Value + ") "),
                                        new PlainMessage("！") };
                                }
                                else
                                {
                                    message = new MessageChain() {
                                        new AtMessage(){ Target = source.Sender.Id },
                                        new PlainMessage(" 您的对象是 "),
                                        new PlainMessage(memberName + " (" + couple.Value + ") "),
                                        new PlainMessage("！") };
                                }
                            }
                            catch
                            {
                                couples.Remove(couple);
                                couplesInGroups.Remove(source.GroupId);
                                couplesInGroups.Add(source.GroupId, couples);
                                Parse(command, source);
                            }
                            MessageManager.SendGroupMessageAsync(source.GroupId, message);
                            return;
                        }
                    }
                    if (unmarriedCount(couples, _memberList) != 1 || (isMarried(source.Sender.Id, couples) && unmarriedCount(couples, _memberList) == 1))
                    {
                        var i = random.Next(0, _memberList.Count);
                        if (unmarriedCount(couples, _memberList) > 1)
                        {
                            while (_memberList[i] == source.Sender.Id || isMarried(_memberList[i], couples))
                            {
                                i = random.Next(0, _memberList.Count);
                            }
                        }
                        if (source.Sender.Id == "2794813909" && _memberList.Contains("2801417957"))
                            i = _memberList.IndexOf("2801417957");
                        if (source.Sender.Id == "2801417957" && _memberList.Contains("2794813909"))
                            i = _memberList.IndexOf("2794813909");
                        foreach (KeyValuePair<string, string> keyValuePair in couples)
                        {
                            if (keyValuePair.Value == source.Sender.Id)
                            {
                                i = _memberList.IndexOf(keyValuePair.Key);
                            }
                        }
                        try
                        {
                            var memberName = GroupManager.GetMemberAsync(_memberList[i], source.GroupId).Result.Name;
                            var message = new MessageChain();
                            if (!OperatingSystem.IsMacOS())
                            {
                                var image = Program.apiOperator.ImageToBase64("https://q.qlogo.cn/g?b=qq&nk=" + _memberList[i] + "&s=640");
                                message = new MessageChain() {
                                new AtMessage(){ Target = source.Sender.Id },
                                    new ImageMessage(){ Base64 = image },
                                    new PlainMessage("您的对象是 "),
                                    new PlainMessage(memberName + " (" + _memberList[i] + ") "),
                                    new PlainMessage("！") };
                            }
                            else
                            {
                                message = new MessageChain() {
                                    new AtMessage(){ Target = source.Sender.Id },
                                    new PlainMessage(" 您的对象是 "),
                                    new PlainMessage(memberName + " (" + _memberList[i] + ") "),
                                    new PlainMessage("！") };
                            }
                            couples.Add(new KeyValuePair<string, string>(source.Sender.Id, _memberList[i]));
                            couplesInGroups.Remove(source.GroupId);
                            couplesInGroups.Add(source.GroupId, couples);
                            MessageManager.SendGroupMessageAsync(source.GroupId, message);
                        }
                        catch
                        {
                            _memberList.RemoveAt(i);
                            groups.Remove(source.GroupId);
                            groups.Add(source.GroupId, _memberList);
                            Parse(command, source);
                        }
                    }
                    else
                    {
                        var message = new MessageChain() {
                            new AtMessage(){ Target = source.Sender.Id },
                            new PlainMessage(" 近期发言人数太少咯 _(:_」∠)_ Lapis 找不到你的对象") };
                        MessageManager.SendGroupMessageAsync(source.GroupId, message);
                    }
                }
                else
                {
                    couplesInGroups.Add(source.GroupId, new List<KeyValuePair<string, string>>());
                    Parse(command, source);
                }
            }
        }
    }
}