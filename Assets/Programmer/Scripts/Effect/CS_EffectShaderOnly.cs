using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectShaderOnly.cs
 概要     : Shaderのみのエフェクトを制御するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

/// <summary>
/// ShaderOnly系Effectの共通処理です。
/// 自分自身についているRendererへShader値を渡します。
/// </summary>
public abstract class CS_EffectShaderOnly : CSAD_EffectCommonProcessBase
{
    /// <summary>
    /// Shaderに渡す再生時間のProperty IDです。
    /// </summary>
    private static readonly int int_EffectTimePropertyId = Shader.PropertyToID("_EffectTime");

    /// <summary>
    /// Shaderに渡す再生状態のProperty IDです。
    /// </summary>
    private static readonly int int_EffectPlayPropertyId = Shader.PropertyToID("_EffectPlay");

    /// <summary>
    /// Effectを表示するRendererです。
    /// </summary>
    protected Renderer rd_EffectRenderer;

    /// <summary>
    /// MaterialPropertyBlockです。
    /// </summary>
    protected MaterialPropertyBlock mpb_EffectMaterialPropertyBlock;

    /// <summary>
    /// 再生中かどうかです。
    /// </summary>
    protected bool bool_IsPlaying;

    /// <summary>
    /// 現在の再生時間です。
    /// </summary>
    protected float f_CurrentPlayTime;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    public override void InitEffect()
    {
        if (rd_EffectRenderer == null)
        {
            rd_EffectRenderer = GetComponent<Renderer>();
        }

        if (mpb_EffectMaterialPropertyBlock == null)
        {
            mpb_EffectMaterialPropertyBlock = new MaterialPropertyBlock();
        }
    }

    /// <summary>
    /// 更新処理です。
    /// </summary>
    protected virtual void Update()
    {
        if (bool_IsPlaying == false)
        {
            return;
        }

        f_CurrentPlayTime += Time.deltaTime;

        SetShaderFloat(int_EffectTimePropertyId, f_CurrentPlayTime);

        UpdateShaderOnlyEffect();
    }

    /// <summary>
    /// ShaderOnlyEffectの再生処理です。
    /// </summary>
    protected override void PlayEffectProcess()
    {
        if (rd_EffectRenderer == null)
        {
            Debug.LogWarning("[CS_EffectShaderOnly] Rendererがありません : " + gameObject.name);
            return;
        }

        bool_IsPlaying = true;
        f_CurrentPlayTime = 0.0f;

        SetShaderFloat(int_EffectPlayPropertyId, 1.0f);
        SetShaderFloat(int_EffectTimePropertyId, 0.0f);

        PlayShaderOnlyEffect();
    }

    /// <summary>
    /// ShaderOnlyEffectの終了処理です。
    /// </summary>
    protected override void EndEffectProcess()
    {
        bool_IsPlaying = false;

        SetShaderFloat(int_EffectPlayPropertyId, 0.0f);

        EndShaderOnlyEffect();
    }

    /// <summary>
    /// Shaderへfloat値を渡します。
    /// </summary>
    /// <param name="int_propertyId">Shader Property ID。</param>
    /// <param name="f_value">設定する値。</param>
    protected void SetShaderFloat(int int_propertyId, float f_value)
    {
        if (rd_EffectRenderer == null)
        {
            return;
        }

        if (mpb_EffectMaterialPropertyBlock == null)
        {
            mpb_EffectMaterialPropertyBlock = new MaterialPropertyBlock();
        }

        rd_EffectRenderer.GetPropertyBlock(mpb_EffectMaterialPropertyBlock);
        mpb_EffectMaterialPropertyBlock.SetFloat(int_propertyId, f_value);
        rd_EffectRenderer.SetPropertyBlock(mpb_EffectMaterialPropertyBlock);
    }

    /// <summary>
    /// ShaderOnlyEffect固有の再生開始処理です。
    /// 必要な場合だけ継承先で上書きします。
    /// </summary>
    protected virtual void PlayShaderOnlyEffect()
    {

    }

    /// <summary>
    /// ShaderOnlyEffect固有の更新処理です。
    /// 必要な場合だけ継承先で上書きします。
    /// </summary>
    protected virtual void UpdateShaderOnlyEffect()
    {

    }

    /// <summary>
    /// ShaderOnlyEffect固有の終了処理です。
    /// 必要な場合だけ継承先で上書きします。
    /// </summary>
    protected virtual void EndShaderOnlyEffect()
    {

    }
}
