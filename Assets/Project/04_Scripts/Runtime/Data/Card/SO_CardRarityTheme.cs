using UnityEngine;

[CreateAssetMenu(menuName = "Config/Card/Card Rarity Theme", fileName = "SO_CardRarityTheme")]
public class SO_CardRarityTheme : ScriptableObject
{
    [field: SerializeField] public CardRarity Rarity { get; private set; }
    
    [field: Header("Colors")]
    [field: SerializeField] public Color TitleTextColor { get; private set; } = Color.white;
    [field: SerializeField] public Color DescriptionTextColor { get; private set; } = new Color(0.8f, 0.8f, 0.8f);
    
    [field: Header("Sprites & Textures")]
    [field: SerializeField] public Sprite BackgroundSprite { get; private set; }
    [field: SerializeField] public Sprite ForegroundSprite { get; private set; }
}