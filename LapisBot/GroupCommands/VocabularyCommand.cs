using System;
using System.Collections.Generic;
using System.IO;
using LapisBot.GroupCommands.VocabularyCommands;
using Newtonsoft.Json;

namespace LapisBot.GroupCommands;

public abstract class VocabularyCommandBase : GroupCommand
{
    protected static VocabularyCommand VocabularyCommandInstance;

    public class WordDto
    {
        [JsonProperty("translations")] public TranslationDto[] Translations;
        [JsonProperty("word")] public string Word;

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
}

public class VocabularyCommand : VocabularyCommandBase
{
    public readonly List<Vocabulary> Vocabularies = [];

    public VocabularyCommand()
    {
        VocabularyCommandInstance = this;
        SubCommands = [new GuessWordsCommand(), new DictionaryCommand()];
    }

    public override void Initialize()
    {
        foreach (var file in Directory.GetFiles(AppContext.BaseDirectory + "resource/vocabulary/"))
        {
            if (Path.GetFileName(file) == ".DS_Store")
                continue;
            var jsonString = File.ReadAllText(file);
            Vocabularies.Add(new Vocabulary { Words = JsonConvert.DeserializeObject<WordDto[]>(jsonString) });
        }
    }
}