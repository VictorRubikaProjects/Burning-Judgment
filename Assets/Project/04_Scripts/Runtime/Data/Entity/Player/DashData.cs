using UnityEngine;

[System.Serializable]
public struct DashData
{
    [Range(0.01f, 1f)] public float Duration;
    public AnimationCurve Curve;
}