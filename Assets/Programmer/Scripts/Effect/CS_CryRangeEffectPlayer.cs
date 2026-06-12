using System.Collections.Generic;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_CryRangeEffectPlayer.cs
 概要     : CryRange用のShaderOnlyEffectクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/11 新規作成
=====================================+
*/

/// <summary>
/// CryRange用のShaderOnlyEffectです。
/// 各MaterialのPrefab状態を保持し、その状態へ徐々に表示します。
/// </summary>
public class CS_CryRangeEffectPlayer : CS_EffectShaderOnly
{
    private static readonly int n_GlowPowerPropertyId = Shader.PropertyToID("_GlowPower");
    private static readonly int n_AlphaPropertyId = Shader.PropertyToID("_Alpha");

    [Header("終了時Glow最大値")]
    [SerializeField]
    private float f_EndMaxGlowPower = 10.0f;

    [Header("終了時間のうちGlowを最大まで上げる割合")]
    [SerializeField, Range(0.01f, 0.99f)]
    private float f_EndGlowUpRate = 0.2f;

    private class CS_CryRangeMaterialDefaultData
    {
        public Renderer rd_Renderer;
        public int n_MaterialIndex;
        public bool b_HasGlowPower;
        public bool b_HasAlpha;
        public float f_DefaultGlowPower;
        public float f_DefaultAlpha;
    }

    private Renderer[] rd_ChildRenderers;
    private MaterialPropertyBlock mpb_PropertyBlock;

    private readonly List<CS_CryRangeMaterialDefaultData> list_DefaultMaterialData =
        new List<CS_CryRangeMaterialDefaultData>();

    public override void InitEffect()
    {
        base.InitEffect();

        rd_ChildRenderers = GetComponentsInChildren<Renderer>(true);

        if (mpb_PropertyBlock == null)
        {
            mpb_PropertyBlock = new MaterialPropertyBlock();
        }

        CachePrefabShaderValues();
    }

    protected override void OnPlayPhaseStart()
    {
        ApplyPlayRate(0.0f);
    }

    protected override void UpdatePlayPhase(float f_NormalizedPlayTime)
    {
        ApplyPlayRate(f_NormalizedPlayTime);
    }

    protected override void OnEndPhaseStart()
    {
        ApplyPlayRate(1.0f);
    }

    protected override void UpdateEndPhase(float f_NormalizedEndTime)
    {
        if (f_NormalizedEndTime <= f_EndGlowUpRate)
        {
            float f_Rate = Mathf.Clamp01(
                f_NormalizedEndTime / f_EndGlowUpRate);

            ApplyEndGlowUpRate(f_Rate);
            return;
        }

        float f_DownRate = Mathf.Clamp01(
            (f_NormalizedEndTime - f_EndGlowUpRate) /
            (1.0f - f_EndGlowUpRate));

        ApplyEndFadeRate(f_DownRate);
    }

    protected override void OnEndPhaseComplete()
    {
        ApplyHidden();
    }

