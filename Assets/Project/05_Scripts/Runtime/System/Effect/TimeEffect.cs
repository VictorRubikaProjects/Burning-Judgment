using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TimeEffect
{
    private CancellationTokenSource m_ctsHitStop = new();
    private bool hitStopRunning = false;

    public async UniTaskVoid HitStop()
    {
        if (hitStopRunning) return;
        
        hitStopRunning = true;
        
        m_ctsHitStop?.Cancel();
        m_ctsHitStop = new CancellationTokenSource();
        
        float originalTimeScale = Time.timeScale;
        
        try
        {
            Time.timeScale = 0f;
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), ignoreTimeScale: true, cancellationToken: m_ctsHitStop.Token);
            Time.timeScale = originalTimeScale;
            hitStopRunning = false;
        }
        catch (OperationCanceledException)
        {
            Time.timeScale = originalTimeScale;
            hitStopRunning = false;
        }
    }
}
