using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectWarpShaderOnly.cs
 概要     : Warp専用のShaderOnlyEffect制御クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/13 新規作成
            2026/07/13 CS側からEffectカラーを変更できるように修正
=====================================+
*/

/// <summary>
/// Warp専用のShaderOnlyEffect制御クラスです。
/// 子Renderer全体のBlackCutとEffectカラーを制御します。
/// </summary>
public class CS_EffectWarpShaderOnly : CS_EffectShaderOnly
{
    /// <summary>
    /// BlackCutのShader Property IDです。
    /// </summary>
    private static readonly int int_BlackCutPropertyId =
        Shader.PropertyToID("_BlackCut");

    /// <summary>
    /// 各Shaderで使用されるColor Property IDです。
    /// </summary>
    private static readonly int int_MainColorPropertyId =
        Shader.PropertyToID("_MainColor");

    private static readonly int int_SweepColorPropertyId =
        Shader.PropertyToID("_SweepColor");

    private static readonly int int_BaseColorPropertyId =
        Shader.PropertyToID("_BaseColor");

    private static readonly int int_LightningColorPropertyId =
        Shader.PropertyToID("_LightningColor");

    private static readonly int int_InnerColorPropertyId =
        Shader.PropertyToID("_InnerColor");

    private static readonly int int_OuterColorPropertyId =
        Shader.PropertyToID("_OuterColor");

    private static readonly int int_ColorAPropertyId =
        Shader.PropertyToID("_ColorA");

    private static readonly int int_ColorBPropertyId =
        Shader.PropertyToID("_ColorB");

    private static readonly int int_TintColorPropertyId =
        Shader.PropertyToID("_TintColor");

    private static readonly int int_ColorPropertyId =
        Shader.PropertyToID("_Color");

    [Header("Effectカラー")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color c_EffectColor = Color.cyan;

    /// <summary>
    /// Warp配下のRenderer一覧です。
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

        CacheChildRenderers();

        EnsureMaterialPropertyBlock();

        ApplyEffectColorToChildren();
    }

    /// <summary>
    /// Inspector上で値が変更された時に、
    /// Edit中でも色を確認できるようにします。
    /// </summary>
    private void OnValidate()
    {
        CacheChildRenderers();

        EnsureMaterialPropertyBlock();

        ApplyEffectColorToChildren();
    }

    /// <summary>
    /// 生成フェーズ開始時の処理です。
    /// </summary>
    protected override void OnPlayPhaseStart()
    {
        ApplyEffectColorToChildren();

        ApplyBlackCutToChildren(1.0f);
    }

