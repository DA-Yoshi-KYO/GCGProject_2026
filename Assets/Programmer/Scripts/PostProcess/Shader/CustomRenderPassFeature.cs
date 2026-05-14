using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static FullScreenPassRendererFeature;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    [SerializeField][Header("レンダーパスのイベント")] public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;   // レンダーパスのイベント
    [SerializeField][Header("使用するポストプロセスマテリアル")] public Material material = null;   // 使用するポストプロセスマテリアル

    class CustomRenderPass : ScriptableRenderPass
    {
        Material passMaterial;
        int passIndex;
        bool passCopyActiveColor;
        bool passBindDepthStencilAttachment;
        private RTHandle passCopiedColor;
        private static MaterialPropertyBlock sharedPropertyBlock = new MaterialPropertyBlock();

        public CustomRenderPass(string passName)
        {
            profilingSampler = new ProfilingSampler(passName);
        }

        public void SetupMembers(Material material, int index, bool copyActiveColor, bool bindDepthStencilAttachment)
        {
            passMaterial = material;
            passIndex = index;
            passCopyActiveColor = copyActiveColor;
            passBindDepthStencilAttachment = bindDepthStencilAttachment;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ResetTarget();

            if (passCopyActiveColor)
                ReAllocate(renderingData.cameraData.cameraTargetDescriptor);
        }

        internal void ReAllocate(RenderTextureDescriptor desc)
        {
            desc.msaaSamples = 1;
            desc.depthBufferBits = (int)DepthBits.None;
            RenderingUtils.ReAllocateIfNeeded(ref passCopiedColor, desc, name: "_FullscreenPassColorCopy");
        }

        public void Dispose()
        {
            passCopiedColor?.Release();
        }

        private static void ExecuteCopyColorPass(CommandBuffer cmd, RTHandle sourceTexture)
        {
            Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
        }

        private static void ExecuteMainPass(CommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex)
        {
            sharedPropertyBlock.Clear();
            if (sourceTexture != null)
                sharedPropertyBlock.SetTexture("_BlitTexture", sourceTexture);

            // We need to set the "_BlitScaleBias" uniform for user materials with shaders relying on core Blit.hlsl to work
            sharedPropertyBlock.SetVector("_BlitScaleBias", new Vector4(1, 1, 0, 0));

            cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, sharedPropertyBlock);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, profilingSampler))
            {
                if (passCopyActiveColor)
                {
                    CoreUtils.SetRenderTarget(cmd, passCopiedColor);
                    ExecuteCopyColorPass(cmd, cameraData.renderer.cameraColorTargetHandle);
                }

                if (passBindDepthStencilAttachment)
                    CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle, cameraData.renderer.cameraDepthTargetHandle);
                else
                    CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle);

                ExecuteMainPass(cmd, passCopyActiveColor ? passCopiedColor : null, passMaterial, passIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass scriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        scriptablePass = new CustomRenderPass(name);

        // Configures where the render pass should be injected.
        scriptablePass.renderPassEvent = renderPassEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData) || renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection)
            return;

        if (material == null)
        {
            Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - no material is assigned. Please make sure a material is assigned for this feature on the renderer asset.", name);
            return;
        }

        if (0 >= material.passCount)
        {
            Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - the pass index is out of bounds for the material.", name);
            return;
        }

        scriptablePass.renderPassEvent = renderPassEvent;
        scriptablePass.ConfigureInput(ScriptableRenderPassInput.None);
        scriptablePass.SetupMembers(material, 0, true, false);

        renderer.EnqueuePass(scriptablePass);
    }
}


