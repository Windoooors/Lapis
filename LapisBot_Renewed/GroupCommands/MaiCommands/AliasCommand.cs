using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Manganese.Text;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Manganese.Array;
using System.Threading.Tasks;

namespace LapisBot_Renewed
{
    public class AliasCommand : MaiCommand
    {
        public InfoCommand FindInfoCommand()
        {
            MaiCommand maiCommand;
            foreach (GroupCommand command in Program.groupCommands)
            {
                if (command is MaiCommand)
                {
                    maiCommand = (MaiCommand)command;
                    foreach (MaiCommand _command in maiCommand.subCommands)
                    {
                        if (_command is InfoCommand)
                        {
                            return (InfoCommand)_command;
                        }
                    }
                }
            }
            return null;
        }

        public MaiCommand maiCommand;

        public override Task Unload()
        {
            foreach (AliasCommand aliasCommand in subCommands)
                aliasCommand.Unload();
        }

        public override Task Initialize()
        {
            subCommands.Add(new AddAliasCommand() { maiCommand = maiCommand });
            foreach (AliasCommand aliasCommand in subCommands)
                aliasCommand.Initialize();
            headCommand = new Regex(@"^alias\s");

        }

        public string GetAliasesInString(Alias alias)
        {
            var result = string.Empty;
            if (alias.aliases.Count != 0)
            {
                result = "歌曲 " + maiCommand.songs[maiCommand.GetSongIndexById(alias.id)].Title + " [" + maiCommand.songs[maiCommand.GetSongIndexById(alias.id)].Type.ToString() + "]" + " 有如下别称：\n";
                for (int i = 0; i < alias.aliases.Count; i++)
                {
                    result += alias.aliases[i];
                    if (i != alias.aliases.Count - 1)
                    {
                        result += "\n";
                    }
                }
            }
            else
            {
                result = "歌曲 " + maiCommand.songs[maiCommand.GetSongIndexById(alias.id)].Title + " [" + maiCommand.songs[maiCommand.GetSongIndexById(alias.id)].Type.ToString() + "]" + " 没有别称";
            }
            return result;
        }

        

        public override void Parse(string command, GroupMessageReceiver source)
        {
            foreach (MaiCommand subCommand in subCommands)
            {
                if (subCommand.headCommand.IsMatch(command) && subCommand.headCommand.Replace(command, "") != string.Empty)
                {
                    command = subCommand.headCommand.Replace(command, "");
                    subCommand.Parse(command, source);
                    return;
                }
            }
            var aliases = maiCommand.GetAliasByAliasString(command);
            if (aliases.Length != 0)
            {
                if (aliases.Length == 1)
                {
                    for (int i = 0; i < maiCommand.songs.Length; i++)
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" " + GetAliasesInString(maiCommand.GetAliasById(aliases[0].id))) });
                        break;
                    }
                }
                else
                {
                    string ids = string.Empty;
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        int _index = maiCommand.GetSongIndexById(aliases[i].id);
                        ids += "ID " + aliases[i].id + " - " + maiCommand.songs[_index].Title + " [" + maiCommand.songs[_index].Type + "]";
                        if (i != aliases.Length - 1)
                            ids += "\n";
                    }
                    int index = maiCommand.GetSongIndexById(aliases[0].id);
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(" 该别称有多首歌曲匹配：\n" + ids + "\n*使用 \"lps mai alias ID " + aliases[0].id + "\" 指令即可查询歌曲 " + maiCommand.songs[index].Title + " [" + maiCommand.songs[index].Type + "] 的别称")});
                }
            }
            else
            {
                var idRegex = new Regex(@"(^id\s|^id|^ID\s|^ID)-?[0-9]+");
                var idHeadRegex = new Regex(@"^id\s|^id|^ID\s|^ID");
                if (idRegex.IsMatch(command))
                {
                    try
                    {
                        var id = idHeadRegex.Replace(command, string.Empty).ToInt32();
                        int index = maiCommand.GetSongIndexById(id);
                        if (index != -1)
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" " + GetAliasesInString(maiCommand.GetAliasById(id))) });
                        else
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                    }
                    catch
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                    }

                }
                else
                {
                    int index = maiCommand.GetSongIndexByTitle(command);
                    if (index != -1)
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" " + GetAliasesInString(maiCommand.GetAliasById(maiCommand.songs[index].Id))) });
                    }
                    else
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                }
                //MessageManager.SendGroupMessageAsync(source.GroupId, " ");
            }
        }
    }
}
