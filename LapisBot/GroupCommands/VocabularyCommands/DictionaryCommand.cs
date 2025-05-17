using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot.Settings;

namespace LapisBot.GroupCommands.VocabularyCommands;

public class DictionaryCommand : VocabularyCommandBase
{
    public DictionaryCommand()
    {
        CommandHead = new Regex("^dictionary|^dict|^查词|^词典|^inquiry");
        DirectCommandHead = new Regex("^dictionary|^dict|^查词|^词典|^inquiry");
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("dictionary", "1");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
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
            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
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

        Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
        {
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(text)
        });
    }
}