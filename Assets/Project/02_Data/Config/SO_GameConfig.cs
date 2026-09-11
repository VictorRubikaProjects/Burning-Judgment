using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "GameConfigs/SO_GameConfig",  fileName = "SO_GameConfig")]
public class SO_GameConfig : ScriptableObject
{
    [SerializeField] public SceneReference menuScene;
}