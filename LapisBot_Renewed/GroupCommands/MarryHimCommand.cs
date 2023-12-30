using System;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

namespace LapisBot_Renewed.GroupCommands
{
    public class MarryHimCommand : DoSomethingWithHimCommand
    {
        private Dictionary<string, List<KeyValuePair<string, string>>> _couplesInGroups =
            new Dictionary<string, List<KeyValuePair<string, string>>>();
        //public Dictionary<string, string> couples;

        private void Reload(object sender, EventArgs e)
        {
            Start();
            if (Groups.Count != 0)
                System.IO.File.WriteAllText(Environment.CurrentDirectory + "/groups.json",
                    JsonConvert.SerializeObject(Groups));
            Console.WriteLine("Data of groups have been saved.");
        }

        private void Start()
        {
            _couplesInGroups.Clear();
        }

        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^娶群友$|^娶$|^嫁$");
            DirectCommand = new Regex(@"^娶群友$|^娶$|^嫁$");
            DefaultSettings.SettingsName = "娶群友";
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

            Program.DateChanged += Reload;
            Start();
            if (System.IO.File.Exists(Environment.CurrentDirectory + "/couples.json"))
            {
                _couplesInGroups =
                    JsonConvert.DeserializeObject<Dictionary<string, List<KeyValuePair<string, string>>>>(
                        System.IO.File.ReadAllText(Environment.CurrentDirectory + "/couples.json"));
            }

            return Task.CompletedTask;
        }

        public override Task Unload()
        {
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "/couples.json",
                JsonConvert.SerializeObject(_couplesInGroups));
            Console.WriteLine("Couples data have been saved.");
            return Task.CompletedTask;
        }

        int UnmarriedCount(List<KeyValuePair<string, string>> couples, List<string> memberList)
        {
            int count = memberList.Count;
            foreach (KeyValuePair<string, string> keyCouple in couples)
            {
                count = count - 2;
                foreach (KeyValuePair<string, string> valueCouple in couples)
                {
                    if (keyCouple.Key == valueCouple.Value)
                        count = count + 1;
                }
            }

            return count;
        }

        bool IsMarried(string id, List<KeyValuePair<string, string>> couples)
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

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            if (Groups.Count != 0)
            {
                Random random = new Random();
                var memberList = new List<string>();
                Groups.TryGetValue(source.GroupId, out memberList);
                if (_couplesInGroups.ContainsKey(source.GroupId))
                {
                    var couples = new List<KeyValuePair<string, string>>();
                    _couplesInGroups.TryGetValue(source.GroupId, out couples);
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
                                    var image = Program.apiOperator.ImageToBase64("https://q.qlogo.cn/g?b=qq&nk=" +
                                        couple.Value + "&s=640");
                                    message = new MessageChain()
                                    {
                                        new AtMessage() { Target = source.Sender.Id },
                                        new ImageMessage() { Base64 = image },
                                        new PlainMessage("您的对象是 "),
                                        new PlainMessage(memberName + " (" + couple.Value + ") "),
                                        new PlainMessage("！")
                                    };
                                }
                                else
                                {
                                    message = new MessageChain()
                                    {
                                        new AtMessage() { Target = source.Sender.Id },
                                        new PlainMessage(" 您的对象是 "),
                                        new PlainMessage(memberName + " (" + couple.Value + ") "),
                                        new PlainMessage("！")
                                    };
                                }
                            }
                            catch
                            {
                                couples.Remove(couple);
                                _couplesInGroups.Remove(source.GroupId);
                                _couplesInGroups.Add(source.GroupId, couples);
                                Parse(command, source);
                            }

                            MessageManager.SendGroupMessageAsync(source.GroupId, message);
                            return Task.CompletedTask;
                        }
                    }

                    if (UnmarriedCount(couples, memberList) != 1 || (IsMarried(source.Sender.Id, couples) &&
                                                                     UnmarriedCount(couples, memberList) == 1))
                    {
                        var i = random.Next(0, memberList.Count);
                        if (UnmarriedCount(couples, memberList) > 1)
                        {
                            while (memberList[i] == source.Sender.Id || IsMarried(memberList[i], couples))
                            {
                                i = random.Next(0, memberList.Count);
                            }
                        }

                        if (source.Sender.Id == "2794813909" && memberList.Contains("2801417957"))
                            i = memberList.IndexOf("2801417957");
                        if (source.Sender.Id == "2801417957" && memberList.Contains("2794813909"))
                            i = memberList.IndexOf("2794813909");
                        foreach (KeyValuePair<string, string> keyValuePair in couples)
                        {
                            if (keyValuePair.Value == source.Sender.Id)
                            {
                                i = memberList.IndexOf(keyValuePair.Key);
                            }
                        }

                        try
                        {
                            var memberName = GroupManager.GetMemberAsync(memberList[i], source.GroupId).Result.Name;
                            var message = new MessageChain();
                            if (!OperatingSystem.IsMacOS())
                            {
                                var image = Program.apiOperator.ImageToBase64("https://q.qlogo.cn/g?b=qq&nk=" +
                                                                              memberList[i] + "&s=640");
                                message = new MessageChain()
                                {
                                    new AtMessage() { Target = source.Sender.Id },
                                    new ImageMessage() { Base64 = image },
                                    new PlainMessage("您的对象是 "),
                                    new PlainMessage(memberName + " (" + memberList[i] + ") "),
                                    new PlainMessage("！")
                                };
                            }
                            else
                            {
                                message = new MessageChain()
                                {
                                    new AtMessage() { Target = source.Sender.Id },
                                    new PlainMessage(" 您的对象是 "),
                                    new PlainMessage(memberName + " (" + memberList[i] + ") "),
                                    new PlainMessage("！")
                                };
                            }

                            couples.Add(new KeyValuePair<string, string>(source.Sender.Id, memberList[i]));
                            _couplesInGroups.Remove(source.GroupId);
                            _couplesInGroups.Add(source.GroupId, couples);
                            MessageManager.SendGroupMessageAsync(source.GroupId, message);
                        }
                        catch
                        {
                            memberList.RemoveAt(i);
                            Groups.Remove(source.GroupId);
                            Groups.Add(source.GroupId, memberList);
                            Parse(command, source);
                        }
                    }
                    else
                    {
                        var message = new MessageChain()
                        {
                            new AtMessage() { Target = source.Sender.Id },
                            new PlainMessage(" 近期发言人数太少咯 _(:_」∠)_ Lapis 找不到你的对象")
                        };
                        MessageManager.SendGroupMessageAsync(source.GroupId, message);
                    }
                }
                else
                {
                    _couplesInGroups.Add(source.GroupId, new List<KeyValuePair<string, string>>());
                    Parse(command, source);
                }
            }

            return Task.CompletedTask;
        }
    }
}