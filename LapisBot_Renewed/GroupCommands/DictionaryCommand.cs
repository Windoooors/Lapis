using System.Threading.Tasks;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Settings;

namespace LapisBot_Renewed.GroupCommands
{

    
    public class DictionaryCommand : VocabularyCommand
    {
        public DictionaryCommand()
        {
            CommandHead = new ("^dictionary|^dict|^查词|^词典|^inquiry");
            DirectCommandHead = new ("^dictionary|^dict|^查词|^词典|^inquiry");
            ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("dictionary", "1");
        }
        
        public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
        {
            WordDto targetWordItem = null;
            foreach (Vocabulary vocabulary in Vocabularies)
            {
                foreach (WordDto wordItem in vocabulary.Words)
                {
                    if (wordItem.Word == command)
                    {
                        targetWordItem = wordItem;
                        break;
                    }
                }
            }

            if (targetWordItem == null)
            {
                Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
                {
                    new CqReplyMsg(source.MessageId),
                    new CqTextMsg("未查询到该词语")
                });
                return Task.CompletedTask;
            }

            var text = string.Empty;
            text += "\n查询结果：" + targetWordItem.Word + " ";
            foreach (WordDto.TranslationDto translation in targetWordItem.Translations)
                text += translation.Type + "." + translation.Translation + "; \n";
            text.TrimEnd();

            Program.Session.SendGroupMessageAsync(source.GroupId, new CqMessage
            {
                new CqReplyMsg(source.MessageId),
                new CqTextMsg(text)
            });

            return Task.CompletedTask;
        }
    }
}
