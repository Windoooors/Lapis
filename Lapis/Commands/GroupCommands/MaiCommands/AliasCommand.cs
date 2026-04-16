using System.Collections.Generic;
using System.Linq;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.MaiCommands.AliasCommands;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public abstract class AliasCommandBase : MaiCommandBase
{
    public static AliasCommand AliasCommandInstance;
}

public class AliasCommand : AliasCommandBase
{
    public AliasCommand()
    {
        AliasCommandInstance = this;
        CommandHead = "alias";
        DirectCommandHead = "alias|别名|查看别名";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("alias", "1");
        SubCommands = [new AddCommand()];
        IntendedArgumentCount = 1;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("lite_command", "1"), source.GroupId))
            return;

        var originalCommandString = command;

        if (command.EndsWith(" 有什么别名"))
            command = command.Replace(" 有什么别名", "");
        else if (command.EndsWith("有什么别名"))
            command = command.Replace("有什么别名", "");
        else
            return;

        ParseWithArgument([command], originalCommandString, source);
    }

    private string GetAliasesInText(SongAlias alias, int songId)
    {
        var song = MaiCommandInstance.GetSongById(songId);
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"歌曲 {song.Title} [{GetSongType(songId)}] 有如下别称：");
        if (alias?.Aliases != null && alias.Aliases.Count != 0)
        {
            var hashSet = new HashSet<string>(alias.Aliases.Select(x => x.Alias));

            foreach (var aliasString in hashSet) stringBuilder.AppendLine(aliasString);

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }
        else
        {
            stringBuilder = new StringBuilder($"歌曲 {song.Title} [{GetSongType(songId)}] 没有别称");
        }

        return stringBuilder.ToString();
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (!MaiCommandInstance.TryGetSongs(arguments[0], out var songs,
                new CommandBehaviorInformationDataObject("alias", "别称"),
                source, true))
            return;

        if (songs.Length == 1)
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(GetAliasesInText(MaiCommandInstance.GetAliasById(songs[0].SongId), songs[0].SongId))
            ]);
            return;
        }

        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(GetMultiAliasesMatchedInformationString(songs,
                new CommandBehaviorInformationDataObject("alias", "别称")))
        ]);
    }
}