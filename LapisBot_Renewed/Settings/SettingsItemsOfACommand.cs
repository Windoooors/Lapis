using System.Collections.Generic;

namespace LapisBot_Renewed.Settings;

public class SettingsItemsOfACommand
{
    public string Identifier { get; set; }
    public string DisplayName { get; set; }
    public List<SettingsItem> Items { get; set; }
}