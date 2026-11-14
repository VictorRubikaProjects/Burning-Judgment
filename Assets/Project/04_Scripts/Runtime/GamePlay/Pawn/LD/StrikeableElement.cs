using UnityEngine;
using UnityEngine.Events;

public class StrikeableElement : MonoBehaviour, IStrikeable
{
    [Header("Strikeable Settings")]
    [SerializeField] protected bool cancelsDash = true;
    [SerializeField] protected bool allowsChain = true;
    [SerializeField] protected UnityEvent onStruck;

    public StrikeResult OnStruck(in StrikeContext context)
    {
        onStruck?.Invoke();
        return new StrikeResult(true, cancelsDash, allowsChain);
    }
}