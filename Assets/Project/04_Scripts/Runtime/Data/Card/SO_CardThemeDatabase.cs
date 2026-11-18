using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Card/Card Theme Database", fileName = "SO_CardThemeDatabase")]
public class SO_CardThemeDatabase : ScriptableObject
{
    [SerializeField] private List<SO_CardRarityTheme> rarityThemes = new List<SO_CardRarityTheme>();
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
}