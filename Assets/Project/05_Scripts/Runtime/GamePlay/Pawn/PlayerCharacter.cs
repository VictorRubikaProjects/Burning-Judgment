using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerCharacter : Actor
{
    [Header("References")]
    [SerializeField] private PlayerHUD hud;
    [SerializeField] private Rigidbody rb;

    public InputComponent Input { get; private set; }

    private CancellationTokenSource m_ctsMove;
    private bool m_isMoving;

    protected override void Awake()
    {
        base.Awake();

        m_ctsMove = new CancellationTokenSource();

        Input = new InputComponent(owner: this);
    }

    protected override void Start()
    {
        base.Start();
        AddActorComponent(Input);
    }

    private void OnEnable()
    {
        Input.OnSwipe += SwipeHandler;
    }

    private void OnDisable()
    {
        Input.OnSwipe -= SwipeHandler;
    }

    private void OnDestroy()
    {
        m_ctsMove.Cancel();
        m_ctsMove.Dispose();
    }

    private void SwipeHandler(Vector2 moveDir)
    {
        Vector3 worldDir = new Vector3(moveDir.x, 0f, moveDir.y);
        transform.rotation = Quaternion.LookRotation(worldDir);

        if (m_isMoving) return;

        Dash(m_ctsMove.Token).Forget();
    }

    private async UniTask Dash(CancellationToken token)
    {
        m_isMoving = true;
        float dashDistance = 1f;
        float dashDuration = 0.1f;

        Vector3 start = rb.position;
        Vector3 target = start + transform.forward * dashDistance;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / dashDuration;
            rb.MovePosition(Vector3.Lerp(start, target, t));
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token).SuppressCancellationThrow();
            if (token.IsCancellationRequested) return;
        }

        rb.MovePosition(target);
        m_isMoving = false;
    }
}