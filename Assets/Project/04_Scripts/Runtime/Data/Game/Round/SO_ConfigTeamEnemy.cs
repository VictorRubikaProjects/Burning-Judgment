using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game/Team Enemy", fileName = "SO_ConfigTeamEnemy")]
public class SO_ConfigTeamEnemy : ScriptableObject
{
    [field:SerializeField] public List<EnemyBase> TeamEnemies {get; private set;}
}
