using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TimeControllerComponent : ActorComponent
{
    public float CurrentTimeDilation { get; private set; } = 1.0f;
    
    private CancellationTokenSource _timeLerpCts;

    public TimeControllerComponent(Actor owner) : base(owner)
    {
    }

    public override void Dispose()
    {
        base.Dispose();
        _timeLerpCts?.Cancel();
        _timeLerpCts?.Dispose();
        _timeLerpCts = null;
        HardResetTime();
    }

    public async UniTaskVoid LerpTimeDilationAsync(float targetDilation, float duration)
    {
        _timeLerpCts?.Cancel();
        _timeLerpCts = new CancellationTokenSource();
        var cancellationToken = _timeLerpCts.Token;

        float startDilation = CurrentTimeDilation;
        float elapsedTime = 0f;

        try
        {
            if (duration <= 0f)
            {
                SetTimeDilation(targetDilation);
                return;
            }

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                CurrentTimeDilation = Mathf.Lerp(startDilation, targetDilation, t);
                ApplyTimeDilationToEngine(CurrentTimeDilation);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            SetTimeDilation(targetDilation);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void SetTimeDilation(float targetDilation)
    {
        _timeLerpCts?.Cancel();

        CurrentTimeDilation = Mathf.Max(0.0f, targetDilation);
        ApplyTimeDilationToEngine(CurrentTimeDilation);
    }

    private void ApplyTimeDilationToEngine(float dilation) => Time.timeScale = dilation;

    public void HardResetTime() => SetTimeDilation(1.0f);
}