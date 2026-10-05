using UnityEngine;

[CreateAssetMenu(fileName = "SO_ConfigEnemyDash", menuName = "Config/Enemy/Dasher")]
public class SO_ConfigEnemyDash : SO_ConfigEnemy
{
    [field : SerializeField] public float DashSpeed {get; private set;}
}