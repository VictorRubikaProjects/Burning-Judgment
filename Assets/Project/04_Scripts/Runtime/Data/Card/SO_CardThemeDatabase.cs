using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Card/Card Theme Database", fileName = "SO_CardThemeDatabase")]
public class SO_CardThemeDatabase : ScriptableObject
{
    [SerializeField] private List<SO_CardRarityTheme> rarityThemes = new();
    
    private Dictionary<CardRarity, SO_CardRarityTheme> m_themeCache;

    private void InitCache()
    {
        if (m_themeCache != null) return;
        m_themeCache = new Dictionary<CardRarity, SO_CardRarityTheme>();
        
        foreach (var theme in rarityThemes)
        {
            if (theme != null)
            {
                m_themeCache.TryAdd(theme.Rarity, theme);
            }
        }
    }

    public SO_CardRarityTheme GetTheme(CardRarity rarity)
    {
        InitCache();
        if (m_themeCache.TryGetValue(rarity, out var theme))
        {
            return theme;
        }
        Debug.LogWarning($"Aucun thème trouvé pour la rareté : {rarity}");
        return null;
    }

    public string GetStatsSummaryByDeltas(AugmentDeltas deltas)
    {
        string stats = "";

        List<Tuple<string, float>> statsList = deltas.GetAllAugmentFromDelta();

        foreach (var stat in statsList)
        {
            if (stat == null || stat.Item2 == 0) continue;

            string name = $" {stat.Item1}";
            
            bool positiveAugment = stat.Item2 > 0;

            name += positiveAugment ? stat.Item2 >= 30 ?
                    " ++\n" : " +\n" : stat.Item2 <= 30 ? " --\n" : " -\n";

            stats += name;
        }

        return stats;
    }
}