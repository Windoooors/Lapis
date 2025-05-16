namespace LapisBot.Settings;

public class SettingsIdentifierPair
{
    public string Identifier;
    public string PrimeIdentifier;

    public SettingsIdentifierPair(string primeIdentifier, string identifier)
    {
        PrimeIdentifier = primeIdentifier;
        Identifier = identifier;
    }

    public SettingsIdentifierPair()
    {
        PrimeIdentifier = "";
        Identifier = "";
    }

    public new string ToString()
    {
        return $"{PrimeIdentifier}.{Identifier}";
    }
}