using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.ImageGenerators;

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
            botDefaultSettings.DisplayNames.Add("HeadlessCommand", "简洁指令");
            botDefaultSettings.DisplayNames.Add("UpdateMessage", "更新提醒");
            botDefaultSettings.DisplayNames.Add("CompressedImage", "图片压缩");
            //Console.WriteLine(AppContext.BaseDirectory + "settings");
            
            if (!Directory.Exists(AppContext.BaseDirectory + "settings"))
                Directory.CreateDirectory(AppContext.BaseDirectory + "settings");
            
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + "settings"))
            {
                //Console.WriteLine(path);
                var settingsString = File.ReadAllText(path);
                botSettingsList.Add(JsonConvert.DeserializeObject<BotSettings>(settingsString));
            }

            CurrentGroupCommandSettings = new GroupCommandSettings() { Enabled = true };
            HeadCommand = new Regex(@"^settings$");
            SubHeadCommand = new Regex(@"^settings\s");
            DirectCommand = new Regex(@"^settings$");
            SubDirectCommand = new Regex(@"^settings\s");
            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            foreach (BotSettings settings in botSettingsList)
            {
                if (settings.GroupId == source.GroupId.ToString())
                {
                    var _image = new BotSettingsImageGenerator().Generate(settings, settings.CompressedImage);
                    var _messageChain = new CqMessage()
                        { new CqAtMsg(source.Sender.UserId), new CqImageMsg("base64://" + _image)};
                    //MessageManager.SendGroupMessageAsync()
                    Program.Session.SendGroupMessageAsync(source.GroupId, _messageChain);
                    return Task.CompletedTask;
                }
            }

            var _settings = botDefaultSettings.Clone();
            _settings.GroupId = source.GroupId.ToString();
            botSettingsList.Add(_settings);
            File.WriteAllText(AppContext.BaseDirectory + "settings/" + source.GroupId + ".json",
                JsonConvert.SerializeObject(_settings));
            var image = new BotSettingsImageGenerator().Generate(_settings, _settings.CompressedImage);
            var messageChain = new CqMessage()
                { new CqAtMsg(source.Sender.UserId), new CqImageMsg("base64://" + image) };
            Program.Session.SendGroupMessageAsync(source.GroupId, messageChain);
            //MessageManager.SendGroupMessageAsync()

            return Task.CompletedTask;
        }

        public override Task SettingsParse(string command, CqGroupMessagePostContext source)
        {
            Program.helpCommand.Parse(command, source);
            return Task.CompletedTask;
        }

        public override Task SubSettingsParse(string command, CqGroupMessagePostContext source)
        {
            Program.helpCommand.Parse(command, source);
            return Task.CompletedTask;
        }

        public BotSettings CurrentBotSettings;
        
        public void GetSettings(string groupId)
        {
            CurrentBotSettings = botDefaultSettings.Clone();
            foreach (BotSettings settings in botSettingsList)
            {
                if (settings.GroupId == groupId)
                {
                    CurrentBotSettings = settings;
                    break;
                }
            }
            if (CurrentBotSettings.GroupId == null)
            {
                CurrentBotSettings.GroupId = groupId;
                botSettingsList.Add(CurrentBotSettings);
                File.WriteAllText(AppContext.BaseDirectory + "settings/" + groupId + ".json",
                    JsonConvert.SerializeObject(CurrentBotSettings));
            }
        }

        public void GetSettings(CqGroupMessagePostContext source)
        {
            CurrentBotSettings = botDefaultSettings.Clone();
            foreach (BotSettings settings in botSettingsList)
            {
                if (settings.GroupId == source.GroupId.ToString())
                {
                    CurrentBotSettings = settings;
                    break;
                }
            }
            if (CurrentBotSettings.GroupId == null)
            {
                CurrentBotSettings.GroupId = source.GroupId.ToString();
                botSettingsList.Add(CurrentBotSettings);
                File.WriteAllText(AppContext.BaseDirectory + "settings/" + source.GroupId + ".json",
                    JsonConvert.SerializeObject(CurrentBotSettings));
            }
        }

        public override Task SubParse(string command, CqGroupMessagePostContext source)
        {
            if (source.Sender.Role == CqRole.Admin ||
                source.Sender.Role == CqRole.Owner || source.Sender.UserId == 2794813909)
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
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        new CqMessage() { new CqAtMsg(source.Sender.UserId), new CqTextMsg(" 设置已生效") });
                }
                else
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        new CqMessage() { new CqAtMsg(source.Sender.UserId), new CqTextMsg(" 输入格式有误") });
                }

                return Task.CompletedTask;
            }
            else
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage() { new CqAtMsg(source.Sender.UserId), new CqTextMsg(" 您无权执行该命令") });
                return Task.CompletedTask;
            }
        }
    }
}
