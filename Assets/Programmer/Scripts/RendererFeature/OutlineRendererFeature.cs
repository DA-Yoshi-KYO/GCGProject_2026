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
        desc.depthBufferBits = 0;   // 深度はカメラの深度バッファを共有するのでここでは持たない
        // desc.msaaSamples はカメラ設定のまま変更しない。
        // ここを 1 に固定すると、Game View で URP Asset の MSAA が有効な場合に
        // カラー(maskHandle)と深度(cameraDepthTarget)のサンプル数が食い違い、
        // 画面が白く壊れる不具合が発生する。
        RenderingUtils.ReAllocateIfNeeded(ref maskHandle, desc, name: "_OutlineMaskTex");

        // カメラの深度バッファ(cameraDepthTargetHandle)を読み書き対象として使うことを明示
        ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // OnCameraSetup がまだ呼ばれていない/RTが無効なケースへの対処
        if (maskHandle == null || maskHandle.rt == null) return;

        var cmd = CommandBufferPool.Get("Outline Mask");

        // カラーは専用RT(maskHandle)、深度は「既存のシーン深度バッファ」を使う。
        // ZWrite Off なので深度は書き換わらず、ZTest だけが機能する。
        // これで xRayOutline=false のとき正しく遮蔽物越しは隠れるようになる。
        var cameraDepthTarget = renderingData.cameraData.renderer.cameraDepthTargetHandle;
        CoreUtils.SetRenderTarget(cmd, maskHandle, cameraDepthTarget, ClearFlag.Color, Color.clear);

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
        // maskHandle.rt が null/破棄済みのケース(Scene View等の別カメラ実行順やウィンドウ未表示時)への対処
        if (maskHandle == null || maskHandle.rt == null || edgeMaterial == null) return;

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
