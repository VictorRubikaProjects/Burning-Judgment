using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;

public class CameraShake 
{
   private CinemachineCamera  m_cinemachineCamera;
   private CameraConfig  m_config;
   private CinemachineBasicMultiChannelPerlin m_perlin;
   private CancellationTokenSource m_ctsShake;
   
   public CameraShake(CinemachineCamera  camera, CameraConfig config)
   {
      m_cinemachineCamera = camera;
      m_config = config;
      m_perlin = m_cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
      m_ctsShake = new CancellationTokenSource();
   }

   public async UniTask Shake()
   {
      m_ctsShake?.Cancel();
      m_ctsShake = new CancellationTokenSource();

      try
      {
         m_perlin.AmplitudeGain = m_config.IntensityShake;
         await UniTask.Delay(TimeSpan.FromSeconds(m_config.DurationShake),cancellationToken: m_ctsShake.Token);
         m_perlin.AmplitudeGain = 0;
      }
      catch (OperationCanceledException ) { }
   }
}