    /// <summary>
    /// PrefabのMaterial値をMaterialごとに保存します。
    /// </summary>
    private void CachePrefabShaderValues()
    {
        list_DefaultMaterialData.Clear();

        if (rd_ChildRenderers == null)
        {
            return;
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
                Material ma_Material = ma_SharedMaterials[j];

                if (ma_Material == null)
                {
                    continue;
                }

                bool b_HasGlowPower = ma_Material.HasProperty(n_GlowPowerPropertyId);
                bool b_HasAlpha = ma_Material.HasProperty(n_AlphaPropertyId);

                if (b_HasGlowPower == false && b_HasAlpha == false)
                {
                    continue;
                }

                CS_CryRangeMaterialDefaultData cs_DefaultData =
                    new CS_CryRangeMaterialDefaultData();

                cs_DefaultData.rd_Renderer = rd_Renderer;
                cs_DefaultData.n_MaterialIndex = j;
                cs_DefaultData.b_HasGlowPower = b_HasGlowPower;
                cs_DefaultData.b_HasAlpha = b_HasAlpha;
                cs_DefaultData.f_DefaultGlowPower =
                    b_HasGlowPower ? ma_Material.GetFloat(n_GlowPowerPropertyId) : 0.0f;
                cs_DefaultData.f_DefaultAlpha =
                    b_HasAlpha ? ma_Material.GetFloat(n_AlphaPropertyId) : 1.0f;

                list_DefaultMaterialData.Add(cs_DefaultData);
            }
        }
    }

    /// <summary>
    /// 再生時間をかけてPrefab状態へ近づけます。
    /// </summary>
    /// <param name="f_Rate">0から1の表示率。</param>
    private void ApplyPlayRate(float f_Rate)
    {
        f_Rate = Mathf.Clamp01(f_Rate);

        for (int i = 0 ; i < list_DefaultMaterialData.Count ; i++)
        {
            CS_CryRangeMaterialDefaultData cs_Data = list_DefaultMaterialData[i];

            float f_GlowPower = cs_Data.f_DefaultGlowPower;
            float f_Alpha = cs_Data.f_DefaultAlpha * f_Rate;

            ApplyMaterialValue(cs_Data, f_GlowPower, f_Alpha);
        }
    }

    /// <summary>
    /// 終了開始から2割でGlowPowerだけ最大まで上げます。
    /// AlphaはPrefab状態のままです。
    /// </summary>
    /// <param name="f_Rate">0から1の上昇率。</param>
    private void ApplyEndGlowUpRate(float f_Rate)
    {
        f_Rate = Mathf.Clamp01(f_Rate);

        for (int i = 0 ; i < list_DefaultMaterialData.Count ; i++)
        {
            CS_CryRangeMaterialDefaultData cs_Data = list_DefaultMaterialData[i];

            float f_GlowPower = Mathf.Lerp(
                cs_Data.f_DefaultGlowPower,
                f_EndMaxGlowPower,
                f_Rate);

            float f_Alpha = cs_Data.f_DefaultAlpha;

            ApplyMaterialValue(cs_Data, f_GlowPower, f_Alpha);
        }
    }

    /// <summary>
    /// 終了時間の残り8割でGlowPowerとAlphaを0にします。
    /// </summary>
    /// <param name="f_Rate">0から1の消失率。</param>
    private void ApplyEndFadeRate(float f_Rate)
    {
        f_Rate = Mathf.Clamp01(f_Rate);

        for (int i = 0 ; i < list_DefaultMaterialData.Count ; i++)
        {
            CS_CryRangeMaterialDefaultData cs_Data = list_DefaultMaterialData[i];

            float f_GlowPower = Mathf.Lerp(
                f_EndMaxGlowPower,
                0.0f,
                f_Rate);

            float f_Alpha = Mathf.Lerp(
                cs_Data.f_DefaultAlpha,
                0.0f,
                f_Rate);

            ApplyMaterialValue(cs_Data, f_GlowPower, f_Alpha);
        }
    }

    /// <summary>
    /// 完全非表示状態にします。
    /// </summary>
    private void ApplyHidden()
    {
        for (int i = 0 ; i < list_DefaultMaterialData.Count ; i++)
        {
            CS_CryRangeMaterialDefaultData cs_Data = list_DefaultMaterialData[i];

            ApplyMaterialValue(
                cs_Data,
                0.0f,
                0.0f);
        }
    }

    /// <summary>
    /// 指定Materialへ値を反映します。
    /// </summary>
    private void ApplyMaterialValue(
        CS_CryRangeMaterialDefaultData cs_Data,
        float f_GlowPower,
        float f_Alpha)
    {
        if (cs_Data == null)
        {
            return;
        }

        if (cs_Data.rd_Renderer == null)
        {
            return;
        }

        cs_Data.rd_Renderer.GetPropertyBlock(
            mpb_PropertyBlock,
            cs_Data.n_MaterialIndex);

        if (cs_Data.b_HasGlowPower)
        {
            mpb_PropertyBlock.SetFloat(
                n_GlowPowerPropertyId,
                f_GlowPower);
        }

        if (cs_Data.b_HasAlpha)
        {
            mpb_PropertyBlock.SetFloat(
                n_AlphaPropertyId,
                f_Alpha);
        }

        cs_Data.rd_Renderer.SetPropertyBlock(
            mpb_PropertyBlock,
            cs_Data.n_MaterialIndex);
    }
}
