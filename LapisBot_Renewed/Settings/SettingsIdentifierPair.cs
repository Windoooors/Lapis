namespace LapisBot_Renewed.Settings;

public class SettingsIdentifierPair
{
    public string PrimeIdentifier;
    public string Identifier;

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
    
    public new string ToString() => $"{PrimeIdentifier}.{Identifier}";
}