using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Sessions.Http.Managers;
using System.IO;
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
        
        public override Task Parse(string command, GroupMessageReceiver source)
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
                MessageManager.SendGroupMessageAsync(source.GroupId,
                    new MessageChain() { new AtMessage() { Target = source.Sender.Id }, new PlainMessage(" 未查询到该词语") });
                return Task.CompletedTask;
            }

            var text = string.Empty;
            text += "\n查询结果：" + targetWordItem.Word + " ";
            foreach (WordDto.TranslationDto translation in targetWordItem.Translations)
                text += translation.Type + "." + translation.Translation + "; \n";
            text.TrimEnd();
            
            MessageManager.SendGroupMessageAsync(source.GroupId,
                new MessageChain() { new AtMessage() { Target = source.Sender.Id }, new PlainMessage(" " + text) });
            CancelCoolDownTimer(source.GroupId);
            return Task.CompletedTask;
        }
    }
}
