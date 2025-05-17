using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Settings;
using Raffinert.FuzzySharp;

namespace LapisBot.GroupCommands.MaiCommands;

public class SearchCommand : MaiCommandBase
{
    public SearchCommand()
    {
        CommandHead = new Regex("^search");
        DirectCommandHead = new Regex("^search|^查歌|^搜歌|^搜索|^索引");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("search", "1");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
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

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("找到了以下歌曲：");

        if (songs.Count > 0)
            foreach (var song in songs)
                stringBuilder.AppendLine("ID " + song.Id + " - " + song.Title + " [" + song.Type + "] （通过标题匹配）");

        if (songAliasDict.Count > 0)
            foreach (var pair in songAliasDict)
            {
                var aliasStringBuilder = new StringBuilder();
                foreach (var alias in pair.Value) aliasStringBuilder.AppendJoin(' ', $"\"{alias}\"");

                stringBuilder.AppendLine("ID " + pair.Key.Id + " - " + pair.Key.Title + " [" + pair.Key.Type +
                                         $"] （通过别称 {aliasStringBuilder}匹配）");
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
        {
            stringBuilder.Append($"*发送 \"lps mai info ID {id}\" 指令即可查询歌曲 {
                maiCommand.GetSong(id).Title} [{maiCommand.GetSong(id).Type}] 的信息");
        }
        else
        {
            stringBuilder.Clear();
            stringBuilder = new StringBuilder("未找到歌曲");
        }

        Program.Session.SendGroupMessageAsync(source.GroupId,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(stringBuilder.ToString())
        ]);
    }
}