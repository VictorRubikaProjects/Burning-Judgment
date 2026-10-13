using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Game/RoundConfig", fileName = "SO_ConfigRound")]
public class SO_ConfigRound : ScriptableObject
{
    [field: Header("Round")]
    [field: SerializeField, FormerlySerializedAs("<RoundData>k__BackingField")]
    [field: Tooltip("Map et équipes d'ennemis possibles pour ce round.")]
    public RoundData RoundData { get; private set; }

    [field: Space, Header("Spawn")]
    [field: SerializeField, FormerlySerializedAs("<PlayerSpawnPosition>k__BackingField")]
    [field: Tooltip("Position d'apparition du joueur dans la map.")]
    public Vector3 PlayerSpawnPosition { get; private set; }

    [field: SerializeField, FormerlySerializedAs("<EnemySpawningPositions>k__BackingField")]
    [field: Tooltip("Points d'apparition des ennemis dans la map.")]
    public Vector3[] EnemySpawnPositions { get; private set; }

    [field: SerializeField, FormerlySerializedAs("<PositionRadius>k__BackingField")]
    [field: Tooltip("Rayon autour de chaque point d'apparition où l'ennemi peut apparaître. 0 = exactement sur le point.")]
    public float SpawnRadius { get; private set; }

    public Vector3 GetRandomPosition()
    {
        return GetPosition(Random.Range(0, EnemySpawnPositions.Length));
    }

    public Vector3 GetPosition(int index)
    {
        Vector3 basePosition = EnemySpawnPositions[index];
        Vector2 randomOffset = Random.insideUnitCircle * SpawnRadius;

        return basePosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }
}