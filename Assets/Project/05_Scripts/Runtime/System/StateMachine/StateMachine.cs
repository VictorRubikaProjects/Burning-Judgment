using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine 
{
    private StateNode m_current;
    Dictionary<Type,StateNode> m_nodes = new();
    HashSet<ITransition> m_anyTransitions = new();

    public void Update()
    {
        var transition = GetTransition();
        
        if (transition != null)
        {
            ChangeState(transition.To);
        }
    }

    public void FixedUpdate()
    {
        m_current.State?.FixedUpdate();
        
    }

    public void SetState(IState state)
    {
        m_current = m_nodes[state.GetType()];
        m_current.State?.OnEnter();
    }

    private void ChangeState(IState state)
    {
        if (state == m_current.State) return;
        
        var previousState = m_current.State;
        var nextState = m_nodes[state.GetType()].State;
        
        previousState?.OnExit();
        nextState?.OnEnter();
        
        m_current = m_nodes[state.GetType()];
    }

    ITransition GetTransition()
    {
        foreach (var transition in m_anyTransitions)
        {
            if (transition.Condition.Evaluate()) 
                return transition;
        }

        foreach (var transition in m_current.Transitions)
        {
            if (transition.Condition.Evaluate())
                return transition;
        }
        
        return null;
    }

    public void AddTransition(IState from, IState to, IPredictate condition)
    {
        GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
    }

    public void AddAnyTransition(IState to, IPredictate condition)
    {
        m_anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
    }

    StateNode GetOrAddNode(IState state)
    {
        var node = m_nodes.GetValueOrDefault(state.GetType());

        if (node == null)
        {
            node = new StateNode(state);
            m_nodes.Add(state.GetType(), node);
        }
        
        return node;
    }
}
