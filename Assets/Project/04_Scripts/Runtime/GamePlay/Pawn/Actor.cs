using System.Collections.Generic;
using Helpers.Runtime.Math;
using UnityEngine;

public class Actor : Pawn
{
    
    [SerializeField] public Transform FeetTransform;
    [SerializeField] private StabilityVisuals stabilityVisuals;

    private readonly List<ActorComponent> m_components = new();

    private float m_stability;
    private int m_zoneMask;

    protected bool m_isDead;

    public int ZoneMask => m_zoneMask;

    public bool IsInZone(ZoneType zoneType) => m_zoneMask.ContainsFlag((int)zoneType);

    public void AddZoneFlag(ZoneType zoneType) => m_zoneMask = m_zoneMask.AddFlag((int)zoneType);

    public void RemoveZoneFlag(ZoneType zoneType) => m_zoneMask = m_zoneMask.RemoveFlag((int)zoneType);

    public float GetStability() => m_stability;

    protected virtual void SetStability(float stability)
    {
        m_stability = Mathf.Clamp(stability, 0.1f, 1.0f);
        stabilityVisuals.SetStabilityVisuals(m_stability);
    }

    public virtual void Kill() { }

    protected override void Update()
    {
        base.Update();

        foreach (ActorComponent component in m_components)
        {
            component.Update();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        foreach (ActorComponent component in m_components)
        {
            component.FixedUpdate();
        }
    }

    protected virtual void OnDestroy()
    {
        foreach (ActorComponent component in m_components)
        {
            component.Dispose();
        }

        m_components.Clear();
    }

    protected T AddActorComponent<T>(T component) where T : ActorComponent
    {
        m_components.Add(component);
        component.Initialize();
        return component;
    }

    public T GetActorComponent<T>() where T : ActorComponent
    {
        foreach (ActorComponent component in m_components)
        {
            if (component is T match) return match;
        }

        return null;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        foreach (ActorComponent component in m_components)
        {
            component.OnDrawGizmosSelected();
        }
    }
}