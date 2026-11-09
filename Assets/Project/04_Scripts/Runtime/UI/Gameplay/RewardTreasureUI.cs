using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class RewardTreasureUI : MonoBehaviour
{
    [SerializeField] private List<SO_Card> cards;
    [SerializeField] private CardUI cardUIPrefab;
    [SerializeField, Min(0)] private int numberCards = 2;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField, Min(0f)] private float spawnDuration = 0.5f;

    private AugmentService m_augmentService;
    private List<CardUI> m_spawned = new();

    private void Awake()
    {
        m_augmentService = ServiceLocator.Get<AugmentService>();
    }

    private void OnEnable()
    {
        Setup();
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Setup()
    {
        List<SO_Card> pool = new List<SO_Card>();

        m_augmentService.FillAvailableCards(cards, pool);

        int count = Mathf.Min(pool.Count, numberCards);

        if (count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        m_spawned = new List<CardUI>(count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            SO_Card card = pool[index];
            pool.RemoveAt(index);

            CardUI cardUI = Instantiate(cardUIPrefab, layoutGroup.transform);

            cardUI.transform.localScale = Vector3.zero;
            cardUI.Setup(card);
            cardUI.OnChosen += HandleCardChosen;

            m_spawned.Add(cardUI);
        }

        AnimateSpawnAsync(m_spawned).Forget();
    }

    private void Clear()
    {
        foreach (CardUI card in m_spawned)
        {
            if (card) Destroy(card.gameObject);
        }

        m_spawned = new List<CardUI>();
    }

    private void HandleCardChosen(SO_Card card)
    {
        foreach (CardUI spawned in m_spawned)
        {
            if (spawned) spawned.SetInteractable(false);
        }

        m_augmentService.AddCard(card);
        gameObject.SetActive(false);
    }

    private async UniTaskVoid AnimateSpawnAsync(List<CardUI> spawned)
    {
        foreach (CardUI card in spawned)
        {
            if (!card) return;

            await Tween.Scale(card.transform, Vector3.one, spawnDuration);
        }

        foreach (CardUI card in spawned)
        {
            if (card) card.SetInteractable(true);
        }
    }
}