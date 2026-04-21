using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostFeature : ScriptableRendererFeature
{
    public class CustomPass : ScriptableRenderPass
    {
        Material material;

        RTHandle source;
        RTHandle tempRT;

        public CustomPass(Material mat)
        {
            material = mat;
        }

        public void Setup(RTHandle src)
        {
            source = src;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;

            RenderingUtils.ReAllocateIfNeeded(
                ref tempRT,
                desc,
                name: "_TempRT"
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("CustomPost");

            // ① source → temp（加工）
            Blitter.BlitCameraTexture(cmd, source, tempRT, material, 0);

            // ② temp → source（戻す）
            Blitter.BlitCameraTexture(cmd, tempRT, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // RTHandleは自動管理されるので基本不要
        }
    }

    public Material material;
    CustomPass pass;

    public override void Create()
    {
        pass = new CustomPass(material);
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(pass);
    }
}
