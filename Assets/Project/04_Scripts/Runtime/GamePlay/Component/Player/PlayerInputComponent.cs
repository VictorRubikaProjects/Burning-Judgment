using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : ActorComponent
{
    #region Variables

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
    public event Action<Vector2, float> OnSwipe;
    public event Action OnChargeEnd;

    private RectTransform m_joystickRectT;
    
    private Vector2 m_lastDirection;
    private Vector2 m_joystickRect;
    private Vector2 m_touchStartPosition;
    
    private readonly CanvasGroup m_joystickGroup;

    #endregion

    #region Actor Component Core

    public PlayerInputComponent(Actor owner, SO_PlayerStats stats, RectTransform joystickRectT) : base(owner)
    {
        m_stats = stats;
        m_joystickRectT = joystickRectT;
        m_joystickRect = joystickRectT.position;

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
        HandleJoystickPlacement();

        if (!m_isCharging) return;
        
        OnChargeUpdate?.Invoke(m_lastDirection, GetChargeRatio());
    }

    #endregion

    #region Joystick

    private void HandleJoystickPlacement()
    {
        if (!m_isTouching || m_isJoystickVisible) return;
        
        float elapsedTouchTime = Time.time - m_touchStartTime;

        if (!(elapsedTouchTime >= m_stats.MaxSwipeDuration)) return;
        
        SetJoystickVisible(true);
    }
    
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        if (!m_isJoystickVisible) return;

        m_chargeStartTime = Time.time;
        m_isCharging = true;
        m_lastDirection = Vector2.zero;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!m_isJoystickVisible) return;

        Vector2 value = context.ReadValue<Vector2>();
        bool isBelowMinMagnitude = value.magnitude < m_stats.MinStickMagnitude;

        if (isBelowMinMagnitude) return;

        if (!m_isDragging)
        {
            m_isDragging = true;
        }

        if (!m_isCharging)
        {
            m_chargeStartTime = Time.time;
            m_isCharging = true;
        }

        Vector2 direction = value.normalized;
        m_lastDirection = m_stats.InvertStickDirection ? -direction : direction;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!m_isCharging) return;
        
        m_isCharging = false;

        if (m_lastDirection != Vector2.zero)
        {
            OnJoystickRelease?.Invoke(m_lastDirection, GetChargeRatio());
        }

        OnChargeEnd?.Invoke();
    }

    private float GetChargeRatio()
    {
        float heldDuration = Time.time - m_chargeStartTime;
        return Mathf.Clamp(heldDuration / m_stats.MaxChargeTime, m_stats.MinChargeRatio, 1f);
    }
    
    private void SetJoystickVisible(bool visible)
    {
        m_isJoystickVisible = visible;
        m_joystickGroup.alpha = visible ? 1f : 0f;
    }

    #endregion

    #region Swipe

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        m_touchStartPosition = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        m_joystickRectT.position = m_touchStartPosition;
        
        m_touchStartTime = Time.time;
        m_isTouching = true;
        m_isDragging = false;
        
        SetJoystickVisible(false);
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        m_isTouching = false;
        SetJoystickVisible(false);
        m_joystickRectT.position = m_joystickRect;

        if (m_isDragging || m_isJoystickVisible) return;

        float elapsedTouchTime = Time.time - m_touchStartTime;
        Vector2 currentPosition = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        Vector2 swipeVector = currentPosition - m_touchStartPosition;
        
        bool isWithinDuration = elapsedTouchTime <= m_stats.MaxSwipeDuration;
        bool hasExceededMinDistance = swipeVector.magnitude >= m_stats.MinSwipeDistance;

        if (!isWithinDuration || !hasExceededMinDistance) return;

        float ratio = Mathf.Clamp01(swipeVector.magnitude / m_stats.MaxSwipeDistance);
        
        OnSwipe?.Invoke(swipeVector.normalized, ratio);
    }

    #endregion
    
}