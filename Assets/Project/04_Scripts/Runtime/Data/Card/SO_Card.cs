using UnityEngine;

[CreateAssetMenu(menuName = "Config/Card", fileName = "SO_Card")]
public class SO_Card : ScriptableObject
{ 
    [Header("Visuals")]
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    
}