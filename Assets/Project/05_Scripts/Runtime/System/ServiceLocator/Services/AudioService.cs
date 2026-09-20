using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioService : IGameService
{
    private Bus m_masterBus;
    private Bus m_sfxBus;
    private Bus m_musicBus;
    private Bus m_ambienceBus;

    private SO_AudioConfig m_config;

    private List<EventInstance> m_eventInstances;
    private EventInstance m_musicEventInstance;

    public AudioService(SO_AudioConfig config)
    {
        m_config = config;
    }

    #region IGameService Core

    public bool IsInitialized { get; set; }

    public void Dispose()
    {
        ReleaseAllInstances();
    }

    public UniTask InitializeService()
    {
        m_masterBus = GetBusSafe(m_config.MasterBusPath);
        m_sfxBus = GetBusSafe(m_config.SfxBusPath);
        m_musicBus = GetBusSafe(m_config.MusicBusPath);
        m_ambienceBus = GetBusSafe(m_config.AmbienceBusPath);

        m_eventInstances = new List<EventInstance>();

        return UniTask.CompletedTask;
    }

    public void ShutDownService()
    {
        ReleaseAllInstances();
    }

    public void Tick() { }

    #endregion
    
    private Bus GetBusSafe(string path)
    {
        try
        {
            return RuntimeManager.GetBus(path);
        }
        catch (EventNotFoundException)
        {
            Debug.LogError($"[AudioService] Bus not found: {path}");
            return default;
        }
    }

    private void ReleaseAllInstances()
    {
        foreach (var instance in m_eventInstances)
        {
            instance.stop(STOP_MODE.IMMEDIATE);
            instance.release();
        }
        m_eventInstances.Clear();

        if (m_musicEventInstance.isValid())
        {
            m_musicEventInstance.stop(STOP_MODE.ALLOWFADEOUT);
            m_musicEventInstance.release();
        }
    }

    public void PlaySfx(EventReference sound, Vector3 position)
    {
        RuntimeManager.PlayOneShot(sound, position);
    }

    public EventInstance CreateInstance(EventReference eventReference, bool addToList = true)
    {
        var eventInstance = RuntimeManager.CreateInstance(eventReference);

        if (addToList)
        {
            m_eventInstances.Add(eventInstance);
        }

        return eventInstance;
    }

    public void SetVolume(BusEnum bus, float vol)
    {
        float evaluated = m_config.VolumeCurve.Evaluate(vol);

        switch (bus)
        {
            case BusEnum.MASTER:
                m_masterBus.setVolume(evaluated);
                break;
            case BusEnum.MUSIC:
                m_musicBus.setVolume(evaluated);
                break;
            case BusEnum.SFX:
                m_sfxBus.setVolume(evaluated);
                break;
            case BusEnum.AMBIENCE:
                m_ambienceBus.setVolume(evaluated);
                break;
        }
    }

    public async UniTask StartMusic(EventReference eventReference)
    {
        while (!RuntimeManager.IsInitialized)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        }

        if (m_musicEventInstance.isValid())
        {
            await StopMusic();
        }

        m_musicEventInstance = CreateInstance(eventReference, false);
        m_musicEventInstance.start();
    }

    public async UniTask StopMusic()
    {
        if (!m_musicEventInstance.isValid()) return;

        m_musicEventInstance.stop(STOP_MODE.ALLOWFADEOUT);
        m_musicEventInstance.release();
        await UniTask.Yield();
    }

    public enum BusEnum
    {
        MASTER,
        MUSIC,
        SFX,
        AMBIENCE
    }
}