using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;


public class AugmentService : IGameService
{
    private readonly List<SO_Card> m_ownedCards = new();
    private readonly List<SO_Card> m_availableBuffer = new();

    private AugmentDeltas m_total;

    public event Action<AugmentDeltas> OnTotalChanged;

    public AugmentDeltas Total => m_total;
    
    public IReadOnlyList<SO_Card> OwnedCards => m_ownedCards;

    #region IGameService region

    public void Dispose()
    {
        OnTotalChanged = null;
        m_ownedCards.Clear();
        m_availableBuffer.Clear();
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

    public List<SO_Card> DrawCards(IEnumerable<SO_Card> catalog, SO_RarityDropTable dropTable, int count)
    {
        FillAvailableCards(catalog, m_availableBuffer);

        List<SO_Card> result = new List<SO_Card>(count);

        Dictionary<CardRarity, List<SO_Card>> poolsByRarity = new();
        foreach (SO_Card card in m_availableBuffer)
        {
            if (!poolsByRarity.TryGetValue(card.Rarity, out List<SO_Card> list))
            {
                list = new List<SO_Card>();
                poolsByRarity[card.Rarity] = list;
            }
            list.Add(card);
        }

        for (int i = 0; i < count; i++)
        {
            if (poolsByRarity.Count == 0) break;

            CardRarity rolledRarity = dropTable ? dropTable.RollRarity() : CardRarity.Common;

            if (!poolsByRarity.TryGetValue(rolledRarity, out List<SO_Card> targetPool) || targetPool.Count == 0)
            {
                rolledRarity = poolsByRarity.Keys.First();
                targetPool = poolsByRarity[rolledRarity];
            }

            int randomIndex = UnityEngine.Random.Range(0, targetPool.Count);
            SO_Card chosenCard = targetPool[randomIndex];

            result.Add(chosenCard);

            targetPool.RemoveAt(randomIndex);
            if (targetPool.Count == 0)
            {
                poolsByRarity.Remove(rolledRarity);
            }
        }

        return result;
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