using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace LapisBot_Renewed.GroupCommands
{
    public class VocabularyCommand : GroupCommand
    {
        public class WordDto
        {
            [JsonProperty("word")] public string Word;
            [JsonProperty("translations")] public TranslationDto[] Translations;
            public class TranslationDto
            {
                [JsonProperty("translation")] public string Translation;
                [JsonProperty("type")] public string Type;
            }
        }

        public class Vocabulary
        {
            public WordDto[] Words;
        }
        
        public List<Vocabulary> Vocabularies = new List<Vocabulary>();
        
        public override Task Initialize()
        {
            foreach (string file in Directory.GetFiles(AppContext.BaseDirectory + "resource/vocabulary/"))
            {
                if (Path.GetFileName(file) == ".DS_Store")
                    continue;
                var jsonString = File.ReadAllText(file);
                Vocabularies.Add(new Vocabulary() { Words = JsonConvert.DeserializeObject<WordDto[]>(jsonString)});
            }
            
            SubCommands.Add(new GuessWordsCommand() { Vocabularies = Vocabularies, ParentCommand = this});
            SubCommands.Add(new DictionaryCommand() { Vocabularies = Vocabularies, ParentCommand = this});
            
            foreach (VocabularyCommand vocabularyCommand in SubCommands)
            {
                vocabularyCommand.Initialize();
            }

            return Task.CompletedTask;
        }
    }
}
