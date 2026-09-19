using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#if UNITY_EDITOR
using UnityEditor;
#endif

internal static class TimerBootstrapper
{

    private static PlayerLoopSystem m_timerSystem;
    
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    internal static void Initialize()
    {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!InsertTimerManager<Update>(ref currentPlayerLoop,0))
            {
                Debug.LogWarning("Unable to register TimerManager into Update loop");
                return;
            }
            
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            PlayerLoopUtils.PrintPlayerLoop(currentPlayerLoop);
            
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeState;
        EditorApplication.playModeStateChanged += OnPlayModeState;
#endif
    }

#if UNITY_EDITOR
    static void OnPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveTimerManager<Update>(ref currentPlayerLoop);
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            
            TimerManager.ClearTimers();
        }
    }
#endif

    static void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
    {
        PlayerLoopUtils.RemoveSystem<T>(ref loop, in m_timerSystem);
    }

    static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
    {
        m_timerSystem = new PlayerLoopSystem()
        {
            type = typeof(TimerManager),
            updateDelegate = TimerManager.UpdateTimers,
            subSystemList = null
        };

        return PlayerLoopUtils.InsertSystem<T>(ref loop, in m_timerSystem, index);
    }
}