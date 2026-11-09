using UnityEngine;
using UnityEngine.Events;

public class StrikeableElement : MonoBehaviour, IStrikeable
{
    [SerializeField] private bool cancelsDash = true;
    [SerializeField] private bool allowsChain = true;
    [SerializeField] private UnityEvent onStruck;

    public StrikeResult OnStruck(in StrikeContext context)
    {
        onStruck?.Invoke();
        return new StrikeResult(true, cancelsDash, allowsChain);
    }
}