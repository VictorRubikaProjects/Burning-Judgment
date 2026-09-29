using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class RequestNormalsFeature : ScriptableRendererFeature
{
    class RequestPass : ScriptableRenderPass
    {
        public RequestPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) { }
    }

    RequestPass pass;

    public override void Create() => pass = new RequestPass();

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        => renderer.EnqueuePass(pass);
}