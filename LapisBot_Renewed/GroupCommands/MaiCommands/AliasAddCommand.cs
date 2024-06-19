using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;

namespace LapisBot_Renewed.GroupCommands.MaiCommands
{
    public class AliasAddCommand : MaiCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^alias add\s");
            DirectCommand = new Regex(@"^添加别名\s");
            DefaultSettings.SettingsName = "添加别名";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");
            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            return Task.CompletedTask;
        }

        public override Task Parse(string command, GroupMessageReceiver source)
        {
            var cmds = command.Split(" ");

            if(cmds.Length > 0)
            {

                var oname = "";
                for(int i = 1;i < cmds.Length;i++)
                {
                    oname+=cmds[i] + " ";
                }


                if(oname == "")
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId, "没有别名吗...那我怎么知道要添加什么啊喵！");
                }
                else
                {
                    var songs = MaiCommandCommand.GetSongs(cmds[0]);

                    if(songs == null || songs.Length == 0)
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, "添加失败！找不到歌曲！");
                    }
                    else if(songs.Length > 1)
                    {
                        MessageManager.SendGroupMessageAsync(source.GroupId, "添加失败！无法确定是哪个歌曲！");
                    }
                    else
                    {
                        var id = songs[0].Id;
                        
                        var success = LocalAlias.singleton.Add(id,oname);
                        if(success)MessageManager.SendGroupMessageAsync(source.GroupId, "添加成功！");
                        else MessageManager.SendGroupMessageAsync(source.GroupId, "已经存在此别名");
                    }
                    
                }

            }
            else
            {
                MessageManager.SendGroupMessageAsync(source.GroupId, "没有参数吗...那我怎么知道要添加什么啊喵！");
            }

            return Task.CompletedTask;
        }
    }

    public class LocalAlias
    {
        Dictionary<int,HashSet<string>> alias = new();

        public bool Add(int originalName,string alia)
        {
            if(!alias.ContainsKey(originalName))alias.Add(originalName,new());

            if(alias[originalName].Contains(alia))return false;
            else
            {
                alias[originalName].Add(alia);
                return true;
            }
        }
        public bool Remove(int originalId,string alia)
        {
            if(!alias.ContainsKey(originalId))return false;
            if(alias[originalId].Contains(alia))
            {
                alias[originalId].Remove(alia);
                return true;
            }
            else return false;
        }
        public bool RemoveAll(int originalId)
        {
            if(!alias.ContainsKey(originalId))return false;
            else
            {
                alias.Remove(originalId);
                return true;
            }
        }
        public HashSet<string> Get(int originalId)
        {
            
            if(!alias.ContainsKey(originalId))return null;
            else return alias[originalId];
        }
        public Dictionary<int,HashSet<string>>.KeyCollection GetKeyCollection()
        {
            return alias.Keys;
        }

        private LocalAlias(){}

        public static LocalAlias singleton{get;} = new();
    }
}