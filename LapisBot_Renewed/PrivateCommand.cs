using System;
using Mirai.Net.Data.Messages.Receivers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LapisBot_Renewed
{
    public class PrivateCommand
    {
        public Regex HeadCommand;

        public Regex SubHeadCommand;

        public virtual Task Initialize() { return Task.CompletedTask; }

        public virtual Task Parse(string command, FriendMessageReceiver source) { return Task.CompletedTask; }

        public virtual Task Parse(string command, FriendMessageReceiver source, bool isSubParse) { return Task.CompletedTask; }

        public virtual Task ParseWithoutPreparse(string command, FriendMessageReceiver source) { return Task.CompletedTask; }

        public virtual Task Unload() { return Task.CompletedTask; }
    }
}


