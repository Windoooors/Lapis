using System;
using Mirai.Net.Data.Messages.Receivers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LapisBot_Renewed
{
    public class PrivateCommand
    {
        public Regex headCommand;

        public Regex subHeadCommand;

        public virtual async Task Initialize() { }

        public virtual async Task Parse(string command, FriendMessageReceiver source) { }

        public virtual async Task Parse(string command, FriendMessageReceiver source, bool isSubParse) { }

        public virtual async Task ParseWithoutPreparse(string command, FriendMessageReceiver source) { }

        public virtual async Task Unload() { }
    }
}
	