    /// <summary>
    /// 生成フェーズ中の更新処理です。
    /// BlackCutを1から0へ補間します。
    /// </summary>
    /// <param name="f_NormalizedPlayTime">生成進行度。</param>
    protected override void UpdatePlayPhase(
        float f_NormalizedPlayTime)
    {
        float f_BlackCut = Mathf.Lerp(
            1.0f,
            0.0f,
            f_NormalizedPlayTime);

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
    /// BlackCutを0から1へ補間します。
    /// </summary>
    /// <param name="f_NormalizedEndTime">終了進行度。</param>
    protected override void UpdateEndPhase(
        float f_NormalizedEndTime)
    {
        float f_BlackCut = Mathf.Lerp(
            0.0f,
            1.0f,
            f_NormalizedEndTime);

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
    /// 外部のCSからEffectカラーを変更します。
    /// </summary>
    /// <param name="c_Color">設定するEffectカラー。</param>
    public void SetEffectColor(Color c_Color)
    {
        c_EffectColor = c_Color;

        // PlayEffect（InitEffect）が一度も呼ばれていない状態でも
        // 色変更だけは反映できるように、ここでもキャッシュを保証する
        CacheChildRenderers();

        EnsureMaterialPropertyBlock();

        ApplyEffectColorToChildren();
    }

    /// <summary>
    /// 子Rendererを取得します。
    /// </summary>
    private void CacheChildRenderers()
    {
        if (rd_ChildRenderers != null &&
            rd_ChildRenderers.Length > 0)
        {
            return;
        }

        rd_ChildRenderers =
            GetComponentsInChildren<Renderer>(true);
    }

    /// <summary>
    /// MaterialPropertyBlockを用意します。
    /// </summary>
    private void EnsureMaterialPropertyBlock()
    {
        if (mpb_ChildPropertyBlock != null)
        {
            return;
        }

        mpb_ChildPropertyBlock =
            new MaterialPropertyBlock();
    }

    /// <summary>
    /// 子Renderer全体へEffectカラーを反映します。
    /// Material側に存在するColor Propertyだけを変更します。
    /// </summary>
    private void ApplyEffectColorToChildren()
    {
        if (rd_ChildRenderers == null)
        {
            return;
        }

        EnsureMaterialPropertyBlock();

        for (int i = 0 ; i < rd_ChildRenderers.Length ; i++)
        {
            Renderer rd_Renderer = rd_ChildRenderers[i];

            if (rd_Renderer == null)
            {
                continue;
            }

            Material[] ma_SharedMaterials =
                rd_Renderer.sharedMaterials;

            for (int j = 0 ; j < ma_SharedMaterials.Length ; j++)
            {
                Material ma_SharedMaterial =
                    ma_SharedMaterials[j];

                if (ma_SharedMaterial == null)
                {
                    continue;
                }

                mpb_ChildPropertyBlock.Clear();

                rd_Renderer.GetPropertyBlock(
                    mpb_ChildPropertyBlock,
                    j);

                bool b_IsColorChanged = false;

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_MainColorPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_BaseColorPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_LightningColorPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_SweepColorPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_InnerColorPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_OuterColorPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_ColorAPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_ColorBPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_TintColorPropertyId);

                b_IsColorChanged |= SetColorIfPropertyExists(
                    ma_SharedMaterial,
                    int_ColorPropertyId);

                if (b_IsColorChanged == false)
                {
                    continue;
                }

                rd_Renderer.SetPropertyBlock(
                    mpb_ChildPropertyBlock,
                    j);
            }
        }
    }

    /// <summary>
    /// Materialに指定Color Propertyがある場合だけ色を設定します。
    /// </summary>
    /// <param name="ma_Material">確認するMaterial。</param>
    /// <param name="int_PropertyId">Color Property ID。</param>
    /// <returns>色を設定した場合はtrue。</returns>
    private bool SetColorIfPropertyExists(
        Material ma_Material,
        int int_PropertyId)
    {
        if (ma_Material.HasProperty(int_PropertyId) == false)
        {
            return false;
        }

        mpb_ChildPropertyBlock.SetColor(
            int_PropertyId,
            c_EffectColor);

        return true;
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

        EnsureMaterialPropertyBlock();

        for (int i = 0 ; i < rd_ChildRenderers.Length ; i++)
        {
            Renderer rd_Renderer = rd_ChildRenderers[i];

            if (rd_Renderer == null)
            {
                continue;
            }

            Material[] ma_SharedMaterials =
                rd_Renderer.sharedMaterials;

            for (int j = 0 ; j < ma_SharedMaterials.Length ; j++)
            {
                Material ma_SharedMaterial =
                    ma_SharedMaterials[j];

                if (ma_SharedMaterial == null)
                {
                    continue;
                }

                if (ma_SharedMaterial.HasProperty(
                    int_BlackCutPropertyId) == false)
                {
                    continue;
                }

                mpb_ChildPropertyBlock.Clear();

                rd_Renderer.GetPropertyBlock(
                    mpb_ChildPropertyBlock,
                    j);

                mpb_ChildPropertyBlock.SetFloat(
                    int_BlackCutPropertyId,
                    f_BlackCut);

                rd_Renderer.SetPropertyBlock(
                    mpb_ChildPropertyBlock,
                    j);
            }
        }
    }
}
