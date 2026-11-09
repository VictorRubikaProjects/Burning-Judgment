using UnityEngine;
using FMODUnity; 

[CreateAssetMenu(menuName = "Config/Audio/Audio Bank Gameplay", fileName = "SO_AudioConfig")]
public class SO_AudioBank : ScriptableObject
{
    [Header("Player")]
    [field: SerializeField] public EventReference PlayerDashEvent { get; private set; }
    [field: SerializeField] public EventReference PlayerHitEvent { get; private set; }
    [field: SerializeField] public EventReference PlayerAttackEvent { get; private set; }

    [Header("Enemy")]
    [field: SerializeField] public EventReference EnemyHitEvent { get; private set; }

    [Header("Enemy Dasher")]
    [field: SerializeField] public EventReference DasherDashEvent { get; private set; }
    [field: SerializeField] public EventReference DasherChargeDashEvent { get; private set; }

    [Header("Enemy Expansion")]
    [field: SerializeField] public EventReference ExpansionChargeAOEEvent { get; private set; }
    [field: SerializeField] public EventReference ExpansionExecuteAOEEvent { get; private set; }

    [Header("Enemy Sniper")]
    [field: SerializeField] public EventReference SniperShootEvent { get; private set; }
    [field: SerializeField] public EventReference SniperAimEvent { get; private set; }

    [Header("UI")]
    [field: SerializeField] public EventReference UIButtonEvent { get; private set; }
    [field: SerializeField] public EventReference UISliderEvent { get; private set; }
    
    [Header("Game")]
    [field: SerializeField] public EventReference RoundWonEvent { get; private set; }
    [field: SerializeField] public EventReference StartGameEvent { get; private set; }
    [field: SerializeField] public EventReference RoundStartEvent { get; private set; }
    
    [Header("Music")]
    [field: SerializeField] public EventReference MainMusic { get; private set; }
}