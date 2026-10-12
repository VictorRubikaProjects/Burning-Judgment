using UnityEngine;

public class ActorTestBounce : Actor , IBouncy
{
    public Transform TargetTransform => transform;
    public bool CanBeBounced => true;
}
