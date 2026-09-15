using UnityEngine;

[CreateAssetMenu(fileName = "SO_CameraConfig", menuName = "Config/CameraConfig")]
public class CameraConfig : ScriptableObject
{
    [SerializeField] private CameraRig m_cameraRigPrefab;

    public CameraRig CameraRigPrefab => m_cameraRigPrefab;
}
