using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.ImageGenerators;
using Lapis.Settings;
using Xabe.FFmpeg;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public static class AudioEditor
{
    public static string Convert(int id)
    {
        try
        {
            if (!Directory.Exists(AppContext.BaseDirectory +
                                  "temp/" + id + "/"))
                Directory.CreateDirectory(AppContext.BaseDirectory +
                                          "temp/" + id + "/");

            var info = FFmpeg.GetMediaInfo(AppContext.BaseDirectory + "resource/tracks/" + id + ".mp3").Result;
            var duration = (int)info
                .Duration
                .TotalSeconds;

            var startTime = TimeSpan.FromSeconds(new Random().Next(10, duration - 13));

            var outputDuration = TimeSpan.FromSeconds(3);

            if (File.Exists(AppContext.BaseDirectory +
                            "temp/" + id + "/" +
                            startTime.TotalSeconds + ".mp3"))
                return new string(AppContext.BaseDirectory +
                                  "temp/" + id + "/" +
                                  startTime.TotalSeconds + ".mp3");

            var conversion = FFmpeg.Conversions.New();

            conversion.AddStream(info.AudioStreams).AddParameter($"-ss {startTime} -t {outputDuration}")
                .SetOutput(AppContext.BaseDirectory +
                           "temp/" + id + "/" +
                           startTime.TotalSeconds + ".mp3");

            var result = conversion.Start().Result;

            return new string(AppContext.BaseDirectory +
                              "temp/" + id + "/" +
                              startTime.TotalSeconds + ".mp3");
        }
        catch
        {
            return "";
        }
    }
}

public class GuessCommand : MaiCommandBase
{
    private readonly Dictionary<string, (int, DateTime)> _guessingGroupsMap = new();

    public GuessCommand()
    {
        CommandHead = "guess";
        DirectCommandHead = "songs|猜歌|song";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("song", "1");
        IntendedArgumentCount = 1;
    }

    public override void Initialize()
    {
        Program.TimeChanged += TimeChanged;
    }

    private void TimeChanged(object obj, EventArgs e)
    {
        if (_guessingGroupsMap.Count == 0)
            return;
        for (var i = 0; i < _guessingGroupsMap.Count; i++)
        {
            if (_guessingGroupsMap.Values.ToArray()[i].Item2 > DateTime.Now)
                continue;
            var keyIdDateTimePair = _guessingGroupsMap.Values.ToArray()[i];
            var groupId = _guessingGroupsMap.Keys.ToArray()[i];
            _guessingGroupsMap.Remove(groupId);
            var taskAnnounce = new Task(() =>
                AnnounceAnswer(keyIdDateTimePair, groupId, false, 0));
            taskAnnounce.Start();
        }
    }

    public override void Parse(string originalPlainMessage, CqGroupMessagePostContext source)
    {
        StartGuessing(source);
    }

    private void StartGuessing(CqGroupMessagePostContext source, SongDto[] songs)
    {
        if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
        {
            var random = new Random();
            var songIndex = random.Next(0, songs.Length);
            _guessingGroupsMap.Add(source.GroupId.ToString(),
                (songs[songIndex].Id, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg($"试试看吧！{BotConfiguration.Instance.BotName} 将在 30s 后公布答案")
            ]);

            SendMessage(source,
                [new CqRecordMsg("file:///" + AudioEditor.Convert(songs[songIndex].Id))]);
        }
        else
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps mai guess answer\"")
            ]);
        }
    }

    private void StartGuessing(CqGroupMessagePostContext source)
    {
        if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
        {
            var random = new Random();
            var songIndex = random.Next(0, MaiCommandInstance.Songs.Length);
            _guessingGroupsMap.Add(source.GroupId.ToString(),
                (MaiCommandInstance.Songs[songIndex].Id, DateTime.Now.Add(new TimeSpan(0, 0, 0, 30))));
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg($"试试看吧！{BotConfiguration.Instance.BotName} 将在 30s 后公布答案")
            ]);

            SendMessage(source,
                [new CqRecordMsg("file:///" + AudioEditor.Convert(MaiCommandInstance.Songs[songIndex].Id))]);
        }
        else
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("本次游戏尚未结束，要提前结束游戏，请发送指令 \"lps mai guess answer\"")
            ]);
        }
    }

    private void AnnounceAnswer((int, DateTime) keyIdDateTimePair, string groupId, bool won, long messageId)
    {
        _guessingGroupsMap.Remove(groupId);

        var text = won ? "Bingo! 答案是：" : "游戏结束啦！ 答案是：";

        var isCompressed =
            SettingsPool.GetValue(new SettingsIdentifierPair("compress", "1"), long.Parse(groupId));

        var image = new InfoImageGenerator().Generate(MaiCommandInstance.GetSong(keyIdDateTimePair.Item1),
            "谜底", null, isCompressed);

        if (messageId == 0)
            SendMessage(long.Parse(groupId),
                [new CqTextMsg(text), new CqImageMsg("base64://" + image)]);
        else
            SendMessage(long.Parse(groupId),
                [new CqReplyMsg(messageId), new CqTextMsg(text), new CqImageMsg("base64://" + image)]);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (arguments[0] == "answer")
        {
            if (!_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
            {
                SendMessage(source,
                [
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("没有游戏正在进行喔！发送指令 \"l mai guess\" 即可开启新一轮的游戏")
                ]);
                return;
            }

            for (var i = 0; i < _guessingGroupsMap.Count; i++)
                if (_guessingGroupsMap.Keys.ToArray()[i] == source.GroupId.ToString())
                    AnnounceAnswer(_guessingGroupsMap.Values.ToArray()[i], source.GroupId.ToString(), false,
                        source.MessageId);

            return;
        }

        var songs = MaiCommandInstance.GetSongsUsingDifficultyString(arguments[0]);

        if (songs.Length == 0)
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("不支持的等级名称")
            ]);
            return;
        }

        StartGuessing(source, songs);
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        var passed = false;

        if (_guessingGroupsMap.ContainsKey(source.GroupId.ToString()))
        {
            _guessingGroupsMap.TryGetValue(source.GroupId.ToString(), out var keyIdDateTimePair);

            MaiCommandInstance.TryGetSongs(command, out var songs);
            if (songs.Length != 0)
                passed = true;
            foreach (var song in songs)
                if (passed && song.Id == keyIdDateTimePair.Item1)
                {
                    var task = new Task(() =>
                        AnnounceAnswer(keyIdDateTimePair, source.GroupId.ToString(), true, source.MessageId));
                    task.Start();

                    return;
                }
        }

        if (passed)
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("不对哦")
            ]);
    }
}