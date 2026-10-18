using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Card", fileName = "SO_Card")]
public class SO_Card : ScriptableObject
{
    [field: Header("Visuals")]
    [field: SerializeField]
    [field: Tooltip("Nom de la carte affiché au joueur.")]
    public string DisplayName { get; private set; }

    [field: SerializeField, TextArea]
    [field: Tooltip("Texte qui décrit l'effet de la carte.")]
    public string Description { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Icône affichée sur la carte.")]
    public Sprite Icon { get; private set; }

    [field: Space, Header("Augment")]
    [field: SerializeField]
    [field: Tooltip("Si coché, le joueur peut prendre cette carte plusieurs fois et les effets s'additionnent. Sinon, elle n'est plus proposée une fois prise.")]
    public bool IsStackable { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Variations de stats appliquées au joueur. Seules les valeurs différentes de 0 ont un effet. Positif = bon pour le joueur, négatif = malus.")]
    public AugmentDeltas Deltas { get; private set; }
}