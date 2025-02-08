using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using SixLabors.ImageSharp.Formats.Png;

namespace LapisBot_Renewed.GroupCommands
{
    public class MarryHimCommand : DoSomethingWithHimCommand
    {
        private Dictionary<string, List<KeyValuePair<string, string>>> _couplesInGroups =
            new Dictionary<string, List<KeyValuePair<string, string>>>();
        //public Dictionary<string, string> couples;

        private void Reload(object sender, EventArgs e)
        {
            if (Groups.Count != 0)
                File.WriteAllText(Environment.CurrentDirectory + "/groups.json",
                    JsonConvert.SerializeObject(Groups));
            Console.WriteLine("Data of groups have been saved.");
            
            Start();
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
            CoolDownTime = 5;
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

        /// <summary>
        /// Parses the command in the context of a group message to perform actions related to marrying someone within the group.
        /// </summary>
        /// <param name="command">The command string input by the user.</param>
        /// <param name="source">The context of the group message containing details like sender, group ID, etc.</param>
        /// <returns>A task representing the asynchronous operation of parsing and handling the marry command.</returns>
        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            if (Groups.Count != 0)
            {
                Random random = new Random();
                var memberList = new List<string>();
                Groups.TryGetValue(source.GroupId.ToString(), out memberList);
                if (_couplesInGroups.ContainsKey(source.GroupId.ToString()))
                {
                    var couples = new List<KeyValuePair<string, string>>();
                    _couplesInGroups.TryGetValue(source.GroupId.ToString(), out couples);
                    foreach (KeyValuePair<string, string> couple in couples)
                    {
                        if (couple.Key == source.Sender.UserId.ToString())
                        {
                            var message = new CqMessage();
                            if (IsGroupContainsMember(source.GroupId, long.Parse(couple.Value)))
                            {
                                var result =
                                    Program.Session.GetGroupMemberInformation(source.GroupId, long.Parse(couple.Value));
                                if (result == null)
                                    return Task.CompletedTask;

                                var memberName = "";

                                if (result.GroupNickname != "")
                                    memberName = result.GroupNickname;
                                else
                                    memberName = result.Nickname;

                                var image = Program.apiOperator.UrlToImage("https://q.qlogo.cn/g?b=qq&nk=" +
                                                                           couple.Value + "&s=640");
                                message =
                                    new CqMessage()
                                    {
                                        new CqReplyMsg(source.MessageId),
                                        new CqImageMsg("base64://" + image.ToBase64()),
                                        new CqTextMsg("您的对象是 "),
                                        new CqTextMsg(memberName + " (" + couple.Value + ") "),
                                        new CqTextMsg("！")
                                    };
                                
                                image.Dispose();
                            }
                            else
                            {
                                couples.Remove(couple);
                                _couplesInGroups.Remove(source.GroupId.ToString());
                                _couplesInGroups.Add(source.GroupId.ToString(), couples);
                                Parse(command, source);
                            }
                            
                            Program.Session.SendGroupMessageAsync(source.GroupId, message);
                            return Task.CompletedTask;
                        }
                    }
                    if (UnmarriedCount(couples, memberList) != 1 || (IsMarried(source.Sender.UserId.ToString(), couples) &&
                                                                     UnmarriedCount(couples, memberList) == 1))
                    {
                        var i = random.Next(0, memberList.Count);
                        if (UnmarriedCount(couples, memberList) > 1)
                        {
                            while (memberList[i] == source.Sender.UserId.ToString() || IsMarried(memberList[i], couples))
                            {
                                i = random.Next(0, memberList.Count);
                            }
                        }
                        
                        foreach (KeyValuePair<string, string> keyValuePair in couples)
                        {
                            if (keyValuePair.Value == source.Sender.UserId.ToString())
                            {
                                i = memberList.IndexOf(keyValuePair.Key);
                            }
                        }
                        if (IsGroupContainsMember(source.GroupId, long.Parse(memberList[i])))
                        {
                            var result =
                                Program.Session.GetGroupMemberInformation(source.GroupId, long.Parse(memberList[i]));
                            if (result == null)
                                return Task.CompletedTask; 
                            
                            var memberName = "";

                            if (result.GroupNickname != "")
                                memberName = result.GroupNickname;
                            else
                                memberName = result.Nickname;
                            
                            var message = new CqMessage();
                            var image = Program.apiOperator.UrlToImage("https://q.qlogo.cn/g?b=qq&nk=" +
                                                                       memberList[i] + "&s=640");
                            message =
                                new CqMessage()
                                {
                                    new CqReplyMsg(source.MessageId),
                                    new CqImageMsg("base64://" + image.ToBase64()),
                                    new CqTextMsg("您的对象是 "),
                                    new CqTextMsg(memberName + " (" + memberList[i] + ") "),
                                    new CqTextMsg("！")
                                };
                            
                            image.Dispose();

                            couples.Add(new KeyValuePair<string, string>(source.Sender.UserId.ToString(), memberList[i]));
                            _couplesInGroups.Remove(source.GroupId.ToString());
                            _couplesInGroups.Add(source.GroupId.ToString(), couples);
                            
                            Program.Session.SendGroupMessageAsync(source.GroupId, message);
                        }
                        else
                        {
                            memberList.RemoveAt(i);
                            Groups.Remove(source.GroupId.ToString());
                            Groups.Add(source.GroupId.ToString(), memberList);
                            Parse(command, source);
                        }
                    }
                    else
                    {
                        var message =
                            new CqMessage()
                            {
                                new CqReplyMsg(source.MessageId),
                                new CqTextMsg("近期发言人数太少咯 _(:_」∠)_ Lapis 找不到你的对象")
                            };

                        Program.Session.SendGroupMessageAsync(source.GroupId, message);
                    }
                }
                else
                {
                    _couplesInGroups.Add(source.GroupId.ToString(), new List<KeyValuePair<string, string>>());
                    Parse(command, source);
                }
            }

            return Task.CompletedTask;
        }
        
        public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
        {
            return Task.CompletedTask;
        }
    }
}