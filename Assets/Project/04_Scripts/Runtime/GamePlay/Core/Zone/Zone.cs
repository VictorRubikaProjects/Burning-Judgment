using System.Collections.Generic;
using UnityEngine;

public abstract class Zone : MonoBehaviour
{
    [SerializeField] private ZoneType m_zoneType = ZoneType.None;

    private readonly HashSet<Actor> m_actorsInZone = new();

    public ZoneType Type => m_zoneType;
    protected IReadOnlyCollection<Actor> ActorsInZone => m_actorsInZone;

    protected virtual void OnDisable()
    {
        if (m_actorsInZone.Count == 0) return;

        List<Actor> actorsSnapshot = new(m_actorsInZone);
        foreach (Actor actor in actorsSnapshot)
        {
            HandleActorExit(actor);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!TryGetActor(other, out Actor actor)) return;
        HandleActorEnter(actor);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!TryGetActor(other, out Actor actor)) return;
        HandleActorExit(actor);
    }

    private void HandleActorEnter(Actor actor)
    {
        if (!m_actorsInZone.Add(actor)) return;

        actor.AddZoneFlag(m_zoneType);
        Enter(actor);
    }

    private void HandleActorExit(Actor actor)
    {
        if (!m_actorsInZone.Remove(actor)) return;

        actor.RemoveZoneFlag(m_zoneType);
        Exit(actor);
    }

    protected virtual void Enter(Actor actor) { }

    protected virtual void Exit(Actor actor) { }

    private static bool TryGetActor(Collider other, out Actor actor)
    {
        actor = other.GetComponentInParent<Actor>();
        return actor != null;
    }
}