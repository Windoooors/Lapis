using System.Text;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.MaiCommands;

public class CutoffPointCommand : MaiCommandBase
{
    public CutoffPointCommand()
    {
        CommandHead = "cutoff";
        DirectCommandHead = "cutoff|分数线";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("cutoff", "1");
        IntendedArgumentCount = 4;
        SubCommands = [];
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!SettingsPool.GetValue(new SettingsIdentifierPair("litecommand", "1"), source.GroupId))
            return;

        var difficultyRegex = new Regex("^(绿|黄|红|紫|白|难度一|难度二|)");
        var achievementRegex = new Regex(@"(S|S\+|SS|SS\+|SSS|SSS\+|鸟|鸟加|\d+.\d+|\d+)(分数线|详情)$");

        if (!(difficultyRegex.IsMatch(command) && achievementRegex.IsMatch(command)))
            return;

        var songName = achievementRegex.Replace(difficultyRegex.Replace(command, "", 1), "", 1).Trim();

        if (songName.Length != 0)
            ParseWithArgument(
                [
                    songName,
                    difficultyRegex.Match(command).ToString(),
                    achievementRegex.Match(command).ToString().Replace("分数线", "").Replace("详情", ""),
                    command.Contains("详情").ToString()
                ],
                command, source);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        if (!MaiCommandInstance.TryGetSongs(arguments[0], out var songs,
                new CommandBehaviorInformationDataObject("cutoff", "分数线",
                    [arguments[1] == "" ? "绿" : arguments[1], arguments[2]]),
                source, true))
            return;

        if (songs.Length != 1)
        {
            SendMessage(source, [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(GetMultiAliasesMatchedInformationString(songs,
                    new CommandBehaviorInformationDataObject("cutoff", "分数线",
                        [arguments[1] == "" ? "绿" : arguments[1], arguments[2]])))
            ]);
            return;
        }

        var song = songs[0];

        var difficultyIndexIsInt = int.TryParse(arguments[1], out var difficultyIndex);

        if (!difficultyIndexIsInt)
            difficultyIndex = arguments[1].ToLower() switch
            {
                "绿" => 0,
                "绿谱" => 0,
                "basic" => 0,
                "bas" => 0,
                "难度一" => 0,
                "1" => 0,

                "黄" => 1,
                "黄谱" => 1,
                "advanced" => 1,
                "avd" => 1,
                "难度二" => 1,
                "2" => 1,

                "红" => 2,
                "红谱" => 2,
                "expert" => 2,
                "exp" => 2,

                "紫" => 3,
                "紫谱" => 3,
                "master" => 3,
                "mas" => 3,

                "白" => 4,
                "白谱" => 4,
                "re:master" => 4,
                "remaster" => 4,
                "re" => 4,
                "remas" => 4,
                "re:mas" => 4,

                _ => song.Charts.Length - 1
            };

