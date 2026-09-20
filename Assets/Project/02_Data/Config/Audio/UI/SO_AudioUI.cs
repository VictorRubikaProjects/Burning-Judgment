using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_AudioUI", menuName = "Config/UI Audio")]
public class SO_AudioUI : ScriptableObject
{
    [field: SerializeField] public EventReference SliderFx {get; private set;}
    [field: SerializeField] public EventReference NewGameFx {get; private set;}
    [field: SerializeField] public EventReference OptionToggleFx {get; private set;}
}
