using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RarityWeight
{
    public CardRarity Rarity;
    [Range(0f, 100f)] public float Weight;
}

[CreateAssetMenu(menuName = "Config/Card/Rarity Drop Table", fileName = "SO_RarityDropTable")]
public class SO_RarityDropTable : ScriptableObject
{
    [SerializeField] private List<RarityWeight> weights = new()
    {
        new RarityWeight { Rarity = CardRarity.Common, Weight = 70f },
        new RarityWeight { Rarity = CardRarity.Epic, Weight = 25f },
        new RarityWeight { Rarity = CardRarity.Legendary, Weight = 5f },
    };

    public CardRarity RollRarity()
    {
        float totalWeight = 0f;
        foreach (var w in weights) totalWeight += w.Weight;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var w in weights)
        {
            cumulative += w.Weight;
            if (roll <= cumulative)
                return w.Rarity;
        }

        return CardRarity.Common;
    }
}