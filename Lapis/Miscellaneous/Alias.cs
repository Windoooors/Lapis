using System.Collections.Generic;

namespace Lapis.Miscellaneous;

public class Alias
{
    public HashSet<string> Aliases;
    public long Id;
}

public class AliasCollection
{
    public readonly List<Alias> Aliases = [];

    public void Add<T>(long id, string aliasString) where T : Alias, new()
    {
        if (TryGetAlias(id, out var alias))
        {
            alias.Aliases.Add(aliasString);
            return;
        }

        Aliases.Add(new T { Id = id, Aliases = [aliasString] });
    }

    public bool TryGetAlias(long id, out Alias aliasOut)
    {
        aliasOut = Aliases.Find(x => x.Id == id);
        return aliasOut != null && aliasOut.Id != 0;
    }

    public void Remove(int index)
    {
        Aliases.RemoveAt(index);
    }

    public long[] GetIds()
    {
        var ids = new List<long>();
        foreach (var alias in Aliases) ids.Add(alias.Id);

        return ids.ToArray();
    }
}