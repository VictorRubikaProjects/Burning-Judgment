using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class RoundData
{
    [field: SerializeField, FormerlySerializedAs("Map")]
    [field: Tooltip("Scène de la map jouée pendant ce round.")]
    public SceneReference Map { get; private set; }

    [field: SerializeField, FormerlySerializedAs("EnemiesPalettes")]
    [field: Tooltip("Équipes d'ennemis possibles pour ce round. Une équipe est tirée au hasard à chaque fois.")]
    public List<SO_ConfigTeamEnemy> EnemyTeams { get; private set; }

    public SO_ConfigTeamEnemy GetRandomEnemyTeam()
    {
        return EnemyTeams[Random.Range(0, EnemyTeams.Count)];
    }
}