using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
   [SerializeField] private RectTransform pivotJoystick;
   
   public RectTransform PivotJoystick => pivotJoystick;
}
