using System;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void DirectionRatioHandler(Vector2 direction, float ratio);

public class PlayerInputComponent : ActorComponent
{
    private readonly SO_PlayerStats m_stats;
    private readonly RectTransform m_joystickTransform;
    private readonly CanvasGroup m_joystickGroup;
    private readonly Vector2 m_joystickRestPosition;

    private PlayerActions m_inputActions;

    private Vector2 m_touchStartPosition;
    private Vector2 m_lastDirection;

    private float m_touchStartTime;

    private bool m_isTouching;
    private bool m_isJoystickVisible;
    private bool m_isStickEngaged;
    private bool m_isCharging;

    public event DirectionRatioHandler OnJoystickRelease;
    public event DirectionRatioHandler OnChargeUpdate;
    public event DirectionRatioHandler OnSwipe;
    public event Action OnChargeEnd;
    public event Action OnChargeStarted;
    public event Action OnTouchPressed;
    public event Action OnHoldConfirmed;
    public event Action OnTouchReleased;

    private PlayerCharacter m_pc;

    private bool m_isEnable;

    public PlayerInputComponent(Actor owner, SO_PlayerStats stats, RectTransform joystickTransform) : base(owner)
    {
        m_pc = owner as PlayerCharacter;
        m_stats = stats;
        m_joystickTransform = joystickTransform;
        m_joystickRestPosition = joystickTransform.position;

        m_joystickGroup = joystickTransform.TryGetComponent(out CanvasGroup group)
            ? group
            : joystickTransform.gameObject.AddComponent<CanvasGroup>();

        SetJoystickVisible(false);

        m_pc.OnControlContextChanged += OnControlContextChanged;
    }

    public override void Initialize() => SetupInput();
    public override void Dispose()
    {
        StopInput();
        m_pc.OnControlContextChanged -= OnControlContextChanged;
    }

    private void SetupInput()
    {
        m_inputActions = new PlayerActions();
        m_inputActions.Gameplay.Enable();

        m_inputActions.Gameplay.Touch.started += OnTouchStarted;
        m_inputActions.Gameplay.Touch.canceled += OnTouchCanceled;

        m_inputActions.Gameplay.Move.started += OnMoveStarted;
        m_inputActions.Gameplay.Move.performed += OnMovePerformed;
        m_inputActions.Gameplay.Move.canceled += OnMoveCanceled;

        m_isEnable = true;
    }

    private void StopInput()
    {
        m_inputActions.Gameplay.Touch.started -= OnTouchStarted;
        m_inputActions.Gameplay.Touch.canceled -= OnTouchCanceled;

        m_inputActions.Gameplay.Move.started -= OnMoveStarted;
        m_inputActions.Gameplay.Move.performed -= OnMovePerformed;
        m_inputActions.Gameplay.Move.canceled -= OnMoveCanceled;
        
        m_inputActions.Gameplay.Disable();
        m_inputActions.Dispose();

        m_isEnable = false;
    }

    public override void Update()
    {
        ShowJoystickAfterHold();

        if (!m_isCharging) return;

        OnChargeUpdate?.Invoke(m_lastDirection, GetChargeRatio());
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        m_touchStartPosition = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        m_joystickTransform.position = m_touchStartPosition;

        m_touchStartTime = Time.unscaledTime;
        m_isTouching = true;
        m_isStickEngaged = false;

        SetJoystickVisible(false);

        OnTouchPressed?.Invoke();
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        m_isTouching = false;
        SetJoystickVisible(false);
        m_joystickTransform.position = m_joystickRestPosition;

        if (m_isCharging)
        {
            m_isCharging = false;

            if (m_lastDirection != Vector2.zero)
            {
                OnJoystickRelease?.Invoke(m_lastDirection, GetChargeRatio());
            }

            OnChargeEnd?.Invoke();
        }
        else if (!m_isStickEngaged)
        {
            TryEmitSwipe();
        }

        OnTouchReleased?.Invoke();
    }

    private void ShowJoystickAfterHold()
    {
        if (!m_isTouching || m_isJoystickVisible) return;
        if (TouchElapsed < m_stats.MaxSwipeDuration) return;

        SetJoystickVisible(true);
        OnHoldConfirmed?.Invoke();
    }

    private void SetJoystickVisible(bool visible)
    {
        m_isJoystickVisible = visible;
        m_joystickGroup.alpha = visible ? 1f : 0f;
    }

    private float TouchElapsed => Time.unscaledTime - m_touchStartTime;

    private void TryEmitSwipe()
    {
        Vector2 currentPosition = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        Vector2 swipeVector = currentPosition - m_touchStartPosition;

        bool isQuickEnough = TouchElapsed <= m_stats.MaxSwipeDuration;

        if (!isQuickEnough) return;

        float ratio = Mathf.Clamp01(swipeVector.magnitude / m_stats.MaxSwipeDistance);

        OnSwipe?.Invoke(swipeVector.normalized, ratio);
    }

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        if (!m_isJoystickVisible) return;

        BeginCharge();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!m_isJoystickVisible) return;

        Vector2 stickValue = context.ReadValue<Vector2>();

        if (stickValue.magnitude < m_stats.MinStickMagnitude) return;

        if (!m_isStickEngaged) EngageStick();
        if (!m_isCharging) BeginCharge();

        Vector2 direction = stickValue.normalized;
        m_lastDirection = m_stats.InvertStickDirection ? -direction : direction;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!m_isCharging) return;
        
        m_lastDirection = Vector2.zero;
    }

    private void BeginCharge()
    {
        m_isCharging = true;
        m_lastDirection = Vector2.zero;
    }

    private void EngageStick()
    {
        m_isStickEngaged = true;
        
        OnChargeStarted?.Invoke();
    }

    private float GetChargeRatio()
    {
        Vector2 currentPosition = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        float stretchMagnitude = (currentPosition - m_touchStartPosition).magnitude;
        float ratio = stretchMagnitude / m_stats.MaxSwipeDistance;

        return Mathf.Clamp(ratio, m_stats.MinChargeRatio, 1f);
    }

    private void OnControlContextChanged(PlayerControlContext context)
    {
        if (PlayerActionPermissions.FromContext(context).InputAvailable)
        {
            if (m_isEnable) return;
            SetupInput();
        }
        else
        {
            if (!m_isEnable) return;
            StopInput();
        }
    }
}