using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP Renderer Feature によるアウトライン描画
/// 
/// 設定手順:
///   1. このファイルを Assets/Scripts/ などに配置
///   2. Project > Assets > [任意] > UniversalRenderPipelineAsset_Renderer を選択
///      または Edit > Project Settings > Graphics > Scriptable Render Pipeline Settings
///      から使用中の Renderer を開く
///   3. Inspector の "Add Renderer Feature" → "Outline Renderer Feature" を追加
///   4. Outline Material に後述の Outline.shader を使ったマテリアルをセット
///   5. Layer Mask でアウトラインを出したいレイヤーを指定
/// </summary>
public class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public Material outlineMaterial = null;
        public LayerMask layerMask = -1; // デフォルト: 全レイヤー
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

    // ─────────────────────────────────────────────────────────────
    class OutlinePass : ScriptableRenderPass
    {
        OutlineSettings settings;
        FilteringSettings filteringSettings;
        ShaderTagId shaderTagId;

        public OutlinePass(OutlineSettings settings)
        {
            this.settings = settings;
            // アウトライン専用のPassName ("Outline") を描画対象にする
            shaderTagId = new ShaderTagId("Outline");
            filteringSettings = new FilteringSettings(RenderQueueRange.all, settings.layerMask);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.outlineMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("Outline");

            // 描画設定：全オブジェクトをアウトラインマテリアルで上書き描画
            var drawingSettings = CreateDrawingSettings(
                new ShaderTagId("SRPDefaultUnlit"),
                ref renderingData,
                SortingCriteria.CommonOpaque
            );
            // アウトラインマテリアルで全サブメッシュを上書き
            drawingSettings.overrideMaterial         = settings.outlineMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;

            context.DrawRenderers(
                renderingData.cullResults,
                ref drawingSettings,
                ref filteringSettings
            );

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
