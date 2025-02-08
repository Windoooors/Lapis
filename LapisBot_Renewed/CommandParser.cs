using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using SixLabors.ImageSharp.Diagnostics;

namespace LapisBot_Renewed
{
    public class CommandParser
    {
        private readonly Regex _headCommandRegex =
            new Regex(@"(^lps\s|^六盘水\s|^l\s|^拉\s|^老婆说\s|^Lapis\s|^lapis\s|^lsp\s)");
        private readonly Regex _settingsRegex = new Regex(@"\ssettings\s[0-9]\s(true|false)$|\ssettings$|\ssettings\s[0-9]\s.*");
        
        public async void Parse(CqPrivateMessagePostContext source)
        {
            try
            {
                var command = source.Message.Text;

                foreach (PrivateCommand _command in Program.privateCommands)
                {
                    await _command.ParseWithoutPreparse(command, source);
                }

                if (_headCommandRegex.IsMatch(command))
                {
                    command = _headCommandRegex.Replace(command, string.Empty);
                    foreach (PrivateCommand _command in Program.privateCommands)
                    {
                        if (_command.HeadCommand != null && _command.HeadCommand.IsMatch(command))
                        {
                            command = _command.HeadCommand.Replace(command, string.Empty);
                            await _command.Parse(command, source, false);
                            await _command.Parse(command, source);
                            break;
                        }
                        else if (_command.SubHeadCommand != null && _command.SubHeadCommand.IsMatch(command))
                        {
                            command = _command.SubHeadCommand.Replace(command, string.Empty);
                            await _command.Parse(command, source, true);
                            break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                await Program.Session.SendPrivateMessageAsync(source.Sender.UserId,
                    new CqMessage
                        { new CqReplyMsg(source.MessageId), new CqTextMsg("抱歉！出现了未知错误\n错误信息如下：\n" + ex.StackTrace) });
            }
            //MessageManager.SendFriendMessageAsync(source.FriendId, "_(:_」∠)_\n感谢您对 Lapis 的支持\n在将 Lapis 拉入您的群聊后，您可以在群聊中发送 \"lps help\" 或访问 https://www.setchin.com/lapis.html 以获取帮助 \nLapis 不会占用其他 Bot 的触发指令，请使用 \"lps\" 或 \"l\" 来触发 Lapis");
        }
        
        private string GetParsedCommandString(Regex commandRegex, string commandString)
        {
            var commandStringWithoutSettingsArguments = _settingsRegex.Replace(commandString, string.Empty);
            if (commandRegex.IsMatch(commandStringWithoutSettingsArguments) ||
                !commandRegex.IsMatch(commandString))
                return commandString.Replace(
                    commandRegex.Match(commandStringWithoutSettingsArguments).ToString(),
                    string.Empty);
            return commandRegex.Replace(commandString, String.Empty);
        }

        private bool IsMatched(Regex commandRegex, string commandString)
        {
            var commandStringWithoutSettingsArguments = _settingsRegex.Replace(commandString, string.Empty);
            return commandRegex != null &&
                   (commandRegex.IsMatch(commandStringWithoutSettingsArguments) ||
                    commandRegex.IsMatch(commandString));
        }

        public void MainParse(CqGroupMessagePostContext source)
        {
            try
            {
                if ((Program.BotSettings.IsDevelopingMode &&
                     (source.Sender.UserId == 2794813909 || source.Sender.UserId == 361851827 ||
                      source.Sender.UserId == 2750558108)) ||
                    !Program.BotSettings.IsDevelopingMode)
                {
                    Program.settingsCommand.GetSettings(source);

                    var commandString = source.Message.Text;

                    RespondWithoutParsingCommand(source, commandString, Program.groupCommands);

                    var currentBotSettings = Program.settingsCommand.CurrentBotSettings;
                    if (currentBotSettings.HeadlessCommand)
                        ParseHeadlessly(source, commandString, Program.groupCommands);

                    if (!_headCommandRegex.IsMatch(commandString))
                        return;

                    commandString = _headCommandRegex.Replace(commandString, string.Empty);

                    if (!Parse(source, commandString, Program.groupCommands))
                        Program.helpCommand.Parse(commandString, source);
                }
            }
            catch(Exception ex)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                        { new CqReplyMsg(source.MessageId), new CqTextMsg("抱歉！出现了未知错误\n错误信息如下：\n" + ex.StackTrace) });
            }
        }

        private void RespondWithoutParsingCommand(CqGroupMessagePostContext source, string commandString,
            List<GroupCommand> commands)
        {
            foreach (GroupCommand command in commands)
            {
                try
                {
                    var taskParse = new Task(() => command.RespondWithoutParsingCommand(commandString, source));
                    taskParse.Start();
                    if (command.SubCommands.Count != 0)
                    {
                        RespondWithoutParsingCommand(source, commandString, command.SubCommands);
                    }
                }
                catch(Exception ex)
                {
                    Program.Session.SendGroupMessageAsync(source.GroupId,
                        new CqMessage
                        {
                            new CqReplyMsg(source.MessageId), new CqTextMsg("抱歉！出现了未知错误\n错误信息如下：\n" + ex.StackTrace)
                        });
                }
            }
        }

        private bool Parse(CqGroupMessagePostContext source, string commandString, List<GroupCommand> commands)
        {
            var parsed = false;
            foreach (GroupCommand command in commands)
            {
                if (IsMatched(command.SubHeadCommand, commandString))
                {
                    commandString = GetParsedCommandString(command.SubHeadCommand, commandString);
                    if (command.SubCommands.Count != 0)
                    {
                        parsed = Parse(source, commandString, command.SubCommands);
                    }

                    if (SettingsParse(source, commandString, command))
                        return true;

                    if (parsed)
                        return true;
                    
                    var taskParse = new Task(() => command.SubAbilityCheckingParse(commandString, source));
                    taskParse.Start();
                    
                    Console.WriteLine($"Number of undisposed ImageSharp buffers: {MemoryDiagnostics.TotalUndisposedAllocationCount}");
                    
                    if (command.SubCommands.Count != 0 && !parsed)
                        return false;
                    return true;
                }
                if (IsMatched(command.HeadCommand, commandString))
                {
                    commandString = GetParsedCommandString(command.HeadCommand, commandString);
                    if (command.SubCommands.Count != 0)
                    {
                        parsed = Parse(source, commandString, command.SubCommands);
                    }

                    if (SettingsParse(source, commandString, command))
                        return true;
                    
                    if (parsed)
                        return true;

                    var taskParse = new Task(() => command.AbilityCheckingParse(commandString, source));
                    taskParse.Start();
                    
                    Console.WriteLine($"Number of undisposed ImageSharp buffers: {MemoryDiagnostics.TotalUndisposedAllocationCount}");
                    
                    if (command.SubCommands.Count != 0 && !parsed)
                        return false;
                    return true;
                }
                if (command.SubHeadCommand == null && command.HeadCommand == null)
                {
                    if (command.SubCommands.Count != 0)
                    {
                        parsed = Parse(source, commandString, command.SubCommands);
                        if (parsed)
                            return true;
                    }
                }
            }
            return false;
        }

        private void ParseHeadlessly(CqGroupMessagePostContext source, string commandString,
            List<GroupCommand> commands)
        {
            foreach (GroupCommand command in commands)
            {
                if (command.SubCommands.Count != 0)
                {
                    ParseHeadlessly(source, commandString, command.SubCommands);
                }

                if (IsMatched(command.SubDirectCommand, commandString))
                {
                    commandString = GetParsedCommandString(command.SubDirectCommand, commandString);

                    if (SettingsParse(source, commandString, command))
                        return;
                    var taskParse = new Task(() => command.SubAbilityCheckingParse(commandString, source));
                    taskParse.Start();
                    Console.WriteLine($"Number of undisposed ImageSharp buffers: {MemoryDiagnostics.TotalUndisposedAllocationCount}");
                }
                else if (IsMatched(command.DirectCommand, commandString))
                {
                    commandString = GetParsedCommandString(command.DirectCommand, commandString);
                    if (SettingsParse(source, commandString, command))
                        return;
                    var taskParse = new Task(() => command.AbilityCheckingParse(commandString, source));
                    taskParse.Start();
                    Console.WriteLine($"Number of undisposed ImageSharp buffers: {MemoryDiagnostics.TotalUndisposedAllocationCount}");
                }
            }
        }

        private bool SettingsParse(CqGroupMessagePostContext source, string commandString,
            GroupCommand command)
        {
            var showSettingsRegex = new Regex(@"^settings$|^\ssettings$");
            var settingsRegex = new Regex(@"^settings\s[0-9]\s(true|false)$|^\ssettings\s[0-9]\s(true|false)$|^settings\s[0-9]\s|^\ssettings\s[0-9]\s.*");
            if (showSettingsRegex.IsMatch(commandString))
            {
                var taskParse = new Task(() => command.SettingsParse(commandString, source));
                taskParse.Start();
                return true;
            }

            if (settingsRegex.IsMatch(commandString))
            {
                commandString = settingsRegex.Match(commandString).ToString();
                var taskParse = new Task(() => command.SubSettingsParse(commandString, source));
                taskParse.Start();
                return true;
            }

            return false;
        }
    }
}