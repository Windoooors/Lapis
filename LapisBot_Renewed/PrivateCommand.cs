using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Post;

namespace LapisBot_Renewed
{
    public class PrivateCommand
    {
        public Regex HeadCommand;

        public Regex SubHeadCommand;

        public virtual Task Initialize() { return Task.CompletedTask; }

        public virtual Task Parse(string command, CqPrivateMessagePostContext source) { return Task.CompletedTask; }

        public virtual Task Parse(string command, CqPrivateMessagePostContext  source, bool isSubParse) { return Task.CompletedTask; }

        public virtual Task ParseWithoutPreparse(string command, CqPrivateMessagePostContext  source) { return Task.CompletedTask; }

        public virtual Task Unload() { return Task.CompletedTask; }
    }
}


