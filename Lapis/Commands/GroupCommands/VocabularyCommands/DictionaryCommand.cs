using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.VocabularyCommands;

public class DictionaryCommand : VocabularyCommandBase
{
    private const int MessageLengthThreshold = 256;
    public static DictionaryCommand DictionaryCommandInstance;

    public DictionaryCommand()
    {
        DictionaryCommandInstance = this;
        CommandHead = "dictionary|dict|查词|词典|inquiry";
        DirectCommandHead = "dictionary|dict|查词|词典|inquiry";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("dictionary", "1");
        IntendedArgumentCount = 1;
    }

    private WordDto LookUpInCustomizedDictionary(string word)
    {
        var results = VocabularyCommandInstance.Vocabularies.Select(x =>
        {
            return x.Words.ToList().Find(y => y.Word.ToLower() == word.ToLower());
        }).Where(x => x is not null).ToArray();

        if (results.Length > 0)
            return results[0];

        return null;
    }

    public bool TryLookUp(string word, out WordDto wordItem)
    {
        if (word.Length < 1)
        {
            wordItem = null;
            return false;
        }

        var bucketHeader = word[0].ToString();

        if (word.Length > 1)
            bucketHeader += word[1];

        var hasBucket = VocabularyCommandInstance.LargeVocabulary.Words.TryGetValue(bucketHeader, out var words);

        if (!hasBucket
            || words == null || words.Length == 0)
        {
            wordItem = LookUpInCustomizedDictionary(word);
            return wordItem != null;
        }

        var result = words.ToList().Find(x => x.Word.ToLower() == word.ToLower());

        wordItem = result;

        if (result == null)
        {
            wordItem = LookUpInCustomizedDictionary(word);
            return wordItem != null;
        }

        return true;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        var exists = DictionaryCommandInstance.TryLookUp(arguments[0], out var targetWordItem);

        if (!exists)
        {
            SendMessage(source, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("未查询到该词语")
            });
            return;
        }

        var stringBuilder = new StringBuilder("查询结果：" + targetWordItem.Word);

        stringBuilder.AppendLine();

        foreach (var translation in targetWordItem.Translations)
        {
            stringBuilder.Append(translation.Type + ". ");

            var regex = new Regex(@"[0-9]+\s.*?(?=[0-9]+\s|$)");
            var matches = regex.Matches(translation.Translation);

            if (matches.Count == 0)
                stringBuilder.AppendLine(translation.Translation);
            else
                foreach (Match match in matches)
                    stringBuilder.AppendLine(match.Value);

            stringBuilder.AppendLine();
        }

        var text = stringBuilder.ToString().Trim();

        var forwardedMessage = text.Length >= MessageLengthThreshold;

        SendMessage(source.GroupId, new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(text)
        }, forwardedMessage);
    }
}