using System;

[Serializable]
public class SettingsData
{
    public static SettingsData CreateDefault()
    {
        return new SettingsData();
    }
}