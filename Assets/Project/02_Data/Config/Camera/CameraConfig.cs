using UnityEngine;

[CreateAssetMenu(fileName = "SO_CameraConfig", menuName = "Config/CameraConfig")]
public class CameraConfig : ScriptableObject
{
    [Header("Camera Config")]
    [SerializeField] private CameraRig m_cameraRigPrefab;
    
    [Header("Camera Shake Config")]
    [SerializeField] public float intensityShake = 1.0f;
    [SerializeField] public float durationShake = 1.0f;
    public CameraRig CameraRigPrefab => m_cameraRigPrefab;
}
