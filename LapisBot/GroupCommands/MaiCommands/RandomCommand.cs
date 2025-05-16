using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.ImageGenerators;
using LapisBot.Settings;

namespace LapisBot.GroupCommands.MaiCommands;

public class RandomCommand : MaiCommandBase
{
    public RandomCommand()
    {
        CommandHead = new Regex("^random");
        DirectCommandHead = new Regex("^random|^随个");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("random", "1");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        var songs = MaiCommandInstance.GetSongsUsingDifficultyString(command);
        if (songs.Length == 0)
        {
            Program.Session.SendGroupMessageAsync(source.GroupId, [new CqTextMsg("不支持的等级名称")]);
        }
        else
        {
            if (songs.Length == 0)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, [new CqTextMsg("不支持的等级名称")]);
                return;
            }

            var random = new Random();
            var j = random.Next(0, songs.Length);

            var isCompressed =
                SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("compress", "1"), source.GroupId);

            Program.Session.SendGroupMessageAsync(source.GroupId,
            [
                new CqReplyMsg(source.MessageId),
                new CqImageMsg("base64://" + new InfoImageGenerator().Generate(songs[j], "随机歌曲", null,
                    isCompressed))
            ]);

            if (SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("random", "2"), source.GroupId))
                Program.Session.SendGroupMessageAsync(source.GroupId,
                [
                    new CqRecordMsg("file:///" + GetSongPath(songs[j].Id))
                ]);
        }
    }
    
    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsCommand.Instance.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        if (command.StartsWith("随个") && !command.Replace("随个", "").StartsWith(' '))
            command = command.Replace("随个", "");
        else
            return;

        ParseWithArgument(command, source);
    }
}