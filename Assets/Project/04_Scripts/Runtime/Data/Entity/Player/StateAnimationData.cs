using UnityEngine;

[System.Serializable]
public struct StateAnimationData
{
    [Range(0f, 0.9f)]
    [Tooltip("Durée de la transition vers cette animation, en secondes. Plus c'est court, plus le changement est sec.")]
    public float CrossFadeDuration;

    [Range(0.1f, 10f)]
    [Tooltip("Vitesse de lecture de l'animation. 1 = vitesse normale, 2 = deux fois plus rapide.")]
    public float Speed;
}