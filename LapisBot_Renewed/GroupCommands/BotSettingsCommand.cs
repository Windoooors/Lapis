using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data;
using Mirai.Net.Sessions.Http.Managers;
using ImageMagick;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using Newtonsoft.Json;
using Flurl.Util;
using System.Linq;

namespace LapisBot_Renewed
{
    public class BotSettingsCommand : GroupCommand
    {
        public class Settings
        {
            public string SettingsName { get; set; }
            public string GroupId { get; set; }
            public Dictionary<string, string> DisplayNames = new Dictionary<string, string>();
        }

        public class BotSettings : Settings
        {
            public bool HeadlessCommand { get; set; }
            public bool UpdateMessage { get; set; }

            public bool CompressedImage { get; set; }

            public BotSettings Clone()
            {
                return JsonConvert.DeserializeObject<BotSettings>(JsonConvert.SerializeObject(this));
            }
        }

        public List<BotSettings> botSettingsList = new List<BotSettings>();

        public BotSettings botDefaultSettings = new BotSettings()
            { SettingsName = "通用设置", HeadlessCommand = true, UpdateMessage = true, CompressedImage = true };

        public override Task Initialize()
        {
            botDefaultSettings.DisplayNames.Add("HeadlessCommand", "无指令头触发指令");
            botDefaultSettings.DisplayNames.Add("UpdateMessage", "更新提醒");
            botDefaultSettings.DisplayNames.Add("CompressedImage", "图片压缩");
            if (!Directory.Exists(AppContext.BaseDirectory + "settings"))
                Directory.CreateDirectory(AppContext.BaseDirectory + "settings");
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + "settings"))
            {
                var settingsString = File.ReadAllText(path);
                botSettingsList.Add(JsonConvert.DeserializeObject<BotSettings>(settingsString));
            }

            _groupCommandSettings = new GroupCommandSettings() { Enabled = true };
            headCommand = new Regex(@"^settings$");
            subHeadCommand = new Regex(@"^settings\s");
            directCommand = new Regex(@"^settings$");
            subDirectCommand = new Regex(@"^settings\s");
            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            foreach (BotSettings settings in botSettingsList)
            {
                if (settings.GroupId == source.GroupId)
                {
                    var _image = BotSettingsImageGenerator.Generate(settings, settings.CompressedImage);
                    var _messageChain = new MessageChain()
                        { new AtMessage(source.Sender.Id), new ImageMessage() { Base64 = _image } };
                    //MessageManager.SendGroupMessageAsync()
                    MessageManager.SendGroupMessageAsync(source.GroupId, _messageChain);
                    return Task.CompletedTask;
                }
            }

            var _settings = botDefaultSettings.Clone();
            _settings.GroupId = source.GroupId;
            botSettingsList.Add(_settings);
            File.WriteAllText(AppContext.BaseDirectory + "settings/" + source.GroupId + ".json",
                JsonConvert.SerializeObject(_settings));
            var image = BotSettingsImageGenerator.Generate(_settings, _settings.CompressedImage);
            var messageChain = new MessageChain()
                { new AtMessage(source.Sender.Id), new ImageMessage() { Base64 = image } };
            //MessageManager.SendGroupMessageAsync()
            MessageManager.SendGroupMessageAsync(source.GroupId, messageChain);
            return Task.CompletedTask;
        }

        public override Task SettingsParse(string command, GroupMessageReceiver source)
        {
            Program.helpCommand.Parse(command, source);
            return Task.CompletedTask;
        }

        public override Task SettingsParse(string command, GroupMessageReceiver source, bool isSubParse)
        {
            Program.helpCommand.Parse(command, source);
            return Task.CompletedTask;
        }

        public BotSettings CurrentBotSettings;

        public void GetSettings(GroupMessageReceiver source)
        {
            foreach (BotSettings settings in botSettingsList)
            {
                if (settings.GroupId == source.GroupId)
                {
                    CurrentBotSettings = settings;
                    break;
                }
            }

            if (CurrentBotSettings.GroupId == null)
            {
                CurrentBotSettings = botDefaultSettings.Clone();
                CurrentBotSettings.GroupId = source.GroupId;
                botSettingsList.Add(CurrentBotSettings);
                File.WriteAllText(AppContext.BaseDirectory + "settings/" + source.GroupId + ".json",
                    JsonConvert.SerializeObject(CurrentBotSettings));
            }
        }

        public override Task Parse(string command, GroupMessageReceiver source, bool isSubParse)
        {
            if (source.Sender.Permission == Mirai.Net.Data.Shared.Permissions.Administrator ||
                source.Sender.Permission == Mirai.Net.Data.Shared.Permissions.Owner || source.Sender.Id == "2794813909")
            {
                //var _settings = new BotSettings();
                var regex = new Regex(@"[1-3]\s((true)|(false))$");
                GetSettings(source);
                if (regex.IsMatch(command))
                {
                    if (command.Contains("true"))
                    {
                        CurrentBotSettings.GetType()
                            .GetProperty(CurrentBotSettings.DisplayNames
                                .ElementAt(Int32.Parse(new Regex("[1-9]").Match(command).ToString()) - 1).Key)
                            .SetValue(CurrentBotSettings, true);
                    }

                    if (command.Contains("false"))
                    {
                        CurrentBotSettings.GetType()
                            .GetProperty(CurrentBotSettings.DisplayNames
                                .ElementAt(Int32.Parse(new Regex("[1-9]").Match(command).ToString()) - 1).Key)
                            .SetValue(CurrentBotSettings, false);
                    }

                    File.Delete(AppContext.BaseDirectory + "settings/" + source.GroupId + ".json");
                    File.WriteAllText(AppContext.BaseDirectory + "settings/" + source.GroupId + ".json",
                        JsonConvert.SerializeObject(CurrentBotSettings));
                    MessageManager.SendGroupMessageAsync(source.GroupId,
                        new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 设置已生效") });
                }
                else
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId,
                        new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 输入格式有误") });
                }

                return Task.CompletedTask;
            }
            else
            {
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" 您无权执行该命令") });
                return Task.CompletedTask;
            }
        }
    }
}
