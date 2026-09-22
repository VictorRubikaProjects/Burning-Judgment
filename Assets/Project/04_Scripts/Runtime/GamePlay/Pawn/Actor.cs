using System.Collections.Generic;


public class Actor : Pawn
{
    private readonly List<ActorComponent> m_components = new();
    
    public virtual void Kill() { }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();

        foreach (ActorComponent component in m_components)
        {
            component.Update();
        }
            
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        foreach (ActorComponent component in m_components)
        {
            component.FixedUpdate();
        }
    }

    protected T AddActorComponent<T>(T component) where T : ActorComponent 
    {
        m_components.Add(component);
        component.Initialize();
        return component;
    }

    public T GetActorComponent<T>() where T : ActorComponent
    {
        foreach (ActorComponent component in m_components)
        {
            if (component is T match) return match;
        }
        
        return null;
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        foreach (ActorComponent component in m_components)
        {
            component.OnDrawGizmosSelected();
        }
    }

}
