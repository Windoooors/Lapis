using System;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using DNS.Protocol;
using System.Threading.Tasks;
using System.IO;

namespace LapisBot_Renewed.GroupCommands
{
    public class GetHimFuckedCommand : DoSomethingWithHimCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^透群友$|^透$|^日$|^操$|^干$|^日批$");
            SubHeadCommand = new Regex(@"^日\s");
            DirectCommand = new Regex(@"^透群友$|^透$|^日$|^操$|^干$|^日批$");
            SubDirectCommand = new Regex(@"^日\s");
            DefaultSettings.SettingsName = "透群友";
            CoolDownTime = 15;
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

        public override Task Unload()
        {
            return Task.CompletedTask;
        }

        private Task Process(string command, GroupMessageReceiver source, string targetId)
        {
            if (Groups.Count != 0)
            {
                Random random = new Random();
                var memberList = new List<string>();
                Groups.TryGetValue(source.GroupId, out memberList);
                if (memberList.Count != 1)
                {
                    var i = random.Next(0, memberList.Count);
                    while (memberList[i] == source.Sender.Id)
                    {
                        i = random.Next(0, memberList.Count);
                    }

                    if (targetId != null)
                    {
                        if (memberList.Contains(targetId))
                        {
                            if (source.Sender.Id != targetId)
                                i = memberList.IndexOf(targetId);
                            else
                            {
                                var message = new MessageChain() { new PlainMessage("吓人") };
                                MessageManager.SendGroupMessageAsync(source.GroupId, message);
                                return Task.CompletedTask;
                            }
                        }
                        else
                        {
                            var message = new MessageChain() { new PlainMessage("该群友未在群聊中发过言！") };
                            MessageManager.SendGroupMessageAsync(source.GroupId, message);
                            return Task.CompletedTask;
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
                                new PlainMessage("您把 "),
                                new PlainMessage(memberName + " (" + memberList[i] + ") "),
                                new PlainMessage("狠狠地操了一顿")
                            };
                        }
                        else
                        {
                            message = new MessageChain()
                            {
                                new AtMessage() { Target = source.Sender.Id },
                                new PlainMessage("您把 "),
                                new PlainMessage(memberName + " (" + memberList[i] + ") "),
                                new PlainMessage("狠狠地操了一顿")
                            };
                        }

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

            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            Process(command, source, null);
            return Task.CompletedTask;
        }

        public override Task SubParse(string command, GroupMessageReceiver source)
        {
            Process(command, source, command);
            return Task.CompletedTask;
        }

        public override Task RespondWithoutParsingCommand(string command, GroupMessageReceiver source)
        {
            return Task.CompletedTask;
        }
    }
}

