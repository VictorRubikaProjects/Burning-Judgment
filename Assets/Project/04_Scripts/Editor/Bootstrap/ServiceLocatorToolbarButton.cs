#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;

[EditorToolbarElement(id, typeof(SceneView))]
internal class ServiceLocatorToolbarButton : EditorToolbarButton
{
    public const string id = "BootStrap/ServiceLocatorButton";

    public ServiceLocatorToolbarButton()
    {
        text = "Services";
        clicked += ServiceLocatorDebugWindow.Open;
    }
}

[Overlay(typeof(SceneView), "Service Locator")]
class ServiceLocatorOverlay : ToolbarOverlay
{
    ServiceLocatorOverlay() : base(ServiceLocatorToolbarButton.id) { }
}
#endif