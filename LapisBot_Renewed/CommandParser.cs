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
        private readonly Regex _headCommandRegex =
            new Regex(@"(^lps\s|^六盘水\s|^l\s|^拉\s|^老婆说\s|^Lapis\s|^lapis\s|^lsp\s)");

        private List<Thread[]> threads;

        public async void Parse(FriendMessageReceiver source)
        {
            var command = source.MessageChain.GetPlainMessage();

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
            //MessageManager.SendFriendMessageAsync(source.FriendId, "_(:_」∠)_\n感谢您对 Lapis 的支持\n在将 Lapis 拉入您的群聊后，您可以在群聊中发送 \"lps help\" 或访问 https://www.setchin.com/lapis.html 以获取帮助 \nLapis 不会占用其他 Bot 的触发指令，请使用 \"lps\" 或 \"l\" 来触发 Lapis");
        }

        public void MainParse(GroupMessageReceiver source)
        {
            if ((RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
                 (source.Sender.Id == "2794813909" || source.Sender.Id == "361851827")) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Program.settingsCommand.GetSettings(source);

                var commandString = source.MessageChain.GetPlainMessage();

                RespondWithoutParsingCommand(source, commandString, Program.groupCommands);
                
                var currentBotSettings = Program.settingsCommand.CurrentBotSettings;
                if (currentBotSettings.HeadlessCommand)
                    ParseHeadlessly(source, commandString, Program.groupCommands);

                if (!_headCommandRegex.IsMatch(commandString))
                    return;
                
                commandString = _headCommandRegex.Replace(commandString, string.Empty);
                
                Parse(source, commandString, Program.groupCommands);
            }
        }

        private async void RespondWithoutParsingCommand(GroupMessageReceiver source, string commandString, List<GroupCommand> commands)
        {
            foreach (GroupCommand command in commands)
            {
                await command.RespondWithoutParsingCommand(commandString, source);
                if (command.SubCommands.Count != 0)
                {
                    RespondWithoutParsingCommand(source, commandString, command.SubCommands);
                }
            }
        }

        private async void Parse(GroupMessageReceiver source, string commandString, List<GroupCommand> commands)
        {
            foreach (GroupCommand command in commands)
            {
                if (command.SubHeadCommand != null && command.SubHeadCommand.IsMatch(commandString))
                {
                    commandString = command.SubHeadCommand.Replace(commandString, string.Empty);
                    if (command.SubCommands.Count != 0)
                    {
                        Parse(source, commandString, command.SubCommands);
                    }

                    if (await SettingsParse(source, commandString, command))
                        return;
                    await command.SubAbilityCheckingParse(commandString, source);
                }
                else if (command.HeadCommand != null && command.HeadCommand.IsMatch(commandString))
                {
                    commandString = command.HeadCommand.Replace(commandString, string.Empty);
                    if (command.SubCommands.Count != 0)
                    {
                        Parse(source, commandString, command.SubCommands);
                    }

                    if (await SettingsParse(source, commandString, command))
                        return;
                    await command.AbilityCheckingParse(commandString, source);
                }
            }
        }

        private async void ParseHeadlessly(GroupMessageReceiver source, string commandString,
            List<GroupCommand> commands)
        {
            foreach (GroupCommand command in commands)
            {
                if (command.SubCommands.Count != 0)
                {
                    ParseHeadlessly(source, commandString, command.SubCommands);
                }

                if (command.SubDirectCommand != null && command.SubDirectCommand.IsMatch(commandString))
                {
                    commandString = command.SubDirectCommand.Replace(commandString, string.Empty);
                    if (await SettingsParse(source, commandString, command))
                        return;
                    await command.SubAbilityCheckingParse(commandString, source);
                }
                else if (command.DirectCommand != null && command.DirectCommand.IsMatch(commandString))
                {
                    commandString = command.DirectCommand.Replace(commandString, string.Empty);
                    if (await SettingsParse(source, commandString, command))
                        return;
                    await command.AbilityCheckingParse(commandString, source);
                }
            }
        }

        private async Task<Boolean> SettingsParse(GroupMessageReceiver source, string commandString,
            GroupCommand command)
        {
            var showSettingsRegex = new Regex(@"^settings$");
            var settingsRegex = new Regex(@"^settings\s[0-9]\s(true|false)$");
            if (showSettingsRegex.IsMatch(commandString))
            {
                await command.SettingsParse(commandString, source);
                return true;
            }

            if (settingsRegex.IsMatch(commandString))
            {
                commandString = settingsRegex.Match(commandString).ToString();
                await command.SubSettingsParse(commandString, source);
                return true;
            }

            return false;
        }
    }
}