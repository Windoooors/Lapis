using System.Collections.Generic;

namespace LapisBot.Settings;

public class SettingsItemsOfACommand
{
    public string Identifier { get; init; }
    public string DisplayName { get; init; }
    public List<SettingsItem> Items { get; init; }
}