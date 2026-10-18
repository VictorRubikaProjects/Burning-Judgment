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

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        List<SO_Card> pool = new List<SO_Card>(cards);
        int count = Mathf.Min(pool.Count, numberCards);
        List<CardUI> spawned = new List<CardUI>(count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            SO_Card card = pool[index];
            pool.RemoveAt(index);

            CardUI cardUI = Instantiate(cardUIPrefab, layoutGroup.transform);
            cardUI.transform.localScale = Vector3.zero;
            cardUI.Setup(card);

            spawned.Add(cardUI);
        }

        AnimateSpawnAsync(spawned).Forget();
    }

    private async UniTaskVoid AnimateSpawnAsync(List<CardUI> spawned)
    {
        foreach (CardUI card in spawned)
        {
            if (!card) return;

            await Tween.Scale(card.transform, Vector3.one, spawnDuration);
        }
    }
}