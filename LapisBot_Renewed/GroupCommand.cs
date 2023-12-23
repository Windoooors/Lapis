using System;
using Mirai.Net.Data.Messages.Receivers;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages;
using System.Linq;

namespace LapisBot_Renewed
{
    public class GroupCommand
    {
        public Regex headCommand;

        public Regex subHeadCommand;

        public Regex subDirectCommand;

        public Regex directCommand;

        public GroupCommand parentCommand;

        public List<GroupCommand> subCommands = new List<GroupCommand>();

        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        public virtual Task Parse(string command, GroupMessageReceiver source)
        {
            return Task.CompletedTask;
        }

        public virtual Task Parse(string command, GroupMessageReceiver source, bool isSubParse)
        {
            return Task.CompletedTask;
        }

        public virtual Task ParseWithoutPreparse(string command, GroupMessageReceiver source)
        {
            return Task.CompletedTask;
        }

        public Task PreParse(string command, GroupMessageReceiver source)
        {
            var _parentGroupCommandSettings = defaultSettings;
            if (parentCommand != null)
            {
                parentCommand.GetDefaultSettings();
                foreach (GroupCommandSettings parentGroupCommandSettings in parentCommand.settingsList)
                {
                    if (parentGroupCommandSettings.GroupId == source.GroupId)
                    {
                        parentCommand._groupCommandSettings = parentGroupCommandSettings;
                        _parentGroupCommandSettings = parentGroupCommandSettings;
                    }
                }
            }

            if (_parentGroupCommandSettings.Enabled == true)
            {
                GetDefaultSettings();
                foreach (GroupCommandSettings groupCommandSettings in settingsList)
                {
                    if (groupCommandSettings.GroupId == source.GroupId)
                        _groupCommandSettings = groupCommandSettings;
                }

                if (_groupCommandSettings != null)
                {
                    if (_groupCommandSettings.Enabled)
                        Parse(command, source);
                }
                else
                    Program.helpCommand.Parse(command, source);
            }

            return Task.CompletedTask;
        }

        public Task PreParse(string command, GroupMessageReceiver source, bool isSubParse)
        {
            var _parentGroupCommandSettings = defaultSettings;
            if (parentCommand != null)
            {
                parentCommand.GetDefaultSettings();
                foreach (GroupCommandSettings parentGroupCommandSettings in parentCommand.settingsList)
                {
                    if (parentGroupCommandSettings.GroupId == source.GroupId)
                    {
                        parentCommand._groupCommandSettings = parentGroupCommandSettings;
                        _parentGroupCommandSettings = parentGroupCommandSettings;
                    }
                }
            }

            if (_parentGroupCommandSettings.Enabled == true)
            {
                GetDefaultSettings();
                foreach (GroupCommandSettings groupCommandSettings in settingsList)
                {
                    if (groupCommandSettings.GroupId == source.GroupId)
                        _groupCommandSettings = groupCommandSettings;
                }

                if (_groupCommandSettings != null)
                {
                    if (_groupCommandSettings.Enabled)
                        Parse(command, source, isSubParse);
                }
                else
                    Program.helpCommand.Parse(command, source);
            }

            return Task.CompletedTask;
        }

        public virtual Task Unload()
        {
            return Task.CompletedTask;
        }

        public virtual Task GetDefaultSettings()
        {
            _groupCommandSettings = defaultSettings.Clone();
            return Task.CompletedTask;
        }

