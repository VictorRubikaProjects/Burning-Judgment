using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Game/Camera Config", fileName = "SO_CameraConfig")]
public class CameraConfig : ScriptableObject
{
    [field: Header("Camera")]
    [field: SerializeField, Tooltip("Prefab de la caméra instancié au lancement du gameplay. Il contient la caméra Cinemachine et le groupe de cibles.")]
    public CameraRig CameraRigPrefab {get; private set;}

    [field: Space, Header("Camera Shake")]
    [field: SerializeField, Range(0f, 10f), FormerlySerializedAs("intensityShake")]
    [Tooltip("Intensité du tremblement de caméra. 0 = aucun tremblement.")]
    public float ShakeIntensity {get; private set;} = 1f;

    [field: SerializeField, Min(0f), FormerlySerializedAs("durationShake")]
    [Tooltip("Durée du tremblement de caméra, en secondes.")]
    public float ShakeDuration {get; private set;} = 1f;
}