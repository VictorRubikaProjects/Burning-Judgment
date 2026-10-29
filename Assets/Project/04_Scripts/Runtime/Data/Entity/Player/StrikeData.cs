using UnityEngine;

[System.Serializable]
public struct StrikeData
{
    [Range(0.1f, 3f)] public float Radius;
    [Range(0f, 3f)] public float ForwardOffset;
}