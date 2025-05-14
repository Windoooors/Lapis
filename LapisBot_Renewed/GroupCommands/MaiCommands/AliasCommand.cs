using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.GroupCommands.MaiCommands.AliasCommands;
using LapisBot.Settings;

namespace LapisBot.GroupCommands.MaiCommands;

public abstract class AliasCommandBase : MaiCommandBase
{
    public static AliasCommand AliasCommandInstance;
}

public class AliasCommand : AliasCommandBase
{
    public AliasCommand()
    {
        AliasCommandInstance = this;
        CommandHead = new Regex("^alias");
        DirectCommandHead = new Regex("^alias|^别名|^查看别名");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("alias", "1");
    }

    public override Task Unload()
    {
        foreach (var aliasCommand in SubCommands)
            aliasCommand.Unload();
        return Task.CompletedTask;
    }


    public override Task RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
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

        foreach (var subAliasCommand in SubCommands) subAliasCommand.Initialize();

        return Task.CompletedTask;
    }

    private string GetAliasesInText(Alias alias)
    {
        var song = MaiCommandInstance.GetSong(alias.Id);
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"歌曲 {song.Title} [{song.Type}] 有如下别称：");
        if (alias.Aliases.Count != 0)
        {
            var hashSet = new HashSet<string>(alias.Aliases);

            foreach (var aliasString in hashSet) stringBuilder.AppendLine(aliasString);

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }
        else
        {
            stringBuilder = new StringBuilder($"歌曲 {song.Title} [{song.Type}] 没有别称");
        }

        return stringBuilder.ToString();
    }

    public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        var songs = MaiCommandInstance.GetSongs(command);

        if (songs == null)
        {
            Program.Session.SendGroupMessageAsync(source.GroupId,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("不存在该歌曲")
            ]);
            return Task.CompletedTask;
        }

        if (songs.Length == 1)
        {
            Program.Session.SendGroupMessageAsync(source.GroupId,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(GetAliasesInText(MaiCommandInstance.GetAliasById(songs[0].Id)))
            ]);
            return Task.CompletedTask;
        }

        Program.Session.SendGroupMessageAsync(source.GroupId, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(GetMultiAliasesMatchedInformationString(songs, "alias", "别称"))
        ]);

        return Task.CompletedTask;
    }
}