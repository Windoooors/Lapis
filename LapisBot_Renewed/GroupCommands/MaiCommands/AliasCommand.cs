using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.GroupCommands.MaiCommands.AliasCommands;
using LapisBot_Renewed.Settings;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public abstract class AliasCommandBase : MaiCommandBase
    {
        public static AliasCommand AliasCommandInstance;
    }
    
    public class AliasCommand : AliasCommandBase
    {
        public override Task Unload()
        {
            foreach (var aliasCommand in SubCommands)
                aliasCommand.Unload();
            return Task.CompletedTask;
        }

        public AliasCommand()
        {
            AliasCommandInstance = this;
            CommandHead = new Regex("^alias");
            DirectCommandHead = new Regex("^alias|^别名|^查看别名");
            ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("alias", "1");
        }


        public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
        {
            if (!SettingsCommand.Instance.GetValue(new("litecommand", "1"), source.GroupId))
                return Task.CompletedTask;

            if (command.EndsWith(" 有什么别名"))
                command = command.Replace(" 有什么别名", "");
            else if (command.EndsWith("有什么别名"))
                command = command.Replace("有什么别名", "");
            else
                return Task.CompletedTask;
            
            ParseWithArgument(command, source);
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            SubCommands.Add(new AddCommand());
            
            foreach (var subAliasCommand in SubCommands)
            {
                subAliasCommand.Initialize();
            }
            
            return Task.CompletedTask;
        }

        public string GetAliasesInString(Alias alias)
        {
            var result = string.Empty;
            var song = MaiCommandInstance.GetSong(alias.Id);
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

        public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
        {
            var songs = MaiCommandInstance.GetSongs(command);

            if (songs == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg("不存在该歌曲")
                    });
                return Task.CompletedTask;
            }

            if (songs.Length == 1)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId,
                    new CqMessage
                    {
                        new CqReplyMsg(source.MessageId),
                        new CqTextMsg(GetAliasesInString(MaiCommandInstance.GetAliasById(songs[0].Id)))
                    });
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
                new CqMessage
                {
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("该别称有多首歌曲匹配：\n" + ids + "\n*发送 \"lps mai alias ID " + idsList[0] + "\" 指令即可查询歌曲 " +
                                  songs[0].Title + " [" + songs[0].Type +
                                  "] 的别称")
                });

            return Task.CompletedTask;
        }
    }
}
