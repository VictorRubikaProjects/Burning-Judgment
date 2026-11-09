using Unity.Cinemachine;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    [field: SerializeField, Tooltip("Caméra Cinemachine pilotée par le CameraService.")]
    public CinemachineCamera CinemachineCamera {get; private set;}

    [field: SerializeField, Tooltip("Groupe de cibles cadrées par la caméra.")]
    public CinemachineTargetGroup TargetGroup {get; private set;}

}