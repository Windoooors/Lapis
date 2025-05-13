using System.Collections.Generic;

namespace LapisBot_Renewed.Settings;

public class SettingsCategory
{
    public string DisplayName { get; set; }
    public List<SettingsItemsOfACommand> Items { get; set; }
}