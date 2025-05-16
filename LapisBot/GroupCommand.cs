using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.GroupCommands;
using LapisBot.Settings;

namespace LapisBot;

public class GroupCommand
{
    public List<GroupCommand> SubCommands = [];

    public SettingsIdentifierPair ActivationSettingsSettingsIdentifier { get; set; } = new();
    public Regex CommandHead;
    public Regex DirectCommandHead;

    public virtual void Initialize()
    {
    }

    public virtual void Parse(CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        HelpCommand.Instance.Parse(source);
    }

    public virtual void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
    }

    public virtual void Unload()
    {
    }
}