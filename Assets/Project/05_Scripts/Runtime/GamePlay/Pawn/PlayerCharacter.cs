using UnityEngine;
using SwipeDirection = InputComponent.SwipeDirection;

public class PlayerCharacter : Actor
{
    [SerializeField] private PlayerHUD hud;
    
    public InputComponent Input {get; private set;}

    protected override void Awake()
    {
        base.Awake();
        
        Input = new InputComponent(owner: this, pivot: hud.PivotJoystick);
    }

    protected override void Start()
    {
        base.Start();
        AddActorComponent(Input);
    }

    private void OnEnable()
    {
        Input.OnTap += TapHandler;
        Input.OnSwipe +=  SwipeHandler;
    }

    private void OnDisable()
    {
        Input.OnTap -= TapHandler;
        Input.OnSwipe -=  SwipeHandler;
    }
    
    private void TapHandler(Vector2 position)
    {
        Debug.Log("Tap Handler Angle :  " + position);
    }
    private void SwipeHandler(SwipeDirection direction)
    {
        Debug.Log("Swipe Handler :  " + direction);
    }
}
