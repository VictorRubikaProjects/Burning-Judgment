using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SwipeDirection = InputComponent.SwipeDirection;

public class PlayerCharacter : Actor
{
    [Header("References")]
    [SerializeField] private PlayerHUD hud;
    [SerializeField] private Rigidbody rb;
    
    public InputComponent Input {get; private set;}

    private CancellationTokenSource m_ctsMove; 

    protected override void Awake()
    {
        base.Awake();
        
        m_ctsMove = new CancellationTokenSource();
        
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
    
    private bool isMoving;
    
    private void TapHandler(Vector2 moveDir)
    {
        Vector3 worldDir = new Vector3(moveDir.x, 0f, moveDir.y);
        transform.rotation = Quaternion.LookRotation(worldDir);

        if (isMoving) return;

        Move().Forget();
    }

    private async UniTask Move()
    {
        isMoving = true;
        float moveDistance = 1f;
        float moveDuration = 0.1f;
        
        Vector3 start = rb.position;
        Vector3 target = start + transform.forward * moveDistance;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / moveDuration;
            rb.MovePosition(Vector3.Lerp(start, target, t));
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }

        rb.MovePosition(target);
        isMoving = false;
    }
    
    private void SwipeHandler(SwipeDirection direction)
    {
        Debug.Log("Swipe Handler :  " + direction);
    }
}
