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

    private bool IsSameKeywords(string[] keywords)
    {
        var firstKeyword = keywords[0];
        foreach (var keyword in keywords)
            if (keyword != firstKeyword)
                return false;

        return true;
    }


    private bool IsMatchBase(string pattern, string input)
    {
        if (pattern.ToLower() == input.ToLower())
            return true;

        var splitResult = pattern.ToLower().Split(' ');
        var patterns = splitResult.Length == 0 ? [pattern.ToLower()] : splitResult;
        var allMatched = true;

        if (patterns.Length > 1 && IsSameKeywords(patterns))
        {
            var regex = new Regex(patterns[0]);
            var matches = regex.Matches(input.ToLower());
            if (matches.Count > 1)
                return true;
            return false;
        } // 处理例如通过 break break break 的关键词匹配歌曲 BREaK! BREaK! BREaK! 的情况

        foreach (var regexString in patterns)
        {
            var regex = new Regex(regexString);

            if (!regex.IsMatch(input.ToLower()))
                allMatched = false;
        }

        return allMatched;
    }
}