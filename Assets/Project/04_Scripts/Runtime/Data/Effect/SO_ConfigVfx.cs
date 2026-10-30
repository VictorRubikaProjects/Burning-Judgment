using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game/Vfx", fileName = "SO_ConfigVfx")]
public class SO_ConfigVfx : ScriptableObject
{
    [field: SerializeField] public ParticleSystem VfxPrefabEjection { get; private set; }
    [field: SerializeField] public ParticleSystem VfxPrefabChainInputSuccess { get; private set; }
    [field: SerializeField] public ParticleSystem VfxPrefabDash { get; private set; }
}
