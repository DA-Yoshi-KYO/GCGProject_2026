using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WallDepth : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        FilteringSettings filtering;
        ShaderTagId shaderTag = new ShaderTagId("UniversalForward");

        public CustomRenderPass()
        {
            // 👇 Wallレイヤーだけ描画
            filtering = new FilteringSettings(RenderQueueRange.opaque, LayerMask.GetMask("Wall"));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var drawingSettings = CreateDrawingSettings(shaderTag, ref renderingData, SortingCriteria.CommonOpaque);

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filtering);
        }
    }

    CustomRenderPass pass;

    public override void Create()
    {
        pass = new CustomRenderPass();

        // 👇 ここ重要（Opaqueの前に描く）
        pass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
