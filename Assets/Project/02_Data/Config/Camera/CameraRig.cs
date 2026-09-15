using Unity.Cinemachine;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    [SerializeField] private CinemachineCamera m_cinemachineCamera;
    [SerializeField] private CinemachineTargetGroup m_targetGroup;

    public CinemachineCamera CinemachineCamera => m_cinemachineCamera;
    public CinemachineTargetGroup TargetGroup => m_targetGroup;
}