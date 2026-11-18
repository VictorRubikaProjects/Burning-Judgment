using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RewardTreasureUI : MonoBehaviour
{
    [SerializeField] private List<SO_Card> cards; 
    [SerializeField] private SO_RarityDropTable dropTable; 
    [SerializeField] private CardUI cardUIPrefab;
    [SerializeField, Min(0)] private int numberCards = 2;
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    [SerializeField, Min(0f)] private float spawnDuration = 2f;

    [Header("Theming")]
    [SerializeField] private SO_CardThemeDatabase themeDatabase;

    private AugmentService m_augmentService;
    private List<CardUI> m_spawned = new();
    
    private CancellationTokenSource m_ctReward;

    private void Awake() => m_augmentService = ServiceLocator.Get<AugmentService>();

    private void OnDisable() => Clear();

    public void Setup(CancellationTokenSource cts)
    {
        m_ctReward = cts;
        
        List<SO_Card> drawnCards = m_augmentService.DrawCards(cards, dropTable, numberCards);

        if (drawnCards.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        m_spawned = new List<CardUI>(drawnCards.Count);

        foreach (SO_Card card in drawnCards)
        {
            CardUI cardUI = Instantiate(cardUIPrefab, layoutGroup.transform);
            cardUI.transform.localScale = Vector3.zero;
            cardUI.Setup(card, themeDatabase);
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
        m_spawned.Clear();
    }

    private async void HandleCardChosen(CardUI clickedCard, SO_Card card)
    {
        foreach (CardUI spawned in m_spawned)
        {
            if (spawned) spawned.SetInteractable(false);
        }

        await clickedCard.PlaySelectAnimationAsync(0.4f);

        m_augmentService.AddCard(card);
        gameObject.SetActive(false);
        m_ctReward.Cancel();
    }

    private async UniTaskVoid AnimateSpawnAsync(List<CardUI> spawned)
    {
        foreach (CardUI card in spawned)
        {
            if (!card) return;
            await card.FlipAsync(spawnDuration);
        }
    }
}