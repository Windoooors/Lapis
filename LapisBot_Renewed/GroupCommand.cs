using System;
using Mirai.Net.Data.Messages.Receivers;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LapisBot_Renewed
{
	public class GroupCommand
	{
        public Regex headCommand;

        public Regex subHeadCommand;

        public virtual async Task Initialize() { }

        public virtual async Task Parse(string command, GroupMessageReceiver source) { }

        public virtual async Task Parse(string command, GroupMessageReceiver source, bool isSubParse) { }

        public virtual async Task ParseWithoutPreparse(string command, GroupMessageReceiver source) { }

        public virtual async Task Unload() { }
    }
}

