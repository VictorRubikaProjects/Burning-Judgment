using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputComponent : ActorComponent
{
    #region Variables

    private readonly float m_swipeThreshold;

    private PlayerActions m_inputActions;

    private Vector2 m_startPos;

    public event Action<Vector2> OnSwipe;

    private float maxTimeSwipe = 0.1f;

    #endregion

    #region ActorComponent Methods

    public InputComponent(Actor owner, float swipeThreshold) : base(owner)
    {
        m_swipeThreshold = swipeThreshold;
    }

    public override void Initialize()
    {
        m_inputActions = new PlayerActions();
        m_inputActions.Gameplay.Enable();

        m_inputActions.Gameplay.Contact.started += TouchStarted;
        m_inputActions.Gameplay.Contact.canceled += TouchEnded;
    }

    public override void Dispose()
    {
        m_inputActions.Gameplay.Contact.started -= TouchStarted;
        m_inputActions.Gameplay.Contact.canceled -= TouchEnded;

        m_inputActions.Gameplay.Disable();
        m_inputActions.Dispose();
    }

    #endregion

    #region Core Methods

    private void TouchStarted(InputAction.CallbackContext context)
    {
        m_startPos = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
    }

    private void TouchEnded(InputAction.CallbackContext context)
    {
        Vector2 endPos = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        Vector2 delta = endPos - m_startPos;

        if (delta.magnitude < m_swipeThreshold) return;

        OnSwipe?.Invoke(delta.normalized);
    }

    #endregion
}