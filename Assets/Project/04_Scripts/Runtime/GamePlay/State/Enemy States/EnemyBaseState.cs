public class EnemyBaseState : IState
{
    private Enemy m_owner;

    public EnemyBaseState(Enemy owner)
    {
        m_owner = owner;
    }
    
    public virtual void OnEnter()
    {
        
    }

    public virtual void OnExit()
    {
       
    }

    public virtual void Update()
    {
        
    }

    public virtual void FixedUpdate()
    {
        
    }
}