using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Game/Game Config", fileName = "SO_GameConfig")]
public class SO_GameConfig : ScriptableObject
{
    [field: Header("Scene References")]
    [field: SerializeField, FormerlySerializedAs("menuScene")]
    [field: Tooltip("Scène du menu principal.")]
    public SceneReference MenuScene { get; private set; }

    [field: SerializeField, FormerlySerializedAs("gameplayScene")]
    [field: Tooltip("Scène de gameplay chargée au lancement d'une partie.")]
    public SceneReference GameplayScene { get; private set; }

    [field: Space(10), Header("Other Config")]
    [field: SerializeField, FormerlySerializedAs("cameraConfig")]
    [field: Tooltip("Configuration de la caméra utilisée pendant le jeu.")]
    public CameraConfig CameraConfig { get; private set; }
}