using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game/Entity Pooling", fileName = "SO_EntityPooling")]
public class SO_EntityPooling : ScriptableObject
{
    [field: SerializeField] public PlayerCharacter Player {get; private set;}
    [field: SerializeField] public List<SO_ConfigRound> ConfigRounds {get; private set;}

    public SO_ConfigRound GetRandomRound()
    {
        return  ConfigRounds[Random.Range(0, ConfigRounds.Count)];
    }
}