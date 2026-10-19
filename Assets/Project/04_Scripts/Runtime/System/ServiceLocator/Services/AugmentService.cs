using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AugmentService : IGameService
{
    private readonly List<SO_Card> m_ownedCards = new();

    private AugmentDeltas m_total;

    public event Action<AugmentDeltas> OnTotalChanged;

    public AugmentDeltas Total => m_total;
    
    public IReadOnlyList<SO_Card> OwnedCards => m_ownedCards;

    #region IGameService region

    public void Dispose()
    {
        OnTotalChanged = null;
        m_ownedCards.Clear();
    }

    public UniTask InitializeService()
    {
        Reset();

        return UniTask.CompletedTask;
    }

    public void ShutDownService()
    {
        Reset();
    }

    public void Tick() { }

    public bool IsInitialized { get; set; }

    #endregion

    public bool AddCard(SO_Card card)
    {
        if (!card) return false;
        if (!card.IsStackable && m_ownedCards.Contains(card)) return false;

        m_ownedCards.Add(card);
        RecomputeTotal();

        return true;
    }

    public bool RemoveCard(SO_Card card)
    {
        if (!m_ownedCards.Remove(card)) return false;

        RecomputeTotal();

        return true;
    }

    public void Reset()
    {
        m_ownedCards.Clear();
        RecomputeTotal();
    }

    public void FillAvailableCards(IEnumerable<SO_Card> catalog, List<SO_Card> results)
    {
        results.Clear();

        foreach (SO_Card card in catalog)
        {
            if (!card) continue;
            if (!card.IsStackable && m_ownedCards.Contains(card)) continue;

            results.Add(card);
        }
    }

    private void RecomputeTotal()
    {
        AugmentDeltas total = default;

        foreach (SO_Card card in m_ownedCards)
        {
            total += card.Deltas;
        }

        m_total = total;
        OnTotalChanged?.Invoke(m_total);
    }
}