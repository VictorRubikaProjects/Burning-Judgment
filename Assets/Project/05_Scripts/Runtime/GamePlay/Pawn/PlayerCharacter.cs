using UnityEngine;

public class PlayerCharacter : Actor
{
    #region Variable

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    
    [Header("Config")]
    [SerializeField] private ConfigStatsPlayer stats;

    public InputComponent Input { get; private set; }

    private StateMachine m_stateMachine;

    private Vector3 m_moveDir;
    private bool m_wantsDash;
    private bool m_isHit;
    private bool m_isDead;

    public Vector3 MoveDir => m_moveDir;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        SetupStateMachine();

        Input = new InputComponent(owner: this);
    }

    protected override void Start()
    {
        base.Start();
        
        AddActorComponent(Input);
        
        ServiceLocator.Get<CameraService>().AddTarget(transform,0.5f);
    }

    private void OnEnable()
    {
        Input.OnSwipe += SwipeHandler;
    }

    private void OnDisable()
    {
        Input.OnSwipe -= SwipeHandler;
    }

    protected override void Update()
    {
        base.Update();
        
        m_stateMachine.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        m_stateMachine.FixedUpdate();
    }

    #endregion

    #region State Machine

    void At(IState from, IState to, IPredictate condition) => m_stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredictate condition) => m_stateMachine.AddAnyTransition(to, condition);
    
    private void SetupStateMachine()
    {
        m_stateMachine = new StateMachine();

        var idleState = new IdleState(this, animator,stats);
        var dashState = new DashState(this, animator, rb,stats);
        var hitState = new HitState(this, animator,stats);
        var deathState = new DeathState(this, animator,stats);

        At(idleState, dashState, new FuncPredicate(() => m_wantsDash));
        At(dashState, idleState, new FuncPredicate(() => dashState.IsFinished));
        At(hitState, idleState, new FuncPredicate(() => hitState.IsFinished));

        Any(hitState, new FuncPredicate(() => m_isHit));
        Any(deathState, new FuncPredicate(() => m_isDead));

        m_stateMachine.SetState(idleState);
    }

    #endregion
    
    private void SwipeHandler(Vector2 moveDir)
    {
        Vector3 worldDir = new Vector3(moveDir.x, 0f, moveDir.y);
        transform.rotation = Quaternion.LookRotation(worldDir);
        m_moveDir = worldDir;
        m_wantsDash = true;
    }

    public void ConsumeDashRequest() => m_wantsDash = false;
    public void ConsumeHitRequest() => m_isHit = false;

    public void RequestHit() => m_isHit = true;
    public void Kill() => m_isDead = true;

    
}