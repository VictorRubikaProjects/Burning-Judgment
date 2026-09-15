using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class CameraService : IGameService
{
    private readonly CameraConfig m_config;
    private CinemachineBrain m_cinemachineBrain;
    private Camera m_camera;
    private CameraRig m_cameraRig;

    public Camera MainCamera => m_camera;
    public CinemachineCamera CinemachineCamera => m_cameraRig.CinemachineCamera;

    public CameraService(CameraConfig config)
    {
        m_config = config;
    }

    public UniTask InitializeService()
    {
        m_cinemachineBrain = Object.FindFirstObjectByType<CinemachineBrain>();

        if (m_cinemachineBrain == null)
        {
            Debug.LogError("CameraService: no CinemachineBrain found in scene.");
            return UniTask.CompletedTask;
        }

        m_camera = m_cinemachineBrain.GetComponent<Camera>();
        m_cameraRig = Object.Instantiate(m_config.CameraRigPrefab);

        IsInitialized = true;
        return UniTask.CompletedTask;
    }

    public void AddTarget(Transform target, float weight)
    {
        m_cameraRig.TargetGroup.AddMember(target, weight, 0f);
    }

    public void RemoveTarget(Transform target)
    {
        m_cameraRig.TargetGroup.RemoveMember(target);
    }

    public void Dispose(){ }
    
    public void ShutDownService()
    {
        if (m_cameraRig != null)
        {
            Object.Destroy(m_cameraRig.gameObject);
        }
    }

    public void Tick() { }

    public bool IsInitialized { get; set; }
}