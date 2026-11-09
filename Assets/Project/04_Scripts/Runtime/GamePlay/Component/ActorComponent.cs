using System;
using UnityEngine;

public class ActorComponent : IDisposable
{
    protected Actor Owner { get; }

    protected ActorComponent(Actor owner)
    {
        if (!owner)
        {
            Debug.LogError("ActorComponent: Owner cannot be null.");
            return;
        }
        
        Owner = owner;
    }

    public virtual void Initialize() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void OnDrawGizmos() { }
    public virtual void OnDrawGizmosSelected() { }
    public virtual void Reset() { }
    public virtual void Dispose() { }
}
