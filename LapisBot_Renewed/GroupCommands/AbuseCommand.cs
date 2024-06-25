using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class AbuseCommand : GroupCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^骂我$|^夸我$");
            DirectCommand = new Regex(@"^骂我$|^夸我$");
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

        private string[] _dirtyWordsStrings =
        {
            "6", "杂鱼\u2661~杂鱼\u2661~", "你妈炸了你个傻逼", "我不骂傻逼", "操你妈了个逼，操你老婆逼，操你女儿逼，你全家 2024 年死光光，操你妈了个逼的", "就你这粉丝量想跟我撞？",
            "严重的怀疑你自以为是，不是所谓的新版你吃不吃史？", "人生自古谁无死？不幸地，Index已在于上浮的搏斗中去世，让我们永远缅怀"
        };

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            if (source.GroupId.ToString() == "1078224429")
            {
                var i = new Random().Next(0, _dirtyWordsStrings.Length);
                Program.Session.SendGroupMessageAsync(source.GroupId, [
                    new CqAtMsg(source.Sender.UserId),
                    new CqTextMsg(" " + _dirtyWordsStrings[i])
                ]);
            }
            else
            {
                var i = new Random().Next(1, 5);
                Program.Session.SendGroupMessageAsync(source.GroupId, [
                    new CqAtMsg(source.Sender.UserId),
                    new CqTextMsg(" " + _dirtyWordsStrings[i])
                ]);
            }

            return Task.CompletedTask;
        }
    }
}
