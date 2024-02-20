using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Sessions.Http.Managers;
using System.IO;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class AbuseCommand : GroupCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^骂我$");
            DirectCommand = new Regex(@"^骂我$");
            DefaultSettings.SettingsName = "骂";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");

            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        private string[] _dirtyWordsStrings = { "6", "杂鱼\u2661~杂鱼\u2661~", "你妈炸了你个傻逼", "操你妈了个逼，操你老婆逼，操你女儿逼，你全家 2024 年死光光，操你妈了个逼的", "就你这粉丝量想跟我撞？", "严重的怀疑你自以为是，不是所谓的新版你吃不吃史？", "人生自古谁无死？不幸地，Index已在于上浮的搏斗中去世，让我们永远缅怀"};

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            var i = new Random().Next(0, _dirtyWordsStrings.Length);
            if (source.GroupId == "1078224429")
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new PlainMessage(_dirtyWordsStrings[i]) });
            else
                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new PlainMessage(_dirtyWordsStrings[0]) });
            return Task.CompletedTask;
        }
    }
}
