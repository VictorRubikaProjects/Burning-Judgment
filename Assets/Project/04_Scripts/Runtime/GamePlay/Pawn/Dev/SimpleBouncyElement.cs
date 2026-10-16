using UnityEngine;

public class SimpleBouncyElement : Actor , IBouncy
{
    public Transform TargetTransform => transform;
    public bool CanBeBounced => true;
}
