using System;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LapisBot_Renewed
{
    public class UpdateMessageCommand : PrivateCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^update\s");
            return Task.CompletedTask;
        }

        public override Task Parse(string command, FriendMessageReceiver source)
        {
            if (source.FriendId == "2794813909")
            {
                var message = command;
                foreach (Mirai.Net.Data.Shared.Group group in Program.bot.Groups.Value)
                {
                    BotSettingsCommand.BotSettings settings = Program.settingsCommand.botDefaultSettings;
                    foreach(BotSettingsCommand.BotSettings _settings in Program.settingsCommand.botSettingsList)
                    {
                        if (_settings.GroupId == group.Id)
                            settings = _settings;
                    }
                    if (settings.UpdateMessage)
                        MessageManager.SendGroupMessageAsync(group.Id, message);
                }
            }
            return Task.CompletedTask;
        }
    }
}
