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
using System.IO;
using System;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class AliasCommand : MaiCommand
    {
        public InfoCommand FindInfoCommand()
        {
            MaiCommand MaiCommandCommand;
            foreach (GroupCommand command in Program.groupCommands)
            {
                if (command is MaiCommand)
                {
                    MaiCommandCommand = (MaiCommand)command;
                    foreach (MaiCommand _command in MaiCommandCommand.SubCommands)
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

        //public MaiCommand MaiCommandCommand;

        public override Task Unload()
        {
            foreach (AliasCommand aliasCommand in SubCommands)
                aliasCommand.Unload();
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            //SubCommands.Add(new AddAliasCommand() { MaiCommandCommand = MaiCommandCommand });
            foreach (AliasCommand aliasCommand in SubCommands)
                aliasCommand.Initialize();
            HeadCommand = new Regex(@"^alias\s");
            DirectCommand = new Regex(@"^alias\s|^别名\s");
            DefaultSettings.SettingsName = "别名";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings");

            }
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }
            return Task.CompletedTask;
        }

        public string GetAliasesInString(Alias alias)
        {
            var result = string.Empty;
            if (alias.Aliases.Count != 0)
            {
                result = "歌曲 " + MaiCommandCommand.Songs[MaiCommandCommand.GetSongIndexById(alias.Id)].Title + " [" + MaiCommandCommand.Songs[MaiCommandCommand.GetSongIndexById(alias.Id)].Type.ToString() + "]" + " 有如下别称：\n";
                List<string> aliasList = new List<string>();
                for (int i = 0; i < alias.Aliases.Count; i++)
                {
                    if (aliasList.Contains(alias.Aliases[i]))
                        continue;
                    result += alias.Aliases[i];
                    aliasList.Add(alias.Aliases[i]);
                    if (i != alias.Aliases.Count - 1)
                    {
                        result += "\n";
                    }
                }
            }
            else
            {
                result = "歌曲 " + MaiCommandCommand.Songs[MaiCommandCommand.GetSongIndexById(alias.Id)].Title + " [" + MaiCommandCommand.Songs[MaiCommandCommand.GetSongIndexById(alias.Id)].Type.ToString() + "]" + " 没有别称";
            }
            return result;
        }



        public override Task Parse(string command, GroupMessageReceiver source)
        {
            foreach (MaiCommand subCommand in SubCommands)
            {
                if (subCommand.HeadCommand.IsMatch(command) && subCommand.HeadCommand.Replace(command, "") != string.Empty)
                {
                    command = subCommand.HeadCommand.Replace(command, "");
                    subCommand.Parse(command, source);
                    return Task.CompletedTask;
                }
            }
            var Aliases = MaiCommandCommand.GetAliasByAliasString(command);
            if (Aliases.Length != 0)
            {
                if (Aliases.Length == 1)
                {
                    for (int i = 0; i < MaiCommandCommand.Songs.Length; i++)
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" " + GetAliasesInString(MaiCommandCommand.GetAliasById(Aliases[0].Id))) });
                        break;
                    }
                }
                else
                {
                    string ids = string.Empty;
                    List<int> idsList = new List<int>();
                    for (int i = 0; i < Aliases.Length; i++)
                    {
                        if (idsList.Contains(Aliases[i].Id))
                            continue;
                        int _index = MaiCommandCommand.GetSongIndexById(Aliases[i].Id);
                        ids += "ID " + Aliases[i].Id + " - " + MaiCommandCommand.Songs[_index].Title + " [" + MaiCommandCommand.Songs[_index].Type + "]";
                        idsList.Add(Aliases[i].Id);
                        if (i != Aliases.Length - 1)
                            ids += "\n";
                    }
                    if (idsList.Count == 1)
                    {
                        Parse("ID " + idsList[0] + command.Replace(MaiCommandCommand.GetAliasStringUsingStartsWith(command), string.Empty), source);
                        return Task.CompletedTask;
                    }
                    int index = MaiCommandCommand.GetSongIndexById(idsList[0]);
                    MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                        new AtMessage(source.Sender.Id),
                        new PlainMessage(" 该别称有多首歌曲匹配：\n" + ids + "\n*使用 \"lps mai alias ID " + idsList[0] + "\" 指令即可查询歌曲 " + MaiCommandCommand.Songs[index].Title + " [" + MaiCommandCommand.Songs[index].Type + "] 的相关信息")});
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
                        int index = MaiCommandCommand.GetSongIndexById(id);
                        if (index != -1)
                            MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" " + GetAliasesInString(MaiCommandCommand.GetAliasById(id))) });
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
                    int index = MaiCommandCommand.GetSongIndexByTitle(command);
                    if (index != -1)
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), new PlainMessage(" " + GetAliasesInString(MaiCommandCommand.GetAliasById(MaiCommandCommand.Songs[index].Id))) });
                    }
                    else
                        MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain(){
                                new AtMessage(source.Sender.Id), new PlainMessage(" 不存在该歌曲") });
                }
                //MessageManager.SendGroupMessageAsync(source.GroupId, " ");
            }
            return Task.CompletedTask;
        }
    }
}
