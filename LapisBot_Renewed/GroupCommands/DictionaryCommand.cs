using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Action;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{

    
    public class DictionaryCommand : VocabularyCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^dictionary\s|^dict\s|^查词\s|^词典\s|^inquiry\s");
            DirectCommand = new Regex(@"^dictionary\s|^dict\s|^查词\s|^词典\s|^inquiry\s");
            DefaultSettings.SettingsName = "词典";
            CurrentGroupCommandSettings = DefaultSettings.Clone();
            if (!Directory.Exists(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + CurrentGroupCommandSettings.SettingsName +
                                          " Settings");

            }

            foreach (string path in Directory.GetFiles(AppContext.BaseDirectory +
                                                       CurrentGroupCommandSettings.SettingsName + " Settings"))
            {
                var settingsString = File.ReadAllText(path);
                settingsList.Add(JsonConvert.DeserializeObject<GroupCommandSettings>(settingsString));
            }

            return Task.CompletedTask;
        }
        
        public override Task Parse(string command, CqGroupMessagePostContext source)
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
            CancelCoolDownTimer(source.GroupId.ToString());
            return Task.CompletedTask;
        }
    }
}
