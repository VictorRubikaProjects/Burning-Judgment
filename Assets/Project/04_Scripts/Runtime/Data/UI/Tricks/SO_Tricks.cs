using UnityEngine;

[CreateAssetMenu(fileName = "SO_Tricks", menuName = "Game/SO_Tricks")]
public class SO_Tricks : ScriptableObject
{
    [TextArea(3, 6)]
    [Tooltip("Liste des astuces affichées pendant le chargement.")]
    public string[] tricksList;
}