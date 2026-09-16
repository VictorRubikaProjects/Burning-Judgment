using Eflatun.SceneReference;
using UnityEngine;


[CreateAssetMenu(menuName = "GameConfigs/SO_GameConfig",  fileName = "SO_GameConfig")]
public class SO_GameConfig : ScriptableObject
{
    [field:Header("Scene References"),Space(10)]
    [SerializeField] public SceneReference menuScene;
    [SerializeField] public SceneReference gameplayScene;
    
    [field:Header("Other Config"),Space(10)]
    [SerializeField] public CameraConfig  cameraConfig;

    [field: Header("Prefabs"), Space(10)] 
    [SerializeField] public GameObject prefabTransition;
    [SerializeField] public GameObject prefabLoading;
}