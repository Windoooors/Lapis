using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.GroupCommands;
using LapisBot.Settings;

namespace LapisBot;

public class GroupCommand
{
    public readonly List<GroupCommand> SubCommands = [];

    public SettingsIdentifierPair ActivationSettingsSettingsIdentifier = new();
    public Regex CommandHead;
    public Regex DirectCommandHead;

    public virtual Task Initialize()
    {
        return Task.CompletedTask;
    }

    public virtual Task Parse(CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
        return Task.CompletedTask;
    }

    public virtual Task ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
        return Task.CompletedTask;
    }

    public virtual Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        return Task.CompletedTask;
    }

    public virtual Task Unload()
    {
        return Task.CompletedTask;
    }
}