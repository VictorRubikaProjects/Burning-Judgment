using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Audio/UI Audio", fileName = "SO_AudioUI")]
public class SO_AudioUI : ScriptableObject
{
    [field: SerializeField] public EventReference SliderFx {get; private set;}
    [field: SerializeField] public EventReference NewGameFx {get; private set;}
    [field: SerializeField] public EventReference OptionToggleFx {get; private set;}
}
