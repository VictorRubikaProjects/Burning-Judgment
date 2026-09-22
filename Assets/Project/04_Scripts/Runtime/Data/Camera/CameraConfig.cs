using UnityEngine;

[CreateAssetMenu(fileName = "SO_CameraConfig", menuName = "Config/CameraConfig")]
public class CameraConfig : ScriptableObject
{
    [Header("Camera Config")]
    [SerializeField] private CameraRig m_cameraRigPrefab;

    [Space][Header("Camera Shake Config")]
    [SerializeField] [Range(0f, 10f)] private float intensityShake = 1.0f;
    
    [SerializeField] [Min(0f)] private float durationShake = 1.0f;


    public CameraRig CameraRigPrefab => m_cameraRigPrefab;
    public float IntensityShake => intensityShake;
    public float DurationShake => durationShake;
}