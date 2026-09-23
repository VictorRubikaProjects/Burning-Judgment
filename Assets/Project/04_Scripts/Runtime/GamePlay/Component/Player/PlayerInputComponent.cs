using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : ActorComponent
{
    #region Variables

    private PlayerActions m_inputActions;

    private Vector2 m_startPos;
    private float m_touchStartTime;
    private bool m_isTouching;
    private bool m_isInAttackWindow;

    private readonly SO_PlayerStats m_stats;

    public event Action<Vector2> OnSwipe;
    public event Action<Vector2> OnPressSwipeSuccess;
    public event Action OnAttackWindowEnter;
    public event Action OnAttackWindowExit;

    #endregion

    #region ActorComponent Methods

    public PlayerInputComponent(Actor owner, SO_PlayerStats stats) : base(owner)
    {
        m_stats = stats;
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

    public override void Update()
    {
        if (!m_isTouching) return;

        float heldDuration = Time.time - m_touchStartTime;
        bool inWindow = IsInAttackWindow(heldDuration);

        if (inWindow && !m_isInAttackWindow)
        {
            m_isInAttackWindow = true;
            OnAttackWindowEnter?.Invoke();
        }
        else if (!inWindow && m_isInAttackWindow)
        {
            m_isInAttackWindow = false;
            OnAttackWindowExit?.Invoke();
        }
    }

    #endregion

    #region Core Methods

    private void TouchStarted(InputAction.CallbackContext context)
    {
        m_startPos = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        m_touchStartTime = Time.time;
        m_isTouching = true;
        m_isInAttackWindow = false;
    }

    private void TouchEnded(InputAction.CallbackContext context)
    {
        m_isTouching = false;

        if (m_isInAttackWindow)
        {
            m_isInAttackWindow = false;
            OnAttackWindowExit?.Invoke();
        }

        Vector2 endPos = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        Vector2 delta = endPos - m_startPos;

        if (delta.magnitude < m_stats.SwipeThreshold) return;

        float heldDuration = Time.time - m_touchStartTime;

        if (IsInAttackWindow(heldDuration))
        {
            OnPressSwipeSuccess?.Invoke(delta.normalized);
        }
        else if (heldDuration <= m_stats.MaxTimeSwipe)
        {
            OnSwipe?.Invoke(delta.normalized);
        }
    }

    private bool IsInAttackWindow(float heldDuration) =>
        heldDuration >= m_stats.TimerAttack - m_stats.ThresholdTimerAttack &&
        heldDuration <= m_stats.TimerAttack + m_stats.ThresholdTimerAttack;

    #endregion
}