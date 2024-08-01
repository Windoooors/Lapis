using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using DNS.Protocol;
using System.Threading.Tasks;
using System.IO;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

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

        private Task Process(string command, CqGroupMessagePostContext source, string targetId)
        {
            if (Groups.Count != 0)
            {
                Random random = new Random();
                var memberList = new List<string>();
                Groups.TryGetValue(source.GroupId.ToString(), out memberList);
                if (memberList.Count != 1)
                {
                    var i = random.Next(0, memberList.Count);
                    while (memberList[i] == source.Sender.UserId.ToString())
                    {
                        i = random.Next(0, memberList.Count);
                    }

                    if (targetId != null)
                    {
                        if (memberList.Contains(targetId))
                        {
                            if (source.Sender.UserId.ToString() != targetId)
                                i = memberList.IndexOf(targetId);
                            else
                            {
                                CqMessage message = [ new CqTextMsg("吓人") ];
                                Program.Session.SendGroupMessageAsync(source.GroupId, message);
                                return Task.CompletedTask;
                            }
                        }
                        else
                        {
                            CqMessage message = [ new CqTextMsg("该群友未在群聊中发过言！") ];
                            Program.Session.SendGroupMessageAsync(source.GroupId, message);
                            return Task.CompletedTask;
                        }
                    }

                    try
                    {
                        var result =
                            Program.Session.GetGroupMemberInformation(source.GroupId, long.Parse(memberList[i]));
                        if (result == null)
                            return Task.CompletedTask; 
                        var memberName = result.Nickname;
                        var message = new CqMessage();
                        if (!OperatingSystem.IsMacOS())
                        {
                            var image = Program.apiOperator.ImageToBase64("https://q.qlogo.cn/g?b=qq&nk=" +
                                                                          memberList[i] + "&s=640");
                            message = 
                            [
                                new CqReplyMsg(source.MessageId),
                                new CqImageMsg("base64://" + image),
                                new CqTextMsg("您把 "),
                                new CqTextMsg(memberName + " (" + memberList[i] + ") "),
                                new CqTextMsg("狠狠地操了一顿")
                            ];
                        }
                        else
                        {
                            message = 
                            [
                                new CqReplyMsg(source.MessageId),
                                new CqTextMsg("您把 "),
                                new CqTextMsg(memberName + " (" + memberList[i] + ") "),
                                new CqTextMsg("狠狠地操了一顿")
                            ];
                        }
                        
                        Program.Session.SendGroupMessageAsync(source.GroupId, message);
                    }
                    catch
                    {
                        memberList.RemoveAt(i);
                        Groups.Remove(source.GroupId.ToString());
                        Groups.Add(source.GroupId.ToString(), memberList);
                        Parse(command, source);
                    }
                }
                else
                {
                    var message = new CqMessage()
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("近期发言人数太少咯 _(:_」∠)_ Lapis 找不到你的对象")
                    };
                    Program.Session.SendGroupMessageAsync(source.GroupId, message);
                }
            }

            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            Process(command, source, null);
            return Task.CompletedTask;
        }

        public override Task SubParse(string command, CqGroupMessagePostContext source)
        {
            Process(command, source, command);
            return Task.CompletedTask;
        }

        public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
        {
            return Task.CompletedTask;
        }
    }
}

