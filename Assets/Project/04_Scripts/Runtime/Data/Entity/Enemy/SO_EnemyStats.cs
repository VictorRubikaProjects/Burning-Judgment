using UnityEngine;

[CreateAssetMenu(fileName = "SO_AIStats", menuName = "Config/AI Stats")]
public class SO_EnemyStats : SO_ActorsStats
{
    [Header("Locomotion")]
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float maxRange = 8f;
    [SerializeField] private float minRange = 1.5f;

    public float Speed => speed;
    public float MaxRange => maxRange;
    public float MinRange => minRange;
}