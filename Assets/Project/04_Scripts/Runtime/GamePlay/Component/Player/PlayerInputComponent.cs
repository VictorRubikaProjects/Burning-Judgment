using System;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void DirectionRatioHandler(Vector2 direction, float ratio);

public class PlayerInputComponent : ActorComponent
{
    #region Variables

    private readonly SO_PlayerStats m_stats;
    private readonly RectTransform m_joystickTransform;
    private readonly CanvasGroup m_joystickGroup;
    private readonly TimeControllerComponent m_timeController;
    private readonly Vector2 m_joystickRestPosition;

    private PlayerActions m_inputActions;

    private Vector2 m_touchStartPosition;
    private Vector2 m_lastDirection;

    private float m_touchStartTime;
    private float m_chargeStartTime;

    private bool m_isTouching;
    private bool m_isJoystickVisible;
    private bool m_isStickEngaged;
    private bool m_isCharging;

    public event DirectionRatioHandler OnJoystickRelease;
    public event DirectionRatioHandler OnChargeUpdate;
    public event DirectionRatioHandler OnSwipe;
    public event Action OnChargeEnd;

    #endregion

    #region Actor Component Core

    public PlayerInputComponent(
        Actor owner,
        SO_PlayerStats stats,
        RectTransform joystickTransform,
        TimeControllerComponent timeController) : base(owner)
    {
        m_stats = stats;
        m_joystickTransform = joystickTransform;
        m_joystickRestPosition = joystickTransform.position;
        m_timeController = timeController;

        m_joystickGroup = joystickTransform.TryGetComponent(out CanvasGroup group)
            ? group
            : joystickTransform.gameObject.AddComponent<CanvasGroup>();

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
        ShowJoystickAfterHold();

        if (!m_isCharging) return;

        OnChargeUpdate?.Invoke(m_lastDirection, GetChargeRatio());
    }

    #endregion

    #region Touch

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        m_touchStartPosition = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        m_joystickTransform.position = m_touchStartPosition;

        m_touchStartTime = Time.time;
        m_isTouching = true;
        m_isStickEngaged = false;

        SetJoystickVisible(false);
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        m_isTouching = false;
        SetJoystickVisible(false);
        m_joystickTransform.position = m_joystickRestPosition;

        if (m_isStickEngaged) return;

        TryEmitSwipe();
    }

    private void ShowJoystickAfterHold()
    {
        if (!m_isTouching || m_isJoystickVisible) return;
        if (Time.time - m_touchStartTime < m_stats.MaxSwipeDuration) return;

        SetJoystickVisible(true);
    }

    private void SetJoystickVisible(bool visible)
    {
        m_isJoystickVisible = visible;
        m_joystickGroup.alpha = visible ? 1f : 0f;
    }

    #endregion

    #region Swipe

    private void TryEmitSwipe()
    {
        float elapsedTouchTime = Time.time - m_touchStartTime;
        Vector2 currentPosition = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        Vector2 swipeVector = currentPosition - m_touchStartPosition;

        bool isQuickEnough = elapsedTouchTime <= m_stats.MaxSwipeDuration;
        bool isLongEnough = swipeVector.magnitude >= m_stats.MinSwipeDistance;

        if (!isQuickEnough || !isLongEnough) return;

        float ratio = Mathf.Clamp01(swipeVector.magnitude / m_stats.MaxSwipeDistance);

        OnSwipe?.Invoke(swipeVector.normalized, ratio);
    }

    #endregion

    #region Joystick Charge

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

        m_isCharging = false;

        if (m_lastDirection != Vector2.zero)
        {
            OnJoystickRelease?.Invoke(m_lastDirection, GetChargeRatio());
        }

        m_timeController.LerpTimeDilationAsync(1f, m_stats.SlowMotionExitDuration).Forget();
        OnChargeEnd?.Invoke();
    }

    private void BeginCharge()
    {
        m_chargeStartTime = Time.unscaledTime; //unscaled time for slow motion impact
        m_isCharging = true;
        m_lastDirection = Vector2.zero;
    }

    private void EngageStick()
    {
        m_isStickEngaged = true;
        m_timeController.LerpTimeDilationAsync(m_stats.SlowMotionTargetDilation, m_stats.SlowMotionEnterDuration).Forget();
    }

    private float GetChargeRatio()
    {
        float heldDuration = Time.unscaledTime - m_chargeStartTime; //unscaled time used so slow motion no impact on charge.
        return Mathf.Clamp(heldDuration / m_stats.MaxChargeTime, m_stats.MinChargeRatio, 1f);
    }

    #endregion
}