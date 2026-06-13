using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public Material outlineMaterial = null;
        public LayerMask layerMask = -1;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public OutlineSettings settings = new OutlineSettings();
    OutlinePass outlinePass;

    public override void Create()
    {
        outlinePass = new OutlinePass(settings);
        outlinePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null) return;
        renderer.EnqueuePass(outlinePass);
    }

    class OutlinePass : ScriptableRenderPass
    {
        OutlineSettings settings;
        FilteringSettings filteringSettings;

        static readonly ShaderTagId[] shaderTagIds = new[]
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
        };

        public OutlinePass(OutlineSettings settings)
        {
            this.settings = settings;
            filteringSettings = new FilteringSettings(RenderQueueRange.all, settings.layerMask);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.outlineMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("Outline");
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            foreach (var tagId in shaderTagIds)
            {
                var drawingSettings = CreateDrawingSettings(
                    tagId,
                    ref renderingData,
                    SortingCriteria.CommonOpaque
                );
                drawingSettings.overrideMaterial = settings.outlineMaterial;
                drawingSettings.overrideMaterialPassIndex = 0;
                // MaterialPropertyBlock を有効にする（per-instanceの色・太さを受け取る）
                drawingSettings.perObjectData = PerObjectData.None;
                drawingSettings.enableInstancing = true;

                context.DrawRenderers(
                    renderingData.cullResults,
                    ref drawingSettings,
                    ref filteringSettings
                );
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
