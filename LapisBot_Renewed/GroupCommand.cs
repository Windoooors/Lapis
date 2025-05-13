using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.GroupCommands;
using LapisBot_Renewed.Settings;

namespace LapisBot_Renewed
{
    public class GroupCommand
    {
        public Regex CommandHead;
        public Regex DirectCommandHead;

        public readonly List<GroupCommand> SubCommands = [];

        public SettingsIdentifierPair ActivationSettingsSettingsIdentifier = new();
        
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
}