        public virtual Task SettingsParse(string command, GroupMessageReceiver source, bool isSubParse)
        {
            if (source.Sender.Permission == Mirai.Net.Data.Shared.Permissions.Administrator ||
                source.Sender.Permission == Mirai.Net.Data.Shared.Permissions.Owner || source.Sender.Id == "2794813909")
            {
                var regex = new Regex(@"[1-" + defaultSettings.DisplayNames.Count + @"]\s((true)|(false))$");
                _groupCommandSettings = (GroupCommandSettings)Activator.CreateInstance(defaultSettings.GetType());
                GetDefaultSettings();
                //var settings = (GroupCommandSettings)Activator.CreateInstance(defaultSettings.GetType());
                for (int i = 0; i < settingsList.Count; i++)
                {
                    if (settingsList[i].GroupId == source.GroupId)
                        _groupCommandSettings = settingsList[i];
                }

                if (_groupCommandSettings.GroupId == null)
                {
                    _groupCommandSettings.GroupId = source.GroupId;
                    settingsList.Add(_groupCommandSettings);
                    if (!Directory.Exists(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
                        Directory.CreateDirectory(AppContext.BaseDirectory + _groupCommandSettings.SettingsName +
                                                  " Settings");
                    File.WriteAllText(
                        AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings/" +
                        _groupCommandSettings.GroupId + ".json", JsonConvert.SerializeObject(_groupCommandSettings));
                }

                if (regex.IsMatch(command))
                {
                    if (command.Contains("true"))
                    {
                        _groupCommandSettings.GetType()
                            .GetProperty(_groupCommandSettings.DisplayNames
                                .ElementAt(Int32.Parse(new Regex("[1-9]").Match(command).ToString()) - 1).Key)
                            .SetValue(_groupCommandSettings, true);
                    }

                    if (command.Contains("false"))
                    {
                        _groupCommandSettings.GetType()
                            .GetProperty(_groupCommandSettings.DisplayNames
                                .ElementAt(Int32.Parse(new Regex("[1-9]").Match(command).ToString()) - 1).Key)
                            .SetValue(_groupCommandSettings, false);
                    }

                    File.Delete(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings/" +
                                source.GroupId + ".json");
                    File.WriteAllText(
                        AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings/" + source.GroupId +
                        ".json", JsonConvert.SerializeObject(_groupCommandSettings));
                    //settings = _groupCommandSettings;
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

        public List<GroupCommandSettings> settingsList = new List<GroupCommandSettings>();


        private static TOut TransReflection<TIn, TOut>(TIn tIn)
        {
            TOut tOut = Activator.CreateInstance<TOut>();
            var tInType = tIn.GetType();
            foreach (var itemOut in tOut.GetType().GetProperties())
            {
                var itemIn = tInType.GetProperty(itemOut.Name);
                ;
                if (itemIn != null)
                {
                    itemOut.SetValue(tOut, itemIn.GetValue(tIn));
                }
            }

            return tOut;
        }

        public virtual Task SettingsParse(string command, GroupMessageReceiver source)
        {
            _groupCommandSettings = (GroupCommandSettings)Activator.CreateInstance(defaultSettings.GetType());
            GetDefaultSettings();
            //var settings = (GroupCommandSettings)Activator.CreateInstance(defaultSettings.GetType());
            for (int i = 0; i < settingsList.Count; i++)
            {
                if (settingsList[i].GroupId == source.GroupId)
                    _groupCommandSettings = settingsList[i];
            }

            if (_groupCommandSettings.GroupId == null)
            {
                _groupCommandSettings.GroupId = source.GroupId;
                settingsList.Add(_groupCommandSettings);
                if (!Directory.Exists(AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings"))
                    Directory.CreateDirectory(AppContext.BaseDirectory + _groupCommandSettings.SettingsName +
                                              " Settings");
                File.WriteAllText(
                    AppContext.BaseDirectory + _groupCommandSettings.SettingsName + " Settings/" +
                    _groupCommandSettings.GroupId + ".json", JsonConvert.SerializeObject(_groupCommandSettings));
            }

            Console.WriteLine(_groupCommandSettings.SettingsName);
            Program.settingsCommand.GetSettings(source);
            var image = BotSettingsImageGenerator.Generate(_groupCommandSettings,
                Program.settingsCommand.CurrentBotSettings.CompressedImage);
            MessageManager.SendGroupMessageAsync(source.GroupId,
                new MessageChain() { new AtMessage(source.Sender.Id), new ImageMessage() { Base64 = image } });
            return Task.CompletedTask;
        }

        public class GroupCommandSettings : BotSettingsCommand.Settings
        {
            public bool Enabled { get; set; }

            public GroupCommandSettings Clone()
            {
                return JsonConvert.DeserializeObject<GroupCommandSettings>(JsonConvert.SerializeObject(this));
            }
        }

        public GroupCommandSettings _groupCommandSettings;

        public GroupCommandSettings defaultSettings = new GroupCommandSettings()
            { Enabled = true, DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" } } };
    }
}

