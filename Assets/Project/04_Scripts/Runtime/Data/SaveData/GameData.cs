using System;

[Serializable]
public class GameData
{
    public static GameData CreateDefault()
    {
        return new GameData();
    }
}