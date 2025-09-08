using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Lapis.Settings;

public static class SettingsPool
{
    private static readonly string SavePath = $"{AppContext.BaseDirectory}/data/settings.json";

    private static readonly List<StorageItem> StorageItems = File.Exists(SavePath)
        ? JsonConvert.DeserializeObject<List<StorageItem>>(File.ReadAllText(SavePath))
        : [];

    public static bool GetValue(SettingsIdentifierPair identifierPair, long groupId)
    {
        var defaultValue = true;

        foreach (var category in SettingsItems.Categories)
        foreach (var itemsOfACommand in category.Items)
            if (itemsOfACommand.Identifier == identifierPair.PrimeIdentifier)
                foreach (var item in itemsOfACommand.Items)
                    if (item.Identifier == identifierPair.Identifier)
                        defaultValue = item.DefaultValue;

        var identifierPairString = identifierPair.ToString();

        foreach (var storageItem in StorageItems)
            if (storageItem.IdentifierPairString == identifierPairString && storageItem.GroupId == groupId)
                return storageItem.Value;

        return defaultValue;
    }

    public static bool SetValue(SettingsIdentifierPair identifierPair, long groupId, bool value)
    {
        var primeIdentifierVerified = false;
        var identifierVerified = false;
        foreach (var category in SettingsItems.Categories)
        {
            foreach (var item in category.Items)
            {
                if (item.Identifier != identifierPair.PrimeIdentifier)
                    continue;
                
                primeIdentifierVerified = true;

                identifierVerified = item.Items.Where(option => option.Identifier == identifierPair.Identifier)
                    .ToArray().Length == 1;
            }
        }

        if (!(primeIdentifierVerified && identifierVerified))
            return false;
        
        var found = false;
        var identifierPairString = identifierPair.ToString();
        foreach (var storageItem in StorageItems)
            if (storageItem.IdentifierPairString == identifierPairString && storageItem.GroupId == groupId)
            {
                storageItem.Value = value;
                found = true;
            }

        if (!found)
            StorageItems.Add(new StorageItem(identifierPairString, groupId, value));

        File.WriteAllText(SavePath, JsonConvert.SerializeObject(StorageItems));

        return true;
    }

    private class StorageItem(string identifierPairString, long groupId, bool value)
    {
        public readonly long GroupId = groupId;
        public readonly string IdentifierPairString = identifierPairString;
        public bool Value = value;
    }
}