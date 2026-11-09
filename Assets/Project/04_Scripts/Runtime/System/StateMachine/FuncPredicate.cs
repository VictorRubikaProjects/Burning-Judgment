using System;

public class FuncPredicate : IPredictate
{
    protected readonly Func<bool> m_func;

    public FuncPredicate(Func<bool> func)
    {
        this.m_func = func;
    }
    
    public bool Evaluate()
    {
        return m_func.Invoke();
    }
}
