using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.VocabularyCommands;

public class DictionaryCommand : VocabularyCommandBase
{
    public DictionaryCommand()
    {
        CommandHead = "dictionary|dict|查词|词典|inquiry";
        DirectCommandHead = "dictionary|dict|查词|词典|inquiry";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("dictionary", "1");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source, long[] mentionedUserIds)
    {
        WordDto targetWordItem = null;
        foreach (var vocabulary in VocabularyCommandInstance.Vocabularies)
        foreach (var wordItem in vocabulary.Words)
            if (wordItem.Word == command)
            {
                targetWordItem = wordItem;
                break;
            }

        if (targetWordItem == null)
        {
            SendMessage(source, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("未查询到该词语")
            });
            return;
        }

        var text = "查询结果：" + targetWordItem.Word + " ";
        foreach (var translation in targetWordItem.Translations)
            text += translation.Type + "." + translation.Translation + "; \n";
        text = text.TrimEnd();

        SendMessage(source, new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(text)
        });
    }
}