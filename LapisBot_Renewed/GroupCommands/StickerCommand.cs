using System;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using System.Collections;
using System.Collections.Generic;

namespace LapisBot_Renewed
{
    public class StickerCommand : GroupCommand
    {
        public Dictionary<string, StickerCommand> subCommands = new Dictionary<string, StickerCommand>();

        public override void Initialize()
        {
            subCommands.Clear();
            headCommand = new Regex(@"^sticker\s");
            //MessageManager.SendGroupMessageAsync(source.GroupId, "傻逼");
            subCommands.Add("喜报", new FortuneCommand());

            foreach (KeyValuePair<string, StickerCommand> stickerCommand in subCommands)
                stickerCommand.Value.Initialize();
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            foreach (KeyValuePair<string, StickerCommand> subCommand in subCommands)
            {
                if (subCommand.Value.headCommand.IsMatch(command) && subCommand.Value.headCommand.Replace(command, "") != string.Empty)
                {
                    command = subCommand.Value.headCommand.Replace(command, "");
                    subCommand.Value.Parse(command, source);
                }
            }
        }
    }
}
