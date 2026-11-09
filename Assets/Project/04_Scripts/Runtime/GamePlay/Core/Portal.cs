using System;
using Event_Bus;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Collider m_triggerCollider;
    [SerializeField] private GameObject m_model;
    
    private EventBinding<FloorClearedEvent> m_eventBindingFloorCleared;
    private EventBinding<FloorStartedEvent> m_eventBindingFloorStarted;

    private bool m_portalTaken = false;
    
    private void Awake()
    {
        m_eventBindingFloorCleared = new EventBinding<FloorClearedEvent>(SpawnPortal);
        m_eventBindingFloorStarted = new EventBinding<FloorStartedEvent>(ClearPortal);
        
        EventBus<FloorClearedEvent>.Register(m_eventBindingFloorCleared);
        EventBus<FloorStartedEvent>.Register(m_eventBindingFloorStarted);
        
        ClearPortal();
    }

    private void OnDestroy()
    {
        EventBus<FloorClearedEvent>.Unregister(m_eventBindingFloorCleared);
        EventBus<FloorStartedEvent>.Unregister(m_eventBindingFloorStarted);
    }

    private void SpawnPortal()
    {
        m_triggerCollider.enabled = true;
        m_model.SetActive(true);
    }

    private void ClearPortal()
    {
        m_portalTaken = false;
        m_triggerCollider.enabled = false;
        m_model.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || m_portalTaken) return;
        m_portalTaken = true;
        ServiceLocator.Get<GameService>().OnPortalTaken();
    }
}
