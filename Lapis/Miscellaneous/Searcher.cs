using System.Linq;
using System.Text.RegularExpressions;

namespace Lapis.Miscellaneous;

public class Searcher
{
    private readonly string[] _specialCharacters =
    [
        ".", ",", ";", ":", "?", "!", "\"", "'",
        "(", ")", "[", "]", "{", "}", "<", ">",
        "~", "-", "=", "+", "*", "/", "\\",
        "%", "&", "#", "@", "$", "。", "，", "；", "：", "？", "！", "“”", "‘’",
        "（", "）", "［", "］", "｛", "｝", "〈", "〉",
        "～", "－", "＝", "＋", "×", "／", "＼",
        "％", "＆", "＃", "＠", "＄"
    ];

    public static Searcher Instance { get; } = new();

    public bool IsMatch(string keyword, string input)
    {
        var inputWithNoSpecialCharacters = input;
        foreach (var specialCharacter in _specialCharacters)
            inputWithNoSpecialCharacters = inputWithNoSpecialCharacters.Replace(specialCharacter, string.Empty);

        var kanjiKeyword = HanziToKanjiConverter.Convert(keyword);

        return IsMatchBase(keyword, input) ||
               IsMatchBase(keyword, inputWithNoSpecialCharacters) ||
               IsMatchBase(kanjiKeyword, input) ||
               IsMatchBase(kanjiKeyword, inputWithNoSpecialCharacters);
    }

    private bool IsSameKeywords(Match[] keywords)
    {
        var firstKeyword = keywords[0].Value;
        foreach (var keyword in keywords)
            if (keyword.Value != firstKeyword)
                return false;

        return true;
    }


    private bool IsMatchBase(string pattern, string input)
    {
        if (pattern.ToLower().Equals(input.ToLower()))
            return true;

        var matchResult =
            new Regex(
                    $"[a-zA-Z0-9]+|{SharedConsts.ChineseCharacterRegexString}|{SharedConsts.JapaneseCharacterRegexString}")
                .Matches(pattern);

        var patterns = matchResult.ToArray();
        var allMatched = true;

        if (patterns.Length > 1 && IsSameKeywords(patterns))
        {
            var regex = new Regex(patterns[0].Value);

            var matches = regex.Matches(input.ToLower());
            if (matches.Count == patterns.Length)
                return true;
            return false;
        } // 处理例如通过 break break break 的关键词匹配歌曲 BREaK! BREaK! BREaK! 的情况

        foreach (var match in patterns)
        {
            var regex = new Regex(match.Value);

            if (!regex.IsMatch(input.ToLower()))
                allMatched = false;
        }

        return allMatched;
    }
}