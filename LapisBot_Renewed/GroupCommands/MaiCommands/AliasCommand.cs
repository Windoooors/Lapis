using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

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
            HeadCommand = new Regex(@"^alias\s");
            DirectCommand = new Regex(@"^alias\s|^别名\s|有什么别名$|\s有什么别名$");
            DefaultSettings.SettingsName = "别名";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");
            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        public string GetAliasesInString(Alias alias)
        {
            var result = string.Empty;
            var song = MaiCommandCommand.GetSong(alias.Id);
            if (alias.Aliases.Count != 0)
            {
                result = "歌曲 " + song.Title + " [" +
                         song.Type.ToString() + "]" +
                         " 有如下别称：\n";
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
                result = "歌曲 " + song.Title + " [" +
                         song.Type.ToString() + "]" +
                         " 没有别称";
            }

            return result;
        }



        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
            var songs = MaiCommandCommand.GetSongs(command);

            if (songs == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqAtMsg(source.Sender.UserId),
                    new CqTextMsg(" 不存在该歌曲")
                ]);
                return Task.CompletedTask;
            }

            if (songs.Length == 1)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqAtMsg(source.Sender.UserId),
                    new CqTextMsg(" " + GetAliasesInString(MaiCommandCommand.GetAliasById(songs[0].Id)))
                ]);
                return Task.CompletedTask;
            }

            string ids = string.Empty;
            List<int> idsList = new List<int>();
            for (int i = 0; i < songs.Length; i++)
            {
                ids += "ID " + songs[i].Id + " - " + songs[i].Title + " [" + songs[i].Type + "]";
                if (i != songs.Length - 1)
                    ids += "\n";
                idsList.Add(songs[i].Id);
            }


            Program.Session.SendGroupMessageAsync(source.GroupId,
            [
                new CqAtMsg(source.Sender.UserId),
                new CqTextMsg(" 该别称有多首歌曲匹配：\n" + ids + "\n*发送 \"lps mai alias ID " + idsList[0] + "\" 指令即可查询歌曲 " +
                              songs[0].Title + " [" + songs[0].Type +
                              "] 的别称")
            ]);

            return Task.CompletedTask;
        }
    }
}
