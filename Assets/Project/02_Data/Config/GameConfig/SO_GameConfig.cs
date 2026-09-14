using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "GameConfigs/SO_GameConfig",  fileName = "SO_GameConfig")]
public class SO_GameConfig : ScriptableObject
{
    [field:Header("Scene References"),Space(10)]
    [SerializeField] public SceneReference menuScene;
    [SerializeField] public SceneReference gameplayScene;
    [SerializeField] public SceneReference uiScene;
}