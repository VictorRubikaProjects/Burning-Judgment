#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class ServiceLocatorDebugWindow : EditorWindow
{
    [MenuItem("Tools/Service Locator/Debug Window")]
    public static void Open() => GetWindow<ServiceLocatorDebugWindow>("Services");

    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Entre en Play Mode pour voir les services actifs.", MessageType.Info);
            return;
        }

        if (ServiceLocator.IsEmpty)
        {
            EditorGUILayout.HelpBox("No Services Registered",MessageType.Warning);
            return;
        }
            
        foreach (Type type in ServiceLocator.Services)
            EditorGUILayout.LabelField(type.Name);

        Repaint();
    }
}
#endif