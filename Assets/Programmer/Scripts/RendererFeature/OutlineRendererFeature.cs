using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public LayerMask outlineLayer;
        public Material maskMaterial;
        public Material edgeMaterial;
        public RenderPassEvent maskEvent = RenderPassEvent.AfterRenderingOpaques;
        public RenderPassEvent edgeEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Settings settings = new Settings();
    OutlineMaskPass maskPass;
    OutlineEdgePass edgePass;

    public override void Create()
    {
        maskPass = new OutlineMaskPass(settings.maskMaterial, settings.outlineLayer)
        { renderPassEvent = settings.maskEvent };
        edgePass = new OutlineEdgePass(settings.edgeMaterial)
        { renderPassEvent = settings.edgeEvent };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.maskMaterial == null || settings.edgeMaterial == null) return;

        renderer.EnqueuePass(maskPass);
        edgePass.Setup(maskPass.MaskHandle);
        renderer.EnqueuePass(edgePass);
    }

    protected override void Dispose(bool disposing) => maskPass?.Dispose();
}

class OutlineMaskPass : ScriptableRenderPass
{
    static readonly ShaderTagId shaderTagId = new ShaderTagId("OutlineMask");
    Material overrideMaterial;
    FilteringSettings filteringSettings;
    RTHandle maskHandle;

    public RTHandle MaskHandle => maskHandle;

    public OutlineMaskPass(Material mat, LayerMask layer)
    {
        overrideMaterial = mat;
        filteringSettings = new FilteringSettings(RenderQueueRange.all, layer);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.colorFormat = RenderTextureFormat.R8;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;
        RenderingUtils.ReAllocateIfNeeded(ref maskHandle, desc, name: "_OutlineMaskTex");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get("Outline Mask");
        CoreUtils.SetRenderTarget(cmd, maskHandle, ClearFlag.Color, Color.clear);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        var drawSettings = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.CommonOpaque);
        drawSettings.overrideMaterial = overrideMaterial;
        drawSettings.overrideMaterialPassIndex = 0;

        context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose() => maskHandle?.Release();
}

class OutlineEdgePass : ScriptableRenderPass
{
    Material edgeMaterial;
    RTHandle maskHandle;
    static readonly int MaskTexId = Shader.PropertyToID("_MaskTex");

    public OutlineEdgePass(Material mat) => edgeMaterial = mat;
    public void Setup(RTHandle mask) => maskHandle = mask;

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (maskHandle == null || edgeMaterial == null) return;

        var cmd = CommandBufferPool.Get("Outline Edge");
        var cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

        edgeMaterial.SetTexture(MaskTexId, maskHandle);
        cmd.SetRenderTarget(cameraTarget);
        cmd.DrawProcedural(Matrix4x4.identity, edgeMaterial, 0, MeshTopology.Triangles, 3);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
