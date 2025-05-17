using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.GroupCommands;
using LapisBot.Settings;
using Microsoft.Extensions.Logging;

namespace LapisBot;

public class CommandParser
{
    private readonly Regex _headCommandRegex =
        new(@"(^lps\s|^六盘水\s|^l\s|^拉\s|^老婆说\s|^Lapis\s|^lapis\s|^lsp\s)");

    public void StartParsing(CqGroupMessagePostContext source)
    {
        try
        {
            var commandString = source.Message.Text;

            RespondWithoutParsingCommand(source, commandString, Program.GroupCommands);

            if (SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
                ParseDirectly(source, commandString, Program.GroupCommands);

            if (!_headCommandRegex.IsMatch(commandString))
                return;

            commandString = _headCommandRegex.Replace(commandString, string.Empty);

            if (!Parse(source, commandString, Program.GroupCommands))
                HelpCommand.Instance.Parse(source);
        }
        catch (Exception ex)
        {
            HelpCommand.Instance.UnexpectedErrorHelp(source);
            Program.Logger.LogError(ex, ex.Message);
        }
    }

    private void RespondWithoutParsingCommand(CqGroupMessagePostContext source, string commandString,
        List<GroupCommand> commands)
    {
        foreach (var command in commands)
            try
            {
                if (SettingsCommand.Instance.GetValue(command.ActivationSettingsSettingsIdentifier, source.GroupId))
                {
                    var taskParse = new Task(() => command.RespondWithoutParsingCommand(commandString, source));
                    taskParse.Start();
                }

                if (command.SubCommands.Count != 0)
                    RespondWithoutParsingCommand(source, commandString, command.SubCommands);
            }
            catch (Exception ex)
            {
                HelpCommand.Instance.UnexpectedErrorHelp(source);
                Program.Logger.LogError(ex, ex.Message);
            }
    }

    private bool Parse(CqGroupMessagePostContext source, string commandString, List<GroupCommand> commands)
    {
        var parsed = false;
        foreach (var command in commands)
            if (command.CommandHead != null)
            {
                var commandReplaced = command.CommandHead.Replace(commandString, string.Empty);

                if (commandReplaced.StartsWith(' '))
                {
                    commandString = commandReplaced;
                    commandString = commandString.Substring(1);

                    if (command.SubCommands.Count != 0) parsed = Parse(source, commandString, command.SubCommands);

                    if (parsed)
                        return true;

                    if (SettingsCommand.Instance.GetValue(command.ActivationSettingsSettingsIdentifier,
                            source.GroupId))
                    {
                        var taskParse = new Task(() => command.ParseWithArgument(commandString, source));
                        taskParse.Start();
                    }

                    return true;
                }

                if (command.CommandHead.IsMatch(commandString))
                {
                    commandString = command.CommandHead.Replace(commandString, string.Empty);

                    if (commandString != string.Empty)
                        return false;

                    if (command.SubCommands.Count != 0) parsed = Parse(source, commandString, command.SubCommands);

                    if (parsed)
                        return true;

                    if (SettingsCommand.Instance.GetValue(command.ActivationSettingsSettingsIdentifier,
                            source.GroupId))
                    {
                        var taskParse = new Task(() => command.Parse(source));
                        taskParse.Start();
                    }

                    return true;
                }
            }
            else
            {
                if (command.SubCommands.Count == 0)
                    return false;

                parsed = Parse(source, commandString, command.SubCommands);
                if (parsed)
                    return true;
            }

        return false;
    }

    private void ParseDirectly(CqGroupMessagePostContext source, string commandString,
        List<GroupCommand> commands)
    {
        foreach (var command in commands)
        {
            if (command.SubCommands.Count != 0)
                ParseDirectly(source, commandString, command.SubCommands);

            if (command.DirectCommandHead == null)
                continue;

            var commandReplaced = command.DirectCommandHead.Replace(commandString, string.Empty);

            if (command.DirectCommandHead.IsMatch(commandString) && commandReplaced.StartsWith(' '))
            {
                commandString = commandReplaced;
                commandString = commandString.Substring(1);

                if (SettingsCommand.Instance.GetValue(command.ActivationSettingsSettingsIdentifier, source.GroupId))
                {
                    var taskParse = new Task(() => command.ParseWithArgument(commandString, source));
                    taskParse.Start();
                    return;
                }
            }
            else if (command.DirectCommandHead.IsMatch(commandString))
            {
                commandString = command.DirectCommandHead.Replace(commandString, string.Empty);
                if (commandString != string.Empty)
                    continue;

                if (SettingsCommand.Instance.GetValue(command.ActivationSettingsSettingsIdentifier, source.GroupId))
                {
                    var taskParse = new Task(() => command.Parse(source));
                    taskParse.Start();
                    return;
                }
            }
        }
    }
}