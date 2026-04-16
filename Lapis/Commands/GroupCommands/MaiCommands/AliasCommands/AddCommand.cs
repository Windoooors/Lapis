using System.Linq;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;
using Microsoft.EntityFrameworkCore;

namespace Lapis.Commands.GroupCommands.MaiCommands.AliasCommands;

public class AddCommand : AliasCommandBase
{
    public AddCommand()
    {
        CommandHead = "add";
        DirectCommandHead = "添加别名";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("alias_add", "1");
        IntendedArgumentCount = 2;
    }

    private bool TryAddAlias(int id, string alias)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        try
        {
            var findResult = db.SongAliasDataSet.Include(x => x.Aliases)
                .FirstOrDefault(x => x.SimplifiedSongId == id % 10000);

            if (findResult == null)
            {
                db.Add(new SongAlias
                {
                    SimplifiedSongId = id % 10000,
                    Aliases =
                    [
                        new SingleSongAlias
                        {
                            Alias = alias
                        }
                    ]
                });

                db.SaveChanges();

                return true;
            }

            if (findResult.Aliases.Exists(y => y.Alias == alias)) return false;

            findResult.Aliases.Add(new SingleSongAlias
            {
                Alias = alias
            });
            db.SaveChanges();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (arguments.Length < IntendedArgumentCount)
        {
            HelpCommand.Instance.ArgumentErrorHelp(source);
            return;
        }

        if (!MaiCommandInstance.TryGetSongs(arguments[0], out var songs,
                new CommandBehaviorInformationDataObject("alias add", "添加别名", [arguments[1]], true),
                source, true))
            return;

        if (songs.Length > 1)
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(
                    MaiCommandInstance.GetMultiAliasesMatchedInformationString(songs,
                        new CommandBehaviorInformationDataObject("alias add", "添加别名", [arguments[1]], true)))
            ]);
            return;
        }

        var matchedSong = songs[0];
        var intendedAliasString = arguments[1];
        if (intendedAliasString == "")
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("没有别名吗...那我怎么知道要添加什么啊喵！") //喵
            ]);
            return;
        }

        var action = () =>
        {
            var id = matchedSong.SongId;

            var alias = MaiCommandInstance.GetAliasById(id);
            
            var success = ((alias != null && !alias.Aliases
                              .Exists(x => x.Alias == intendedAliasString)) || alias == null) &&
                          TryAddAlias(id, intendedAliasString);

            if (success)
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("添加成功！")
                ]);
            else
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("已存在此别名")
                ]);
        };
        TaskHandleQueue.HandleableTask task = new(source.Sender.UserId, () =>
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("别名添加已取消！")
            ]);
        }, action);
        var success = TaskHandleQueue.Instance.AddTask(task, source.GroupId, source.Sender.UserId);

        if (success)
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(
                    $"您正在尝试为歌曲 \"{matchedSong.Title} [{GetSongType(matchedSong.SongId)}]\" 添加别名 \"{intendedAliasString}\""
                    + "\n发送 \"lps handle confirm\" 以确认，发送 \"lps handle cancel\" 以取消")
            ]);
        else
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("您当前已有代办事项！请处理后再试！")
            ]);
    }
}