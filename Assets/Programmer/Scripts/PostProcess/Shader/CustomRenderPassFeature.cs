using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static FullScreenPassRendererFeature;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    [SerializeField]
    [Header("レンダーパスのイベント")]
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    [SerializeField]
    [Header("ポストプロセスのデータベース")]
    public CSO_PostProcessStack postProcessStack;

    Dictionary<CSO_PostProcessStack.EffectEntry, CustomRenderPass> passCache = new();

    class CustomRenderPass : ScriptableRenderPass
    {
        Material passMaterial;
        int passIndex;
        bool passCopyActiveColor;
        bool passBindDepthStencilAttachment;
        private RTHandle passCopiedColor;
        private MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        public CustomRenderPass(string passName)
        {
            profilingSampler = new ProfilingSampler(passName);
        }

        public void SetupMembers(Material material, int index, bool copyActiveColor, bool bindDepthStencilAttachment, MaterialPropertyBlock block)
        {
            passMaterial = material;
            passIndex = index;
            passCopyActiveColor = copyActiveColor;
            passBindDepthStencilAttachment = bindDepthStencilAttachment;
            propertyBlock = block;
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

        private void ExecuteMainPass(CommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex)
        {
            if (sourceTexture != null)
                propertyBlock.SetTexture("_BlitTexture", sourceTexture);

            propertyBlock.SetVector("_BlitScaleBias", new Vector4(1, 1, 0, 0));

            cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, propertyBlock);
        }

        // ExecuteMainPassがインスタンスメソッドになったのでExecute側も修正
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

                // ✅ staticメソッドからインスタンスメソッドに変更
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

    public override void Create()
    {
        passCache.Clear();
    }

    private CustomRenderPass GetOrCreatePass(CSO_PostProcessStack.EffectEntry entry)
    {
        if (!passCache.TryGetValue(entry, out var pass))
        {
            pass = new CustomRenderPass(entry.material.name);
            pass.renderPassEvent = renderPassEvent;
            passCache[entry] = pass;
        }
        return pass;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData)
            || renderingData.cameraData.cameraType == CameraType.Preview
            || renderingData.cameraData.cameraType == CameraType.Reflection)
            return;

        if (postProcessStack == null)
        {
            Debug.LogWarningFormat("PostProcessStack が未設定です: {0}", name);
            return;
        }

        var stack = VolumeManager.instance.stack;

        foreach (var effect in postProcessStack.effects)
        {
            if (effect == null || effect.material == null) continue;
            if (!effect.IsActive(stack)) continue;

            // Volumeの値をPropertyBlockに書き込む
            effect.Apply(stack);

            var pass = GetOrCreatePass(effect);
            pass.renderPassEvent = renderPassEvent;
            pass.ConfigureInput(ScriptableRenderPassInput.None);
            pass.SetupMembers(effect.material, 0, true, false, effect.PropertyBlock);
            renderer.EnqueuePass(pass);
        }
    }
}
