using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// This will always be called first.
/// For security do not change the execution order from another script except this one.
/// </summary>
[DefaultExecutionOrder(-1)]
public class BootStrap : MonoBehaviour
{
    [SerializeField] private SO_GameConfig gameConfig;
    
    private readonly GameInitializer _gameInitializer = new();
    
    public SO_GameConfig GameConfig => gameConfig;

    private void Awake()
    {
        if (!gameConfig)
        {
            Debug.LogError($"[BootStrap] The game config file is missing cancel bootstrap .");
            return;
        }
        
        _gameInitializer.Initialize(gameConfig);

        UniTaskScheduler.UnobservedTaskException += OnUnobservedException;
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