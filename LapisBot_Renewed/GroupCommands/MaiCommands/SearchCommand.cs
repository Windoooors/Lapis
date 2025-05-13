using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Collections;
using LapisBot_Renewed.Settings;
using Raffinert.FuzzySharp;

namespace LapisBot_Renewed.GroupCommands.MaiCommands;

public class SearchCommand : MaiCommandBase
{
    public override Task Unload()
    {
        return Task.CompletedTask;
    }

    public SearchCommand()
    {
        CommandHead = new Regex("^search");
        DirectCommandHead = new Regex("^search|^查歌|^搜歌|^搜索|^索引");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("search", "1");
    }

    public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        var maiCommand = MaiCommandInstance;

        var songAliasDict = new Dictionary<SongDto, List<string>>();
        var songs = new List<SongDto>();

        foreach (var song in maiCommand.Songs)
        {
            var aliases = maiCommand.GetAliasById(song.Id).Aliases;

            foreach (var alias in aliases)
            {
                var ratio = Fuzz.Ratio(command, alias);
                if (ratio >= 60)
                {
                    if (!songAliasDict.ContainsKey(song))
                    {
                        songAliasDict.Add(song, [alias]);
                        continue;
                    }

                    songAliasDict[song].Add(alias);
                }
            }

            if (Fuzz.Ratio(command, song.Title) >= 50)
                songs.Add(song);
        }

        var text = "找到了以下歌曲：\n";

        if (songs.Count > 0)
            foreach (var song in songs)
            {
                text += "ID " + song.Id + " - " + song.Title + " [" + song.Type + "] （通过标题匹配）\n";
            }

        if (songAliasDict.Count > 0)
            foreach (var pair in songAliasDict)
            {
                var aliasText = "";
                foreach (var alias in pair.Value)
                {
                    aliasText += $"\"{alias}\" ";
                }

                text += "ID " + pair.Key.Id + " - " + pair.Key.Title + " [" + pair.Key.Type + $"] （通过别称 {aliasText}匹配）\n";
            }

        var id = 0;
        if (songs.Count > 0 && songAliasDict.Count == 0)
            id = songs[0].Id;
        if (songs.Count == 0 && songAliasDict.Count > 0)
            id = songAliasDict.Keys.ToArray()[0].Id;
        if (songs.Count > 0 && songAliasDict.Count > 0)
            id = songs[0].Id;
        if (songs.Count == 0 && songAliasDict.Count == 0)
            id = -1;
        
        if (id != -1)
            text += $"*发送 \"lps mai info ID { id}\" 指令即可查询歌曲 {
                maiCommand.GetSong(id).Title} [{maiCommand.GetSong(id).Type}] 的信息";
        else
            text = "未找到歌曲";
        
        Program.Session.SendGroupMessageAsync(source.GroupId,
            new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(text)
            });

        return Task.CompletedTask;
    }
}

