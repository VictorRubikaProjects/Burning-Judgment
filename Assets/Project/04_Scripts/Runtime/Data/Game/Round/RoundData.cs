using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
using Random = UnityEngine.Random;

[Serializable]
public class RoundData
{
    public SceneReference Map;
    public List<SO_ConfigTeamEnemy> EnemiesPalettes;
    
    public SO_ConfigTeamEnemy GetRandomEnemiesPalette()
    {
        return EnemiesPalettes[Random.Range(0, EnemiesPalettes.Count)];
    }
}
