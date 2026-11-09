using UnityEngine;

[DisallowMultipleComponent]
public class PlayerDebugView : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("Référence vers le PlayerCharacter. Si vide, tentera de le trouver sur le même GameObject.")]
    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        if (playerCharacter == null)
        {
            playerCharacter = GetComponent<PlayerCharacter>();
            if (playerCharacter == null)
            {
                Debug.LogError($"[PlayerDebugView] Aucun PlayerCharacter trouvé sur {gameObject.name} !", this);
                enabled = false;
            }
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (playerCharacter == null) return;

        Rect rect = new Rect(Screen.width - 400, 10, 200, 40);
        string text = playerCharacter.CurrentStateName;

        GUIStyle style = new GUIStyle
        {
            fontSize = 40,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUI.Box(rect, text, style);
    }
#endif
}