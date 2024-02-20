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
using LapisBot_Renewed.ImageGenerators;

namespace LapisBot_Renewed
{
    public class GroupCommand
    {
        public Regex HeadCommand;

        public Regex SubHeadCommand;

        public Regex SubDirectCommand;

        public Regex DirectCommand;

        public GroupCommand ParentCommand;

        public GroupCommandSettings CurrentGroupCommandSettings;

        public readonly List<GroupCommand> SubCommands = new List<GroupCommand>();

        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        public void GetCurrentGroupCommandSettings(GroupMessageReceiver source)
        {
            GetDefaultSettings();
            foreach (GroupCommandSettings groupCommandSettings in settingsList)
            {
                if (groupCommandSettings.GroupId == source.GroupId)
                    CurrentGroupCommandSettings = groupCommandSettings;
            }
        }

        public virtual Task Parse(string command, GroupMessageReceiver source)
        {
            return Task.CompletedTask;
        }

        public virtual Task SubParse(string command, GroupMessageReceiver source)
        {
            return Task.CompletedTask;
        }

        public virtual Task RespondWithoutParsingCommand(string command, GroupMessageReceiver source)
        {
            return Task.CompletedTask;
        }

        public Task AbilityCheckingParse(string command, GroupMessageReceiver source)
        {
            var currentParentGroupCommandSettings = DefaultSettings;
            if (ParentCommand != null)
            {
                ParentCommand.GetCurrentGroupCommandSettings(source);
                currentParentGroupCommandSettings = ParentCommand.CurrentGroupCommandSettings;
            }

            if (currentParentGroupCommandSettings.Enabled)
            {
                GetCurrentGroupCommandSettings(source);

                if (CurrentGroupCommandSettings != null)
                {
                    if (CurrentGroupCommandSettings.Enabled)
                        Parse(command, source);
                }
                else
                    Program.helpCommand.Parse(command, source);
            }

            return Task.CompletedTask;
        }

        public Task SubAbilityCheckingParse(string command, GroupMessageReceiver source)
        {
            var currentParentGroupCommandSettings = DefaultSettings;
            if (ParentCommand != null)
            {
                ParentCommand.GetCurrentGroupCommandSettings(source);
                currentParentGroupCommandSettings = ParentCommand.CurrentGroupCommandSettings;
            }

            if (currentParentGroupCommandSettings.Enabled == true)
            {
                GetCurrentGroupCommandSettings(source);

                if (CurrentGroupCommandSettings != null)
                {
                    if (CurrentGroupCommandSettings.Enabled)
                        SubParse(command, source);
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
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            return Task.CompletedTask;
        }

        public virtual Task SubSettingsParse(string command, GroupMessageReceiver source)
        {
            if (source.Sender.Permission == Mirai.Net.Data.Shared.Permissions.Administrator ||
                source.Sender.Permission == Mirai.Net.Data.Shared.Permissions.Owner || source.Sender.Id == "2794813909")
            {
                var regex = new Regex(@"[1-" + DefaultSettings.DisplayNames.Count + @"]\s((true)|(false))$");
                CurrentGroupCommandSettings = (GroupCommandSettings)Activator.CreateInstance(DefaultSettings.GetType());
                GetDefaultSettings();
                //var settings = (GroupCommandSettings)Activator.CreateInstance(DefaultSettings.GetType());
                for (int i = 0; i < settingsList.Count; i++)
                {
                    if (settingsList[i].GroupId == source.GroupId)
                        CurrentGroupCommandSettings = settingsList[i];
                }

                if (CurrentGroupCommandSettings.GroupId == null)
                {
                    CurrentGroupCommandSettings.GroupId = source.GroupId;
                    settingsList.Add(CurrentGroupCommandSettings);
                    if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings"))
                        Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                                  " Settings");
                    File.WriteAllText(
                        AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings/" +
                        CurrentGroupCommandSettings.GroupId + ".json",
                        JsonConvert.SerializeObject(CurrentGroupCommandSettings));
                }

                if (regex.IsMatch(command))
                {
                    if (command.Contains("true"))
                    {
                        CurrentGroupCommandSettings.GetType()
                            .GetProperty(CurrentGroupCommandSettings.DisplayNames
                                .ElementAt(Int32.Parse(new Regex("[1-9]").Match(command).ToString()) - 1).Key)
                            .SetValue(CurrentGroupCommandSettings, true);
                    }

                    if (command.Contains("false"))
                    {
                        CurrentGroupCommandSettings.GetType()
                            .GetProperty(CurrentGroupCommandSettings.DisplayNames
                                .ElementAt(Int32.Parse(new Regex("[1-9]").Match(command).ToString()) - 1).Key)
                            .SetValue(CurrentGroupCommandSettings, false);
                    }

                    File.Delete(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings/" +
                                source.GroupId + ".json");
                    File.WriteAllText(
                        AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings/" +
                        source.GroupId +
                        ".json", JsonConvert.SerializeObject(CurrentGroupCommandSettings));
                    //settings = CurrentGroupCommandSettings;
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
            CurrentGroupCommandSettings = (GroupCommandSettings)Activator.CreateInstance(DefaultSettings.GetType());
            GetDefaultSettings();
            //var settings = (GroupCommandSettings)Activator.CreateInstance(DefaultSettings.GetType());
            for (int i = 0; i < settingsList.Count; i++)
            {
                if (settingsList[i].GroupId == source.GroupId)
                    CurrentGroupCommandSettings = settingsList[i];
            }

            if (CurrentGroupCommandSettings.GroupId == null)
            {
                CurrentGroupCommandSettings.GroupId = source.GroupId;
                settingsList.Add(CurrentGroupCommandSettings);
                if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                      " Settings"))
                    Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                              " Settings");
                File.WriteAllText(
                    AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings/" +
                    CurrentGroupCommandSettings.GroupId + ".json",
                    JsonConvert.SerializeObject(CurrentGroupCommandSettings));
            }
            Program.settingsCommand.GetSettings(source);
            var image = new BotSettingsImageGenerator().Generate(CurrentGroupCommandSettings,
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

        public GroupCommandSettings DefaultSettings = new GroupCommandSettings()
            { Enabled = true, DisplayNames = new Dictionary<string, string>() { { "Enabled", "启用" } } };
    }
}

