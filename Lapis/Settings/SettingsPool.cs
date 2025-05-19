using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Lapis.Settings;

public static class SettingsPool
{
    private static readonly string SavePath = $"{AppContext.BaseDirectory}/data/settings.json";

    private static readonly List<StorageItem> StorageItems = File.Exists(SavePath)
        ? JsonConvert.DeserializeObject<List<StorageItem>>(File.ReadAllText(SavePath))
        : [];

    public static bool GetValue(string identifierPairString, long groupId, bool defaultValue)
    {
        foreach (var storageItem in StorageItems)
            if (storageItem.IdentifierPairString == identifierPairString && storageItem.GroupId == groupId)
                return storageItem.Value;

        return defaultValue;
    }

    public static void SetValue(string identifierPairString, long groupId, bool value)
    {
        var found = false;
        foreach (var storageItem in StorageItems)
            if (storageItem.IdentifierPairString == identifierPairString && storageItem.GroupId == groupId)
            {
                storageItem.Value = value;
                found = true;
            }

        if (!found)
            StorageItems.Add(new StorageItem(identifierPairString, groupId, value));

        File.WriteAllText(SavePath, JsonConvert.SerializeObject(StorageItems));
    }

    private class StorageItem(string identifierPairString, long groupId, bool value)
    {
        public readonly long GroupId = groupId;
        public readonly string IdentifierPairString = identifierPairString;
        public bool Value = value;
    }
}