using System;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using DNS.Protocol;

namespace LapisBot_Renewed
{
	public class GetHimFuckedCommand : DoSomethingWithHimCommand
	{
        public override void Initialize()
        {
            headCommand = new Regex(@"^透群友$|^透$|^日$|^操$|^干$|^日批$");
            subHeadCommand = new Regex(@"^日蛇精$");
        }

        public override void Unload()
        {
            
        }

        public override void ParseWithoutPreparse(string command, GroupMessageReceiver source)
        {

        }

        public override void Parse(string command, GroupMessageReceiver source, bool isSubParse)
        {
            if (groups.Count != 0)
            {
                Random random = new Random();
                var _memberList = new List<string>();
                groups.TryGetValue(source.GroupId, out _memberList);
                if (_memberList.Count != 1)
                {
                    var i = random.Next(0, _memberList.Count);
                    while (_memberList[i] == source.Sender.Id)
                    {
                        i = random.Next(0, _memberList.Count);
                    }
                    if (isSubParse)
                    {
                        if (_memberList.Contains("2794813909"))
                        {
                            if (source.Sender.Id != "2794813909")
                                i = _memberList.IndexOf("2794813909");
                            else
                            {
                                var message = new MessageChain() { new PlainMessage("得了吧") };
                                MessageManager.SendGroupMessageAsync(source.GroupId, message);
                                return;
                            }
                        }
                        else
                        {
                            var message = new MessageChain() { new PlainMessage("这货没在这群发过言") };
                            MessageManager.SendGroupMessageAsync(source.GroupId, message);
                            return;
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
                            new PlainMessage("您把 "),
                            new PlainMessage(memberName + " (" + _memberList[i] + ") "),
                            new PlainMessage("狠狠地操了一顿") };
                        }
                        else
                        {
                            message = new MessageChain() {
                            new AtMessage(){ Target = source.Sender.Id },
                            new PlainMessage("您把 "),
                            new PlainMessage(memberName + " (" + _memberList[i] + ") "),
                            new PlainMessage("狠狠地操了一顿") };
                        }
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
        }
    }
}

