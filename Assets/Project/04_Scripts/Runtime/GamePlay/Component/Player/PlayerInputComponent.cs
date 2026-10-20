using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : ActorComponent
{
    private PlayerActions m_inputActions;

    private Vector2 m_lastDirection;
    private float m_chargeStartTime;
    private bool m_isCharging;

    private readonly SO_PlayerStats m_stats;

    public event Action<Vector2, float> OnJoystickRelease;
    public event Action<Vector2, float> OnChargeUpdate;
    public event Action OnChargeEnd;

    private RectTransform m_joystickRectT;
    private Vector2 m_joystickRect;
    
    public PlayerInputComponent(Actor owner, SO_PlayerStats stats, RectTransform joystickRectT) : base(owner)
    {
        m_stats = stats;
        m_joystickRectT = joystickRectT;
        m_joystickRect =  joystickRectT.position;
    }

    public override void Initialize()
    {
        m_inputActions = new PlayerActions();
        m_inputActions.Gameplay.Enable();

        m_inputActions.Gameplay.Touch.started += OnTouchStarted;
        m_inputActions.Gameplay.Touch.canceled += OnTouchCanceled;
        
        m_inputActions.Gameplay.Move.started += OnMoveStarted;
        m_inputActions.Gameplay.Move.performed += OnMovePerformed;
        m_inputActions.Gameplay.Move.canceled += OnMoveCanceled;
    }

    public override void Dispose()
    {
        m_inputActions.Gameplay.Touch.started -= OnTouchStarted;
        m_inputActions.Gameplay.Touch.canceled -= OnTouchCanceled;

        m_inputActions.Gameplay.Move.started -= OnMoveStarted;
        m_inputActions.Gameplay.Move.performed -= OnMovePerformed;
        m_inputActions.Gameplay.Move.canceled -= OnMoveCanceled;

        m_inputActions.Gameplay.Disable();
        m_inputActions.Dispose();
    }

    public override void Update()
    {
        if (!m_isCharging) return;
        OnChargeUpdate?.Invoke(m_lastDirection, GetChargeRatio());
    }
    
    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        Vector2 value = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        m_joystickRectT.position = new Vector2(value.x, value.y);
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        m_joystickRectT.position = m_joystickRect;
    }

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        m_chargeStartTime = Time.time;
        m_isCharging = true;
        m_lastDirection = Vector2.zero;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        if (value.magnitude < m_stats.MinStickMagnitude) return;

        Vector2 direction = value.normalized;
        m_lastDirection = m_stats.InvertStickDirection ? -direction : direction;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!m_isCharging) return;
        m_isCharging = false;

        if (m_lastDirection != Vector2.zero)
            OnJoystickRelease?.Invoke(m_lastDirection, GetChargeRatio());

        OnChargeEnd?.Invoke();
    }

    private float GetChargeRatio()
    {
        float held = Time.time - m_chargeStartTime;
        return Mathf.Clamp(held / m_stats.MaxChargeTime, m_stats.MinChargeRatio, 1f);
    }
}