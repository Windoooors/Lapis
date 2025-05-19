using System.Collections.Generic;

namespace Lapis.Settings;

public class SettingsCategory
{
    public string DisplayName { get; init; }
    public List<SettingsItemsOfACommand> Items { get; init; }
}