        if (song.Charts.Length - 1 < difficultyIndex || difficultyIndex < 0)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "不支持的难度参数"]);
            return;
        }

        var targetAchievementIsFloat = float.TryParse(arguments[2], out var targetAchievement);

        if (!targetAchievementIsFloat)
            targetAchievement = arguments[2].ToLower() switch
            {
                "S" => 97f,
                "S+" => 98f,
                "SS" => 99f,
                "SS+" => 99.5f,

                "SSS" => 100,
                "鸟" => 100,

                "SSS+" => 100.5f,
                "鸟加" => 100.5f,

                _ => -1
            };

        if (targetAchievement < 0 || targetAchievement > 101)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "不支持的达成率参数"]);
            return;
        }

        var notes = song.Charts[difficultyIndex].Notes;

        var stringBuilder =
            new StringBuilder(
                $"谱面详情：\nTAP 数量：{notes[0]}\nHOLD 数量：{notes[1]}\nSLIDE 数量：{notes[2]}\nTOUCH 数量：{(song.Type == "DX" ? notes[3] : "0")}\nBREAK 数量：{(song.Type == "DX" ? notes[4] : notes[3])}\n");

        var totalBasicScore = (notes[0] + (song.Type == "DX" ? notes[3] : 0)) * 500 +
                              notes[1] * 1000 + notes[2] * 1500 + (song.Type == "DX" ? notes[4] : notes[3]) * 2500;
        var totalBreakScore = (song.Type == "DX" ? notes[4] : notes[3]) * 100;
        var totalScore = totalBasicScore + totalBreakScore;

        var singleMissTapAchievementLoss = 50000f / totalBasicScore;
        var singleGoodTapAchievementLoss = 25000f / totalBasicScore;
        var singleGreatTapAchievementLoss = 10000f / totalBasicScore;

        stringBuilder.AppendLine($"（FiNALE）总分数：{totalScore}");

        stringBuilder.AppendLine("\n在其余音符所得分数均为最高的情况下，该谱面允许：");

        var currentAchievement = 101f;
        var goodCount = 0;
        while (currentAchievement - singleGoodTapAchievementLoss > targetAchievement)
        {
            currentAchievement -= singleGoodTapAchievementLoss;
            goodCount++;
        }

        currentAchievement = 101f;
        var greatCount = 0;
        while (currentAchievement - singleGreatTapAchievementLoss > targetAchievement)
        {
            currentAchievement -= singleGreatTapAchievementLoss;
            greatCount++;
        }

        currentAchievement = 101f;
        var missCount = 0;
        while (currentAchievement - singleMissTapAchievementLoss > targetAchievement)
        {
            currentAchievement -= singleMissTapAchievementLoss;
            missCount++;
        }

        stringBuilder.AppendLine(
            $"\n打出 {greatCount} 个 GREAT TAP （-{singleGreatTapAchievementLoss.ToString("0.0000")}%/GREAT (400) TAP）");
        stringBuilder.AppendLine(
            $"或打出 {goodCount} 个 GOOD TAP （-{singleGoodTapAchievementLoss.ToString("0.0000")}%/GOOD (250) TAP）");
        stringBuilder.AppendLine(
            $"或打出 {missCount} 个 MISS TAP （-{singleMissTapAchievementLoss.ToString("0.0000")}%/MISS (0) TAP）");

        if (arguments.Length > 3)
        {
            if (arguments[3].ToLower() is not "true" and not "false")
            {
                SendMessage(source, [new CqReplyMsg(source.MessageId), "是否显示详情参数类型错误！应为 true 或 false"]);
                return;
            }

            if (arguments[3].ToLower() == "true")
            {
                stringBuilder.AppendLine("\n附：");

                stringBuilder.AppendLine($"-{(20000f / totalBasicScore).ToString("0.0000")}%/GREAT (800) HOLD");
                stringBuilder.AppendLine($"-{(50000f / totalBasicScore).ToString("0.0000")}%/GOOD (500) HOLD");
                stringBuilder.AppendLine($"-{(100000f / totalBasicScore).ToString("0.0000")}%/MISS (0) HOLD");

                stringBuilder.AppendLine("");

                stringBuilder.AppendLine($"-{(30000f / totalBasicScore).ToString("0.0000")}%/GREAT (1200) SLIDE");
                stringBuilder.AppendLine($"-{(75000f / totalBasicScore).ToString("0.0000")}%/GOOD (750) SLIDE");
                stringBuilder.AppendLine($"-{(150000f / totalBasicScore).ToString("0.0000")}%/MISS (0) SLIDE");

                stringBuilder.AppendLine("");

                stringBuilder.AppendLine(
                    $"-{(25f / totalBreakScore).ToString("0.0000")}%/PERFECT (2550) BREAK");
                stringBuilder.AppendLine(
                    $"-{(50f / totalBreakScore).ToString("0.0000")}%/PERFECT (2500) BREAK");
                stringBuilder.AppendLine(
                    $"-{(50000f / totalBasicScore + 60f / totalBreakScore).ToString("0.0000")}%/GREAT (2000) BREAK");
                stringBuilder.AppendLine(
                    $"-{(100000f / totalBasicScore + 60f / totalBreakScore).ToString("0.0000")}%/GREAT (1500) BREAK");
                stringBuilder.AppendLine(
                    $"-{(125000f / totalBasicScore + 60f / totalBreakScore).ToString("0.0000")}%/GREAT (1250) BREAK");
                stringBuilder.AppendLine(
                    $"-{(150000f / totalBasicScore + 70f / totalBreakScore).ToString("0.0000")}%/GOOD (1000) BREAK");
                stringBuilder.AppendLine(
                    $"-{(250000f / totalBasicScore + 100f / totalBreakScore).ToString("0.0000")}%/MISS (0) BREAK");
            }
        }

        SendMessage(source, [new CqReplyMsg(source.MessageId), stringBuilder.ToString().Trim()]);
    }
}