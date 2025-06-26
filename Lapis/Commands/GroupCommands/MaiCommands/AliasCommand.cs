using System.Collections.Generic;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands.MaiCommands.AliasCommands;
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
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        if (command.EndsWith(" 有什么别名"))
            command = command.Replace(" 有什么别名", "");
        else if (command.EndsWith("有什么别名"))
            command = command.Replace("有什么别名", "");
        else
            return;

        ParseWithArgument(command, source);
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

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        var songs = MaiCommandInstance.GetSongs(command);

        if (songs == null)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                GetMultiSearchResultInformationString(command, "alias", "别称")
            ]);
            return;
        }

        if (songs.Length == 1)
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(GetAliasesInText(MaiCommandInstance.GetAliasById(songs[0].Id)))
            ]);
            return;
        }

        SendMessage(source, [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(GetMultiAliasesMatchedInformationString(songs, "alias", "别称"))
        ]);
    }
}