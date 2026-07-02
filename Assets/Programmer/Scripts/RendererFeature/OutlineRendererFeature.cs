using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
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
        maskPass = new OutlineMaskPass(settings.maskMaterial)
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

// ========================================================
// Pass 1: OutlineTarget が持つ per-object マテリアルで描画
//   各 OutlineTarget は OnEnable 時に BaseMaskMaterial を
//   元にインスタンスを生成し、色・太さをそこにセットする。
//   cmd.DrawRenderer にそのインスタンスを渡すことで
//   SRPBatcher に関係なく確実に per-object の値が反映される。
// ========================================================
class OutlineMaskPass : ScriptableRenderPass
{
    static readonly List<CS_OutlineTarget> targets = new List<CS_OutlineTarget>();
    public static void Register(CS_OutlineTarget t) { if (t != null && !targets.Contains(t)) targets.Add(t); }
    public static void Unregister(CS_OutlineTarget t) => targets.Remove(t);

    // OutlineTarget が自身のインスタンスを作るために参照する
    public static Material BaseMaskMaterial { get; private set; }

    RTHandle maskHandle;
    public RTHandle MaskHandle => maskHandle;

    public OutlineMaskPass(Material mat)
    {
        BaseMaskMaterial = mat;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.colorFormat = RenderTextureFormat.ARGB32;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;
        RenderingUtils.ReAllocateIfNeeded(ref maskHandle, desc, name: "_OutlineMaskTex");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get("Outline Mask");
        CoreUtils.SetRenderTarget(cmd, maskHandle, ClearFlag.Color, Color.clear);

        foreach (var t in targets)
        {
            if (t == null) continue;
            var r = t.CachedRenderer;
            var mat = t.MaskMaterial;   // per-object インスタンス
            if (r == null || mat == null) continue;

            for (int i = 0 ; i < r.sharedMaterials.Length ; i++)
                cmd.DrawRenderer(r, mat, i, 0);  // ★ インスタンスを直接渡す
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose() => maskHandle?.Release();
}

// ========================================================
// Pass 2: マスクRT からエッジ検出してカメラバッファに合成
// ========================================================
class OutlineEdgePass : ScriptableRenderPass
{
    Material edgeMaterial;
    RTHandle maskHandle;

    static readonly int MaskTexId = Shader.PropertyToID("_MaskTex");
    static readonly int MaskTexelSizeId = Shader.PropertyToID("_MaskTex_TexelSize");

    public OutlineEdgePass(Material mat) => edgeMaterial = mat;
    public void Setup(RTHandle mask) => maskHandle = mask;

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (maskHandle == null || edgeMaterial == null) return;

        var cmd = CommandBufferPool.Get("Outline Edge");
        var cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        var rt = maskHandle.rt;

        edgeMaterial.SetTexture(MaskTexId, maskHandle);
        edgeMaterial.SetVector(MaskTexelSizeId,
            new Vector4(1f / rt.width, 1f / rt.height, rt.width, rt.height));

        cmd.SetRenderTarget(cameraTarget);
        cmd.DrawProcedural(Matrix4x4.identity, edgeMaterial, 0, MeshTopology.Triangles, 3);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
