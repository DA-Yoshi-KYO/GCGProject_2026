using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectMagicCircleShaderOnly.cs
 概要     : 魔法陣専用のShaderOnlyEffect制御クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

/// <summary>
/// 魔法陣専用のShaderOnlyEffect制御クラスです。
/// 子Renderer全体の BlackCut を制御します。
/// </summary>
public class CS_EffectMagicCircleShaderOnly : CS_EffectShaderOnly
{
    /// <summary>
    /// BlackCutのShader Property IDです。
    /// </summary>
    private static readonly int int_BlackCutPropertyId = Shader.PropertyToID("_BlackCut");

    /// <summary>
    /// 魔法陣配下のRenderer一覧です。
    /// </summary>
    private Renderer[] rd_ChildRenderers;

    /// <summary>
    /// 子Renderer用のMaterialPropertyBlockです。
    /// </summary>
    private MaterialPropertyBlock mpb_ChildPropertyBlock;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    public override void InitEffect()
    {
        base.InitEffect();

        if (rd_ChildRenderers == null || rd_ChildRenderers.Length == 0)
        {
            rd_ChildRenderers = GetComponentsInChildren<Renderer>(true);
        }

        if (mpb_ChildPropertyBlock == null)
        {
            mpb_ChildPropertyBlock = new MaterialPropertyBlock();
        }
    }

    /// <summary>
    /// 生成フェーズ開始時の処理です。
    /// </summary>
    protected override void OnPlayPhaseStart()
    {
        ApplyBlackCutToChildren(1.0f);
    }

    /// <summary>
    /// 生成フェーズ中の更新処理です。
    /// 0 → 1 に補間します。
    /// </summary>
    /// <param name="f_NormalizedPlayTime">生成進行度。</param>
    protected override void UpdatePlayPhase(float f_NormalizedPlayTime)
    {
        float f_BlackCut = Mathf.Lerp(1.0f, 0.0f, f_NormalizedPlayTime);
        ApplyBlackCutToChildren(f_BlackCut);
    }

    /// <summary>
    /// 終了フェーズ開始時の処理です。
    /// </summary>
    protected override void OnEndPhaseStart()
    {
        ApplyBlackCutToChildren(0.0f);
    }

    /// <summary>
    /// 終了フェーズ中の更新処理です。
    /// 1 → 0 に補間します。
    /// </summary>
    /// <param name="f_NormalizedEndTime">終了進行度。</param>
    protected override void UpdateEndPhase(float f_NormalizedEndTime)
    {
        float f_BlackCut = Mathf.Lerp(0.0f, 1.0f, f_NormalizedEndTime);
        ApplyBlackCutToChildren(f_BlackCut);
    }

    /// <summary>
    /// 終了フェーズ完了時の処理です。
    /// </summary>
    protected override void OnEndPhaseComplete()
    {
        ApplyBlackCutToChildren(1.0f);
    }

    /// <summary>
    /// 子Renderer全体へBlackCutを適用します。
    /// </summary>
    /// <param name="f_BlackCut">設定するBlackCut値。</param>
    private void ApplyBlackCutToChildren(float f_BlackCut)
    {
        if (rd_ChildRenderers == null)
        {
            return;
        }

        if (mpb_ChildPropertyBlock == null)
        {
            mpb_ChildPropertyBlock = new MaterialPropertyBlock();
        }

        for (int i = 0 ; i < rd_ChildRenderers.Length ; i++)
        {
            Renderer rd_Renderer = rd_ChildRenderers[i];

            if (rd_Renderer == null)
            {
                continue;
            }

            Material[] ma_SharedMaterials = rd_Renderer.sharedMaterials;

            for (int j = 0 ; j < ma_SharedMaterials.Length ; j++)
            {
                Material ma_SharedMaterial = ma_SharedMaterials[j];

                if (ma_SharedMaterial == null)
                {
                    continue;
                }

                if (ma_SharedMaterial.HasProperty(int_BlackCutPropertyId) == false)
                {
                    continue;
                }

                rd_Renderer.GetPropertyBlock(mpb_ChildPropertyBlock, j);
                mpb_ChildPropertyBlock.SetFloat(int_BlackCutPropertyId, f_BlackCut);
                rd_Renderer.SetPropertyBlock(mpb_ChildPropertyBlock, j);
            }
        }
    }
}
