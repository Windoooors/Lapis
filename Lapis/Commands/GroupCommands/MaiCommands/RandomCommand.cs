using System;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.ImageGenerators;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class RandomCommand : MaiCommandBase
{
    public RandomCommand()
    {
        CommandHead = "random";
        DirectCommandHead = "random|随个";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("random", "1");
        IntendedArgumentCount = 1;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        var command = arguments[0];

        var songs = MaiCommandInstance.GetSongsUsingDifficultyString(command);
        if (songs.Length == 0)
        {
            SendMessage(source, [new CqTextMsg("不支持的等级名称")]);
        }
        else
        {
            if (songs.Length == 0)
            {
                SendMessage(source, [new CqTextMsg("不支持的等级名称")]);
                return;
            }

            var random = new Random();
            var j = random.Next(0, songs.Length);

            var isCompressed =
                SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqImageMsg("base64://" + new InfoImageGenerator().Generate(songs[j], "随机歌曲", null,
                    isCompressed))
            ]);

            if (SettingsPool.GetValue(new SettingsIdentifierPair("random", "2"), source.GroupId))
                SendMessage(source,
                [
                    new CqRecordMsg("file:///" + GetSongPath(songs[j].Id))
                ]);
        }
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        var originalCommandString = command;

        if (command.StartsWith("随个") && !command.Replace("随个", "").StartsWith(' '))
            command = command.Replace("随个", "");
        else
            return;

        ParseWithArgument([command], originalCommandString, source);
    }
}