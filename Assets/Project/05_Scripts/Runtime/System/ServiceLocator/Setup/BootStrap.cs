using System;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;

/// <summary>
/// This will always be called first.
/// For security do not change the execution order from another script except this one.
/// </summary>
[DefaultExecutionOrder(-1)]
public class BootStrap : MonoBehaviour
{
    [SerializeField] private SO_GameConfig gameConfig;
    [SerializeField] private SO_AudioConfig audioConfig;
    
    private readonly GameInitializer _gameInitializer = new();
    
    public SO_GameConfig GameConfig => gameConfig;
    public SO_AudioConfig AudioConfig => audioConfig;
    
    public int MaxFrames = 60;

    private void Awake()
    {
        if (!gameConfig)
        {
            Debug.LogError($"[BootStrap] The game config file is missing cancel bootstrap .");
            return;
        }
        
        _gameInitializer.Initialize(gameConfig,audioConfig);

        UniTaskScheduler.UnobservedTaskException += OnUnobservedException;
        
        Application.targetFrameRate = MaxFrames;
    }

    private void OnUnobservedException(Exception exception)
    {
        Debug.LogException(exception);
    }

    private void OnDestroy()
    {
        UniTaskScheduler.UnobservedTaskException -= OnUnobservedException;
    }
}