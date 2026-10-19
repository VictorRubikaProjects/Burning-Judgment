using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game/Vfx", fileName = "SO_ConfigVfx")]
public class SO_ConfigVfx : ScriptableObject
{
    [field: SerializeField] public ParticleSystem VfxPrefabPushed { get; private set; }
}
