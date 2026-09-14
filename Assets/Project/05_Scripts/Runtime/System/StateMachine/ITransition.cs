using UnityEngine;

public interface ITransition 
{
    IState To { get; }
    IPredictate Condition { get; }
}
