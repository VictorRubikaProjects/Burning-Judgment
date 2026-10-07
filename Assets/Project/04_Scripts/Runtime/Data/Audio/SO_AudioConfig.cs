using UnityEngine;
using FMOD;

[CreateAssetMenu(menuName = "Config/Audio/Audio Config", fileName = "SO_AudioConfig")]
public class SO_AudioConfig : ScriptableObject
{
    public AnimationCurve VolumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    public float MusicFadeOutTime = 1f;
    
    public string MasterBusPath = "bus:/";
    public string MusicBusPath = "bus:/Music";
    public string SfxBusPath = "bus:/Sfx";
    public string AmbienceBusPath = "bus:/Ambience";
}