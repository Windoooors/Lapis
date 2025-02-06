using System.Threading.Tasks;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace LapisBot_Renewed
{
    public class UpdateMessageCommand : PrivateCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^update\s");
            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqPrivateMessagePostContext source)
        {
            if (source.Sender.UserId == 2794813909)
            {
                var message = command;
                var result = Program.Session.GetGroupList();
                if (result == null)
                    return Task.CompletedTask;
                foreach (CqGroup group in result.Groups)
                {
                    BotSettingsCommand.BotSettings settings = Program.settingsCommand.botDefaultSettings;
                    foreach(BotSettingsCommand.BotSettings _settings in Program.settingsCommand.botSettingsList)
                    {
                        if (_settings.GroupId == group.GroupId.ToString())
                            settings = _settings;
                    }

                    if (settings.UpdateMessage)
                        Program.Session.SendGroupMessageAsync(group.GroupId, new CqMessage
                            { message });
                }
            }
            return Task.CompletedTask;
        }
    }
}
