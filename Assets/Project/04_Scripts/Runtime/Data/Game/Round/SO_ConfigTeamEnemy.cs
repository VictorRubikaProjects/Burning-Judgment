using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game/Team Enemy", fileName = "SO_ConfigTeamEnemy")]
public class SO_ConfigTeamEnemy : ScriptableObject
{
    [field: SerializeField]
    [field: Tooltip("Prefabs des ennemis qui composent cette équipe. Un même prefab peut être ajouté plusieurs fois.")]
    public List<EnemyBase> TeamEnemies { get; private set; }
}