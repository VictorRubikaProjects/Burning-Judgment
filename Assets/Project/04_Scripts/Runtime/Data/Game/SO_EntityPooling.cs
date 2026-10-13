using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game/Entity Pooling", fileName = "SO_EntityPooling")]
public class SO_EntityPooling : ScriptableObject
{
    [field: SerializeField]
    [field: Tooltip("Prefab du joueur instancié au début de chaque round.")]
    public PlayerCharacter Player { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Rounds possibles. Un de ces rounds est tiré au hasard à chaque partie.")]
    public List<SO_ConfigRound> ConfigRounds { get; private set; }

    public SO_ConfigRound GetRandomRound()
    {
        return ConfigRounds[Random.Range(0, ConfigRounds.Count)];
    }
}