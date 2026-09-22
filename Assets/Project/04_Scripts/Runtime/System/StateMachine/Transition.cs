using UnityEngine;

public class Transition : ITransition
{
    public IState To { get; }
    
    public IPredictate Condition { get; }

    public Transition(IState to, IPredictate condition)
    {
        To = to;
        Condition = condition;
    }
}
