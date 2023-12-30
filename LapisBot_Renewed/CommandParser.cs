using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using System.Threading;
using Manganese.Array;
using System.Runtime.InteropServices;
using System.Linq;

namespace LapisBot_Renewed
{
    public class CommandParser
    {
        bool enabled = true;
        private Regex headCommand = new Regex(@"(^lps\s|^六盘水\s|^l\s|^拉\s|^老婆说\s|^Lapis\s|^lapis\s|^lsp\s)");
        private List<Thread[]> threads;

        public async void Parse(FriendMessageReceiver source)
        {
            if (enabled)
            {
                var command = source.MessageChain.GetPlainMessage();

                foreach (PrivateCommand _command in Program.privateCommands)
                {
                    await _command.ParseWithoutPreparse(command, source);
                }

                if (headCommand.IsMatch(command))
                {
                    command = headCommand.Replace(command, string.Empty);
                    foreach (PrivateCommand _command in Program.privateCommands)
                    {
                        if (_command.headCommand != null && _command.headCommand.IsMatch(command))
                        {
                            command = _command.headCommand.Replace(command, string.Empty);
                            await _command.Parse(command, source, false);
                            await _command.Parse(command, source);
                            enabled = false;
                            Thread thread = new Thread(Enable);
                            thread.Start();
                            break;
                        }
                        else if (_command.subHeadCommand != null && _command.subHeadCommand.IsMatch(command))
                        {
                            command = _command.subHeadCommand.Replace(command, string.Empty);
                            await _command.Parse(command, source, true);
                            enabled = false;
                            Thread thread = new Thread(Enable);
                            thread.Start();
                            break;
                        }
                    }
                }
                //MessageManager.SendFriendMessageAsync(source.FriendId, "_(:_」∠)_\n感谢您对 Lapis 的支持\n在将 Lapis 拉入您的群聊后，您可以在群聊中发送 \"lps help\" 或访问 https://www.setchin.com/lapis.html 以获取帮助 \nLapis 不会占用其他 Bot 的触发指令，请使用 \"lps\" 或 \"l\" 来触发 Lapis");
            }
        }

        void Enable()
        {
            //Thread.Sleep(30000);
            enabled = true;
        }

