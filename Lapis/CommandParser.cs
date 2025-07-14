using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands;
using Lapis.Commands.UniversalCommands;
using Lapis.Settings;
using Microsoft.Extensions.Logging;

namespace Lapis;

public class CommandParser
{
    private readonly Regex _headCommandRegex =
        new(@"(^lps\s|^六盘水\s|^l\s|^拉\s|^老婆说\s|^Lapis\s|^lapis\s|^lsp\s)");

    public void StartParsing(CqMessagePostContext source)
    {
        try
        {
            var commandString = source.Message.Text;
            /*var mentionedUserIdsList = new List<long>();
            foreach (var messageElement in source.Message)
                if (messageElement is CqAtMsg atMessage && !atMessage.IsAtAll)
                    mentionedUserIdsList.Add(atMessage.Target);
            var mentionedUserIds = mentionedUserIdsList.ToArray();*/

            RespondWithoutParsingCommand(source, commandString, Program.Commands);

            switch (source)
            {
                case CqGroupMessagePostContext groupSource:
                    if (SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"),
                            groupSource.GroupId))
                        ParseDirectly(source, commandString, Program.Commands);
                    break;
                case CqPrivateMessagePostContext:
                    ParseDirectly(source, commandString, Program.Commands);
                    break;
            }

            if (!_headCommandRegex.IsMatch(commandString))
                return;

            commandString = _headCommandRegex.Replace(commandString, string.Empty, 1);

            if (!Parse(source, commandString, Program.Commands))
                HelpCommand.Instance.Parse(source);
        }
        catch (Exception ex)
        {
            HelpCommand.Instance.UnexpectedErrorHelp(source);
            Program.Logger.LogError(ex, ex.Message);
        }
    }

    private void RespondWithoutParsingCommand(CqMessagePostContext source, string commandString,
        Command[] commands)
    {
        foreach (var command in commands)
        {
            StartRespondWithoutParsingCommandTask(command, source, commandString);

            if (command.SubCommands.Length != 0)
                RespondWithoutParsingCommand(source, commandString, command.SubCommands);
        }
    }

    private bool Parse(CqMessagePostContext source, string commandString, Command[] commands)
    {
        commandString = commandString.Trim();
        var parsed = false;
        foreach (var command in commands)
            if (command.CommandHead != null)
            {
                var commandHeadMatchingEndingSpace = new Regex(@$"^({command.CommandHead})\s");
                if (commandHeadMatchingEndingSpace.IsMatch(commandString))
                {
                    commandString = commandHeadMatchingEndingSpace.Replace(commandString, string.Empty, 1);

                    if (command.SubCommands.Length != 0)
                        parsed = Parse(source, commandString, command.SubCommands);

                    if (parsed)
                        return true;

                    return StartParsingWithArgumentTask(command, source, commandString);
                }

                var commandHeadMatchingEndOfString = new Regex($"^({command.CommandHead})$");
                if (commandHeadMatchingEndOfString.IsMatch(commandString))
                {
                    if (command.SubCommands.Length != 0)
                        parsed = Parse(source, commandString, command.SubCommands);

                    if (parsed)
                        return true;

                    return StartParsingTask(command, source);
                }
            }
            else
            {
                if (command.SubCommands.Length == 0)
                    continue;

                parsed = Parse(source, commandString, command.SubCommands);
                if (parsed)
                    return true;
            }

        return false;
    }

    private void ParseDirectly(CqMessagePostContext source, string commandString,
        Command[] commands)
    {
        commandString = commandString.Trim();
        foreach (var command in commands)
        {
            if (command.SubCommands.Length != 0)
                ParseDirectly(source, commandString, command.SubCommands);

            if (command.DirectCommandHead == null)
                continue;

            var directCommandHeadMatchingEndingSpace = new Regex(@$"^({command.DirectCommandHead})\s");

            if (directCommandHeadMatchingEndingSpace.IsMatch(commandString))
            {
                commandString = directCommandHeadMatchingEndingSpace.Replace(commandString, string.Empty, 1);

                StartParsingWithArgumentTask(command, source, commandString);
                return;
            }

            var directCommandHeadMatchingEndOfString = new Regex($"^({command.DirectCommandHead})$");

            if (directCommandHeadMatchingEndOfString.IsMatch(commandString))
            {
                StartParsingTask(command, source);
                return;
            }
        }
    }

    private bool StartParsingWithArgumentTask(Command command, CqMessagePostContext source, string commandString)
    {
        Task taskParse = null;

        commandString = commandString.Trim();

        var quotationRegex = new Regex("\"([^\"]+)\"|(\\S+)");

        var argumentList = new List<string>();

        var matches = quotationRegex.Matches(commandString);

        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            if (i < command.IntendedArgumentCount)
                argumentList.Add((match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).Trim());
            else
                argumentList[^1] +=
                    " " + (match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).Trim();
        }

        switch (command)
        {
            case GroupCommand groupCommand:
                if (source is CqGroupMessagePostContext groupSource)
                    if (SettingsPool.GetValue(command.ActivationSettingsSettingsIdentifier,
                            groupSource.GroupId))
                        taskParse = new Task(() =>
                            groupCommand.ParseWithArgument(argumentList.ToArray(), groupSource));

                break;
            case PrivateCommand privateCommand:
                if (source is CqPrivateMessagePostContext privateSource)
                    taskParse = new Task(() => privateCommand.ParseWithArgument(argumentList.ToArray(), privateSource));

                break;
            case UniversalCommand universalCommand:
                taskParse = new Task(() => universalCommand.ParseWithArgument(argumentList.ToArray(), source));
                break;
        }

        if (taskParse == null)
            return false;
        taskParse.Start();
        return true;
    }

    private bool StartParsingTask(Command command, CqMessagePostContext source)
    {
        Task taskParse = null;
        switch (command)
        {
            case GroupCommand groupCommand:
                if (source is CqGroupMessagePostContext groupSource)
                    if (SettingsPool.GetValue(command.ActivationSettingsSettingsIdentifier,
                            groupSource.GroupId))
                        taskParse = new Task(() => groupCommand.Parse(groupSource));

                break;
            case PrivateCommand privateCommand:
                if (source is CqPrivateMessagePostContext privateSource)
                    taskParse = new Task(() => privateCommand.Parse(privateSource));

                break;
            case UniversalCommand universalCommand:
                taskParse = new Task(() => universalCommand.Parse(source));
                break;
        }

        if (taskParse == null)
            return false;
        taskParse.Start();
        return true;
    }

    private void StartRespondWithoutParsingCommandTask(Command command, CqMessagePostContext source,
        string commandString)
    {
        Task taskParse = null;
        switch (command)
        {
            case GroupCommand groupCommand:
                if (source is CqGroupMessagePostContext groupSource)
                    if (SettingsPool.GetValue(command.ActivationSettingsSettingsIdentifier,
                            groupSource.GroupId))
                        taskParse = new Task(() =>
                            groupCommand.RespondWithoutParsingCommand(commandString, groupSource));

                break;
            case PrivateCommand privateCommand:
                if (source is CqPrivateMessagePostContext privateSource)
                    taskParse = new Task(() =>
                        privateCommand.RespondWithoutParsingCommand(commandString, privateSource));

                break;
            case UniversalCommand universalCommand:
                taskParse = new Task(() =>
                    universalCommand.RespondWithoutParsingCommand(commandString, source));

                break;
        }

        if (taskParse == null)
            return;
        taskParse.Start();
    }
}