using System.Collections.Generic;

namespace Lapis.Miscellaneous;

public static class SharedConsts
{
    public const string ChineseCharacterRegexString = @"[\u4e00-\u9fa5]";
    public const string JapaneseCharacterRegexString = @"[\u3040-\u30FF\u31F0-\u31FF\uFF00-\uFFEF]";

    public static readonly Dictionary<string, string> PlateCharacterToVersionName = new()
    {
        { "真", "maimai PLUS" },
        { "超", "maimai GreeN" },
        { "檄", "maimai GreeN PLUS" },
        { "橙", "maimai ORANGE" },
        { "暁", "maimai ORANGE PLUS" },
        { "桃", "maimai PiNK" },
        { "櫻", "maimai PiNK PLUS" },
        { "紫", "maimai MURASAKi" },
        { "菫", "maimai MURASAKi PLUS" },
        { "白", "maimai MiLK" },
        { "雪", "MiLK PLUS" },
        { "輝", "maimai FiNALE" },
        { "舞", "maimai ALL" },
        { "熊", "maimai でらっくす" },
        { "華", "maimai でらっくす" },
        { "爽", "maimai でらっくす Splash" },
        { "煌", "maimai でらっくす Splash" },
        { "宙", "maimai でらっくす UNiVERSE" },
        { "星", "maimai でらっくす UNiVERSE" },
        { "祭", "maimai でらっくす FESTiVAL" },
        { "祝", "maimai でらっくす FESTiVAL" },
        { "双", "maimai でらっくす BUDDiES" },
        { "宴", "maimai でらっくす BUDDiES" },
        { "镜", "maimai でらっくす PRiSM" },
        { "彩", "maimai でらっくす PRiSM" }
    };

    public static readonly Dictionary<string, List<string>> VersionNameToPlateCharacters = new()
    {
        ["maimai"] = ["真"],
        ["maimai PLUS"] = ["真"],
        ["maimai GreeN"] = ["超"],
        ["maimai GreeN PLUS"] = ["檄"],
        ["maimai ORANGE"] = ["橙"],
        ["maimai ORANGE PLUS"] = ["暁"],
        ["maimai PiNK"] = ["桃"],
        ["maimai PiNK PLUS"] = ["櫻"],
        ["maimai MURASAKi"] = ["紫"],
        ["maimai MURASAKi PLUS"] = ["菫"],
        ["maimai MiLK"] = ["白"],
        ["MiLK PLUS"] = ["雪"],
        ["maimai FiNALE"] = ["輝"],
        ["maimai ALL"] = ["舞"],
        ["maimai でらっくす"] = ["熊", "華"],
        ["maimai でらっくす Splash"] = ["爽", "煌"],
        ["maimai でらっくす UNiVERSE"] = ["宙", "星"],
        ["maimai でらっくす FESTiVAL"] = ["祭", "祝"],
        ["maimai でらっくす BUDDiES"] = ["双", "宴"],
        ["maimai でらっくす PRiSM"] = ["镜", "彩"]
    };

    public static readonly Dictionary<string, string> DxVersionToChineseVersionName = new()
    {
        { "maimai でらっくす", "舞萌DX" },
        { "maimai でらっくす Splash", "舞萌DX 2021" },
        { "maimai でらっくす UNiVERSE", "舞萌DX 2022" },
        { "maimai でらっくす FESTiVAL", "舞萌DX 2023" },
        { "maimai でらっくす BUDDiES", "舞萌DX 2024" },
        { "maimai でらっくす PRiSM", "舞萌DX 2025" }
    };

    public static Dictionary<string, string> Categories = new()
    {
        { "流行&动漫", "anime" },
        { "舞萌", "maimai" },
        { "niconico & VOCALOID", "niconico" },
        { "东方Project", "touhou" },
        { "其他游戏", "game" },
        { "音击&中二节奏", "ongeki" },
        { "POPSアニメ", "anime" },
        { "maimai", "maimai" },
        { "niconicoボーカロイド", "niconico" },
        { "東方Project", "touhou" },
        { "ゲームバラエティ", "game" },
        { "オンゲキCHUNITHM", "ongeki" },
        { "宴会場", "宴会场" }
    };

    public static Dictionary<string, string> Characters = new()
    {
        { "晓", "暁" },
        { "樱", "櫻" },
        { "堇", "菫" },
        { "辉", "輝" },
        { "华", "華" }
    };

    public static readonly string[] SpecialCharacters =
    [
        ".", ",", ";", ":", "?", "!", "\"", "'",
        "(", ")", "[", "]", "{", "}", "<", ">",
        "~", "-", "=", "+", "*", "/", "\\",
        "%", "&", "#", "@", "$", "。", "，", "；", "：", "？", "！", "“”", "‘’",
        "（", "）", "［", "］", "｛", "｝", "〈", "〉",
        "～", "－", "＝", "＋", "×", "／", "＼",
        "％", "＆", "＃", "＠", "＄", "’"
    ];
}