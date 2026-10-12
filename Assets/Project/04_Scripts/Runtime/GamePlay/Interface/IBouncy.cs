using UnityEngine;

public interface IBouncy
{
    public Transform  TargetTransform { get; }
    
    public bool CanBeBounced { get; }
}