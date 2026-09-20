using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SaveService : IGameService
{
    private const string SettingsFile = "settings.json";
    private const string GameFile = "game.json";

    public SettingsData Settings { get; private set; }
    public GameData Game { get; private set; }

    private string _settingsPath;
    private string _gamePath;

    private readonly object _settingsLock = new();
    private readonly object _gameLock = new();

    #region IGameService Core

    public void Dispose() { }

    public async UniTask InitializeService()
    {
        _settingsPath = Path.Combine(Application.persistentDataPath, SettingsFile);
        _gamePath = Path.Combine(Application.persistentDataPath, GameFile);

        await LoadOrCreateSettings();
        await LoadOrCreateGame();
    }

    public void ShutDownService() { }

    public void Tick() { }

    public bool IsInitialized { get; set; }

    #endregion

    #region Settings

    public async UniTask LoadOrCreateSettings()
    {
        if (!File.Exists(_settingsPath))
        {
            Settings = SettingsData.CreateDefault();
            await SaveSettings();
            return;
        }
        await LoadSettings();
    }

    public async UniTask SaveSettings()
    {
        string json = JsonUtility.ToJson(Settings, true);
        string path = _settingsPath;

        await UniTask.RunOnThreadPool(() =>
        {
            lock (_settingsLock)
            {
                WriteAtomic(path, json);
            }
        });
    }

    public async UniTask LoadSettings()
    {
        try
        {
            string json = await UniTask.RunOnThreadPool(() =>
            {
                lock (_settingsLock)
                {
                    return File.ReadAllText(_settingsPath);
                }
            });

            Settings = JsonUtility.FromJson<SettingsData>(json);
            if (Settings == null)
                throw new InvalidDataException("Parsed settings is null");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveService] Failed to load settings, falling back to defaults: {e.Message}");
            Settings = SettingsData.CreateDefault();
            await SaveSettings();
        }
    }

    #endregion

    #region Game

    public async UniTask LoadOrCreateGame()
    {
        if (!File.Exists(_gamePath))
        {
            Game = GameData.CreateDefault();
            await SaveGame();
            return;
        }
        await LoadGame();
    }

    public async UniTask SaveGame()
    {
        string json = JsonUtility.ToJson(Game, true);
        string path = _gamePath;

        await UniTask.RunOnThreadPool(() =>
        {
            lock (_gameLock)
            {
                WriteAtomic(path, json);
            }
        });
    }

    public async UniTask LoadGame()
    {
        try
        {
            string json = await UniTask.RunOnThreadPool(() =>
            {
                lock (_gameLock)
                {
                    return File.ReadAllText(_gamePath);
                }
            });

            Game = JsonUtility.FromJson<GameData>(json);
            if (Game == null)
                throw new InvalidDataException("Parsed game data is null");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveService] Failed to load game data, falling back to defaults: {e.Message}");
            Game = GameData.CreateDefault();
            await SaveGame();
        }
    }

    #endregion

    private static void WriteAtomic(string path, string json)
    {
        string tempPath = path + ".tmp";
        File.WriteAllText(tempPath, json);

#if UNITY_STANDALONE || UNITY_EDITOR
        if (File.Exists(path))
            File.Replace(tempPath, path, null);
        else
            File.Move(tempPath, path);
#else
        if (File.Exists(path))
            File.Delete(path);

        File.Move(tempPath, path);
#endif
    }
}