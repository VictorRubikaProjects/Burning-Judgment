using System.Collections.Generic;


public class Actor : Pawn
{
    private readonly List<ActorComponent> components = new();
    

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        for (int i = 0; i < components.Count; i++)
            components[i].Update();
    }

    protected T AddActorComponent<T>(T component) where T : ActorComponent
    {
        components.Add(component);
        component.Initialize();
        return component;
    }

    public T GetActorComponent<T>() where T : ActorComponent
    {
        for (int i = 0; i < components.Count; i++)
            if (components[i] is T match)
                return match;
        return null;
    }

}
