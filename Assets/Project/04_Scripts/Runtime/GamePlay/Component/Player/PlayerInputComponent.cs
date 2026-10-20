using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : ActorComponent
{
    private readonly SO_PlayerStats m_stats;
    
    private PlayerActions m_inputActions;
    
    private float m_chargeStartTime;
    private float m_touchStartTime;
    
    private bool m_isCharging;
    private bool m_isTouching;
    private bool m_isJoystickVisible;
    private bool m_isDragging;

    public event Action<Vector2, float> OnJoystickRelease;
    public event Action<Vector2, float> OnChargeUpdate;
    public event Action<Vector2, float> OnTap;
    public event Action OnChargeEnd;

    private RectTransform m_joystickRectT;
    private readonly RectTransform m_originRectT;
    
    private Vector2 m_lastDirection;
    private Vector2 m_joystickRect;
    private Vector2 m_touchStartPosition;
    
    private readonly CanvasGroup m_joystickGroup;
    
    public PlayerInputComponent(Actor owner, SO_PlayerStats stats, RectTransform joystickRectT, RectTransform originRectT) : base(owner)
    {
        m_stats = stats;
        m_joystickRectT = joystickRectT;
        m_joystickRect = joystickRectT.position;
        m_originRectT = originRectT;

        m_joystickGroup = joystickRectT.TryGetComponent(out CanvasGroup group)
            ? group
            : joystickRectT.gameObject.AddComponent<CanvasGroup>();

        SetJoystickVisible(false);
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
        if (m_isTouching && !m_isJoystickVisible && Time.time - m_touchStartTime > m_stats.MaxTapDuration)
            SetJoystickVisible(true);

        if (!m_isCharging) return;
        OnChargeUpdate?.Invoke(m_lastDirection, GetChargeRatio());
    }
    
    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        m_touchStartPosition = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        m_touchStartTime = Time.time;
        m_isTouching = true;
        m_isDragging = false;
        m_joystickRectT.position = m_touchStartPosition;
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        m_isTouching = false;
        SetJoystickVisible(false);
        m_joystickRectT.position = m_joystickRect;

        if (m_isDragging) return;
        if (Time.time - m_touchStartTime > m_stats.MaxTapDuration) return;

        Vector2 offset = m_touchStartPosition - (Vector2)m_originRectT.position;
        float ratio = Mathf.Clamp01(offset.magnitude / m_stats.MaxTapDistance);
        OnTap?.Invoke(offset.normalized, ratio);
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

        if (!m_isDragging)
        {
            m_isDragging = true;
            SetJoystickVisible(true);
        }

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
    
    private void SetJoystickVisible(bool visible)
    {
        m_isJoystickVisible = visible;
        m_joystickGroup.alpha = visible ? 1f : 0f;
    }
}