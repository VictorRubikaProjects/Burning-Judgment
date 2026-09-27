using UnityEngine;

[CreateAssetMenu(menuName = "Entity Pool System", fileName = "SO_EntityPoolSystem")]
public class SO_EntityPooling : ScriptableObject
{
    [field: SerializeField] public PlayerCharacter Player {get; private set;}
    [field: SerializeField] public Enemy EnemyTest {get; private set;}
    [field: SerializeField] public Vector3 PlayerSpawnPosition {get; private set;}
    [field: SerializeField] public Vector3[] EnemySpawningPositions {get; private set;}
    [field: SerializeField] public float PositionRadius {get; private set;}

    public Vector3 GetRandomPosition()
    {
        Vector3 basePosition = EnemySpawningPositions[Random.Range(0, EnemySpawningPositions.Length)];
        Vector2 randomOffset = Random.insideUnitCircle * PositionRadius;

        return basePosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }

    public Vector3 GetPosition(int index)
    {
        Vector3 basePosition = EnemySpawningPositions[index];
        Vector2 randomOffset = Random.insideUnitCircle * PositionRadius;

        return basePosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }
}