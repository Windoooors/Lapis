using System.Collections.Generic;

namespace Lapis.Settings;

public class SettingsItemsOfACommand
{
    public string Identifier { get; init; }
    public string DisplayName { get; init; }
    public List<SettingsItem> Items { get; init; }
}