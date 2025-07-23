using System.Collections.Generic;

namespace Lapis.Miscellaneous;

public static class SharedConsts
{
    public const string ChineseCharacterRegexString = @"[\u4e00-\u9fa5]";
    public const string JapaneseCharacterRegexString = @"[\u3040-\u30FF\u31F0-\u31FF\uFF00-\uFFEF]";

    public static readonly Dictionary<string, string> PlateVersionToVersionName = new()
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
        { "镜", "maimai でらっくす PRiSM" }
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