        public async void Parse(GroupMessageReceiver source)
        {
            if ((RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && (source.Sender.Id == "2794813909" || source.Sender.Id == "361851827")) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var firstParseRegex = new Regex(@"^settings$|^set$");
                var secondParseRegex = new Regex(@"^settings\s|^set\s");
                var thirdParseRegex = new Regex(@"^\ssettings$|^\sset$");
                var fourthParseRegex = new Regex(@"^\ssettings\s|^\sset\s");
                if (enabled)
                {
                    BotSettingsCommand.BotSettings settings = Program.settingsCommand.botDefaultSettings;
                    var command = source.MessageChain.GetPlainMessage();

                    foreach (GroupCommand _command in Program.groupCommands)
                    {
                        await _command.ParseWithoutPreparse(command, source);
                    }

                    foreach (BotSettingsCommand.BotSettings _settings in Program.settingsCommand.botSettingsList)
                    {
                        if (_settings.GroupId == source.GroupId)
                        {
                            settings = _settings;
                            break;
                        }
                    }
                    if (settings.HeadlessCommand)
                    {
                        foreach (GroupCommand _command in Program.groupCommands)
                        {
                            if (_command.subCommands.Count != 0)
                            {
                                foreach (GroupCommand _subCommand in _command.subCommands)
                                {
                                    if (new Regex(@"(\ssettings.*|\ssettings^)|(\sset.*|\sset^)").IsMatch(command))
                                    {
                                        var ___command = new Regex(@"(\ssettings.*|\ssettings^)|(\sset.*|\sset^)").Replace(command, "");
                                        if (_subCommand.directCommand != null && _subCommand.directCommand.IsMatch(___command) || _subCommand.directCommand != null && _subCommand.directCommand.IsMatch(___command + " "))
                                        {
                                            var __command = command.Replace(___command, "");
                                            if (firstParseRegex.IsMatch(__command))
                                            {
                                                var taskParse = new Task(() => _subCommand.SettingsParse(___command, source));
                                                taskParse.Start();
                                                return;
                                            }
                                            if (secondParseRegex.IsMatch(__command))
                                            {
                                                command = secondParseRegex.Replace(command, string.Empty);
                                                var taskParse = new Task(() => _subCommand.SettingsParse(command, source, true));
                                                taskParse.Start();
                                                return;
                                            }
                                            if (thirdParseRegex.IsMatch(__command))
                                            {
                                                var taskParse = new Task(() => _subCommand.SettingsParse(command, source));
                                                taskParse.Start();
                                                return;
                                            }
                                            if (fourthParseRegex.IsMatch(__command))
                                            {
                                                command = fourthParseRegex.Replace(command, string.Empty);
                                                var taskParse = new Task(() => _subCommand.SettingsParse(command, source, true));
                                                taskParse.Start();
                                                return;
                                            }
                                        }
                                    }
                                    if (_subCommand.directCommand != null && _subCommand.directCommand.IsMatch(command))
                                    {
                                        command = _subCommand.directCommand.Replace(command, string.Empty);
                                        //await _subCommand.PreParse(command, source);
                                        var taskParse = new Task(() => _subCommand.PreParse(command, source));
                                        taskParse.Start();
                                        return;
                                    }
                                    else if (_subCommand.subDirectCommand != null && _subCommand.subDirectCommand.IsMatch(command))
                                    {
                                        command = _subCommand.subDirectCommand.Replace(command, string.Empty);
                                        var taskParse = new Task(() => _subCommand.PreParse(command, source, true));
                                        taskParse.Start();
                                        enabled = false;
                                        Thread thread = new Thread(Enable);
                                        thread.Start();
                                        return;
                                    }
                                }
                            }
                            if (_command.directCommand != null && _command.directCommand.IsMatch(command))
                            {
                                command = _command.directCommand.Replace(command, string.Empty);
                                var taskParse = new Task(() => _command.PreParse(command, source));
                                taskParse.Start();
                                return;
                            }
                            else if (_command.subDirectCommand != null && _command.subDirectCommand.IsMatch(command))
                            {
                                command = _command.subDirectCommand.Replace(command, string.Empty);
                                var taskParse = new Task(() => _command.PreParse(command, source, true));
                                taskParse.Start();
                                enabled = false;
                                Thread thread = new Thread(Enable);
                                thread.Start();
                                return;
                            }
                            if (new Regex(@"(\ssettings.*|\ssettings^)|(\sset.*|\sset^)").IsMatch(command))
                            {
                                var ___command = new Regex(@"(\ssettings.*|\ssettings^)|(\sset.*|\sset^)").Replace(command, "");
                                if (_command.directCommand != null && _command.directCommand.IsMatch(___command) || _command.directCommand != null && _command.directCommand.IsMatch(___command + " "))
                                {
                                    var __command = command.Replace(___command, "");
                                    if (firstParseRegex.IsMatch(__command))
                                    {
                                        var taskParse = new Task(() => _command.SettingsParse(___command, source));
                                        taskParse.Start();
                                        return;
                                    }
                                    if (secondParseRegex.IsMatch(__command))
                                    {
                                        command = secondParseRegex.Replace(command, string.Empty);
                                        var taskParse = new Task(() => _command.SettingsParse(command, source, true));
                                        taskParse.Start();
                                        return;
                                    }
                                    if (thirdParseRegex.IsMatch(__command))
                                    {
                                        var taskParse = new Task(() => _command.SettingsParse(command, source));
                                        taskParse.Start();
                                        return;
                                    }
                                    if (fourthParseRegex.IsMatch(__command))
                                    {
                                        command = fourthParseRegex.Replace(command, string.Empty);
                                        var taskParse = new Task(() => _command.SettingsParse(command, source, true));
                                        taskParse.Start();
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    if (headCommand.IsMatch(command))
                    {
                        command = headCommand.Replace(command, string.Empty);
                        foreach (GroupCommand _command in Program.groupCommands)
                        {
                            foreach (GroupCommand _subCommand in _command.subCommands)
                            {
                                if (new Regex(@"(\ssettings.*|\ssettings^)|(\sset.*|\sset^)").IsMatch(command))
                                {
                                    var ___command = new Regex(@"(\ssettings.*|\ssettings^)|(\sset.*|\sset^)").Replace(command, "");
                                    var ____command = _command.headCommand.Replace(___command, "");
                                    if (_subCommand.headCommand != null && _subCommand.headCommand.IsMatch(____command) || _subCommand.headCommand != null && _subCommand.headCommand.IsMatch(____command + " "))
                                    {
                                        var __command = command.Replace(___command, "");
                                        if (firstParseRegex.IsMatch(__command))
                                        {
                                            var taskParse = new Task(() => _subCommand.SettingsParse(___command, source));
                                            taskParse.Start();
                                            return;
                                        }
                                        if (secondParseRegex.IsMatch(__command))
                                        {
                                            command = secondParseRegex.Replace(command, string.Empty);
                                            var taskParse = new Task(() => _subCommand.SettingsParse(command, source, true));
                                            taskParse.Start();
                                            return;
                                        }
                                        if (thirdParseRegex.IsMatch(__command))
                                        {
                                            var taskParse = new Task(() => _subCommand.SettingsParse(command, source));
                                            taskParse.Start();
                                            return;
                                        }
                                        if (fourthParseRegex.IsMatch(__command))
                                        {
                                            command = fourthParseRegex.Replace(command, string.Empty);
                                            var taskParse = new Task(() => _subCommand.SettingsParse(command, source, true));
                                            taskParse.Start();
                                            return;
                                        }
                                    }
                                }
                            }
                            if (new Regex(@"(\ssettings.*|\ssettings^)|(\sset.*|\sset^)").IsMatch(command))
                            {
                                var ___command = new Regex(@"(\ssettings.*|\ssettings^)|(\sset.*|\sset^)").Replace(command, "");
                                if (_command.headCommand != null && _command.headCommand.IsMatch(___command) || _command.headCommand != null && _command.headCommand.IsMatch(___command + " "))
                                {
                                    var __command = command.Replace(___command, "");
                                    if (firstParseRegex.IsMatch(__command))
                                    {
                                        var taskParse = new Task(() => _command.SettingsParse(___command, source));
                                        taskParse.Start();
                                        return;
                                    }
                                    if (secondParseRegex.IsMatch(__command))
                                    {
                                        command = secondParseRegex.Replace(command, string.Empty);
                                        var taskParse = new Task(() => _command.SettingsParse(command, source, true));
                                        taskParse.Start();
                                        return;
                                    }
                                    if (thirdParseRegex.IsMatch(__command))
                                    {
                                        var taskParse = new Task(() => _command.SettingsParse(command, source));
                                        taskParse.Start();
                                        return;
                                    }
                                    if (fourthParseRegex.IsMatch(__command))
                                    {
                                        command = fourthParseRegex.Replace(command, string.Empty);
                                        var taskParse = new Task(() => _command.SettingsParse(command, source, true));
                                        taskParse.Start();
                                        return;
                                    }
                                }
                            }
                            if (_command.headCommand != null && _command.headCommand.IsMatch(command))
                            {
                                Console.WriteLine(_command.GetType().Name);
                                command = _command.headCommand.Replace(command, string.Empty);
                                var taskParse = new Task(() => _command.PreParse(command, source));
                                taskParse.Start();
                                enabled = false;
                                Thread thread = new Thread(Enable);
                                thread.Start();
                                return;
                            }
                            else if (_command.subHeadCommand != null && _command.subHeadCommand.IsMatch(command))
                            {
                                command = _command.subHeadCommand.Replace(command, string.Empty);
                                var taskParse = new Task(() => _command.PreParse(command, source, true));
                                taskParse.Start();
                                enabled = false;
                                Thread thread = new Thread(Enable);
                                thread.Start();
                                return;
                            }
                        }
                        await Program.helpCommand.Parse("", source);
                    }
                }
            }
        }
    }
}