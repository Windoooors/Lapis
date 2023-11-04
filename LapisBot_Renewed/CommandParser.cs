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

namespace LapisBot_Renewed
{
    public class CommandParser
    {
        bool enabled = true;
        private Regex headCommand = new Regex(@"(^lps\s|^六盘水\s|^l\s|^拉\s|^老婆说\s|^Lapis\s|^lapis\s|^lsp\s)");
        private List<Thread[]> threads;

        public void Parse(FriendMessageReceiver source)
        {
            if (enabled)
            {
                var command = source.MessageChain.GetPlainMessage();

                foreach (PrivateCommand _command in Program.privateCommands)
                {
                    _command.ParseWithoutPreparse(command, source);
                }

                if (headCommand.IsMatch(command))
                {
                    command = headCommand.Replace(command, string.Empty);
                    foreach (PrivateCommand _command in Program.privateCommands)
                    {
                        if (_command.headCommand != null && _command.headCommand.IsMatch(command))
                        {
                            command = _command.headCommand.Replace(command, string.Empty);
                            _command.Parse(command, source, false);
                            _command.Parse(command, source);
                            enabled = false;
                            Thread thread = new Thread(Enable);
                            thread.Start();
                            break;
                        }
                        else if (_command.subHeadCommand != null && _command.subHeadCommand.IsMatch(command))
                        {
                            command = _command.subHeadCommand.Replace(command, string.Empty);
                            _command.Parse(command, source, true);
                            enabled = false;
                            Thread thread = new Thread(Enable);
                            thread.Start();
                            break;
                        }
                    }
                }
                //MessageManager.SendFriendMessageAsync(source.FriendId, "_(:_」∠)_\n感谢您对 Lapis 的支持\n在将 Lapis 拉入您的群聊后，您可以在群聊中发送 \"lps help\" 或访问 https://www.setchin.com/lapis.html 以获取帮助 \nLapis 不会占用其他 Bot 的触发指令，请使用 \"lps\" 或 \"l\" 来触发 Lapis");
            }
        }

        void Enable()
        {
            //Thread.Sleep(30000);
            enabled = true;
        }

        public async void Parse(GroupMessageReceiver source)
        {
            if (enabled)
            {
                var command = source.MessageChain.GetPlainMessage();

                foreach (GroupCommand _command in Program.groupCommands)
                {
                    await _command.ParseWithoutPreparse(command, source);
                }

                if (headCommand.IsMatch(command))
                {
                    command = headCommand.Replace(command, string.Empty);
                    foreach (GroupCommand _command in Program.groupCommands)
                    {
                        if (_command.headCommand != null && _command.headCommand.IsMatch(command))
                        {
                            command = _command.headCommand.Replace(command, string.Empty);
                            await _command.Parse(command, source, false);
                            await _command.Parse(command, source);
                            enabled = false;
                            Thread thread = new Thread(Enable);
                            thread.Start();
                            break;
                        }
                        else if (_command.subHeadCommand != null && _command.subHeadCommand.IsMatch(command))
                        {
                            command = _command.subHeadCommand.Replace(command, string.Empty);
                            await _command.Parse(command, source, true);
                            enabled = false;
                            Thread thread = new Thread(Enable);
                            thread.Start();
                            break;
                        }
                    }
                }
            }
        }
    }
}