using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputComponent : ActorComponent
{
    #region Variables

    private readonly RectTransform m_pivot;
    private readonly float m_tapThreshold;
    private readonly float m_maxTapTime;

    private PlayerActions m_inputActions;

    private Vector2 m_startPos;
    private float m_startTime;
    
    public event Action<SwipeDirection> OnSwipe;
    public event Action<Vector2> OnTap;
    
    public enum SwipeDirection { Up, Down }

    #endregion
    
    #region ActorComponent Methods
    
    public InputComponent(Actor owner, RectTransform pivot, float tapThreshold = 30f, float maxTapTime = 0.3f) : base(owner)
    {
        if (!pivot)
        {
            Debug.LogError("Pivot not set");
            return;
        }
        
        this.m_pivot = pivot;
        this.m_tapThreshold = tapThreshold;
        this.m_maxTapTime = maxTapTime;
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
        m_startTime = Time.time;
    }

    private void TouchEnded(InputAction.CallbackContext context)
    {
        Vector2 endPos = m_inputActions.Gameplay.Position.ReadValue<Vector2>();
        Vector2 delta = endPos - m_startPos;
        float elapsed = Time.time - m_startTime;

        if (delta.magnitude <= m_tapThreshold && elapsed <= m_maxTapTime)
        {
            HandleDialTap(m_startPos);
            return;
        }

        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            HandleSwipe(delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down);
    }

    private void HandleDialTap(Vector2 screenPos)
    {
        //cam : null because canvas overlay
        Vector2 pivotScreenPos = RectTransformUtility.WorldToScreenPoint(null, m_pivot.position); 
        Vector2 moveDir = (screenPos - pivotScreenPos).normalized;

        OnTap?.Invoke(moveDir);
    }

    private void HandleSwipe(SwipeDirection dir) => OnSwipe?.Invoke(dir);

    #endregion
    
}