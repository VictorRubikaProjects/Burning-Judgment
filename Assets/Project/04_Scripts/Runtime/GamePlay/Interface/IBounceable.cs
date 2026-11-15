using UnityEngine;

public interface IBounceable
{
    int BounceCount { get; }
    void OnBounced(Vector3 direction, float distance);
    bool CanBounce();
}