using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Audio/Audio Bank UI", fileName = "SO_AudioUI")]
public class SO_AudioBankUI : ScriptableObject
{
    [field: SerializeField] public EventReference SliderFx {get; private set;}
    [field: SerializeField] public EventReference NewGameFx {get; private set;}
    [field: SerializeField] public EventReference OptionToggleFx {get; private set;}
}
