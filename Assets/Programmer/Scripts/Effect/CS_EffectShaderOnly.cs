using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectShaderOnly.cs
 概要     : Shaderのみのエフェクトを制御するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

public abstract class CS_EffectShaderOnly : CSAD_EffectCommonProcessBase
{
    /// <summary>
    /// Effectを表示するRendererです。
    /// </summary>
    [Header("Effectを表示するRenderer")]
    [SerializeField]
    protected Renderer rd_EffectRenderer;

    /// <summary>
    /// Shaderに渡す再生時間のProperty名です。
    /// </summary>
    [Header("Shader Property Name")]
    [SerializeField]
    protected string str_EffectTimePropertyName = "_EffectTime";

    /// <summary>
    /// Shaderに渡す再生状態のProperty名です。
    /// </summary>
    [SerializeField]
    protected string str_EffectPlayPropertyName = "_EffectPlay";

    /// <summary>
    /// 再生中かどうかです。
    /// </summary>
    protected bool b_IsPlaying;

    /// <summary>
    /// 現在の再生時間です。
    /// </summary>
    protected float f_CurrentPlayTime;

    /// <summary>
    /// MaterialPropertyBlockです。
    /// </summary>
    protected MaterialPropertyBlock mpb_EffectMaterialPropertyBlock;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitEffect()
    {
        if (rd_EffectRenderer == null)
        {
            rd_EffectRenderer = GetComponentInChildren<Renderer>(true);
        }

        if (mpb_EffectMaterialPropertyBlock == null)
        {
            mpb_EffectMaterialPropertyBlock = new MaterialPropertyBlock();
        }
    }

    /// <summary>
    /// 毎フレーム更新処理
    /// </summary>
    protected virtual void Update()
    {
        if (b_IsPlaying == false)
        {
            return;
        }

        f_CurrentPlayTime += Time.deltaTime;

        SetShaderFloat(str_EffectTimePropertyName, f_CurrentPlayTime);

        UpdateShaderOnlyEffect();
    }

    /// <summary>
    /// ShaderOnlyEffectの再生処理です。
    /// </summary>
    protected override void PlayEffectProcess()
    {
        b_IsPlaying = true;
        f_CurrentPlayTime = 0.0f;

        SetShaderFloat(str_EffectPlayPropertyName, 1.0f);
        SetShaderFloat(str_EffectTimePropertyName, 0.0f);

        PlayShaderOnlyEffect();
    }

    /// <summary>
    /// ShaderOnlyEffectの終了処理です。
    /// </summary>
    protected override void EndEffectProcess()
    {
        b_IsPlaying = false;

        SetShaderFloat(str_EffectPlayPropertyName, 0.0f);

        EndShaderOnlyEffect();
    }

    /// <summary>
    /// Shaderへfloat値を渡します。
    /// </summary>
    /// <param name="str_propertyName">Shader Property名。</param>
    /// <param name="f_value">設定する値。</param>
    protected void SetShaderFloat(string str_propertyName, float f_value)
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
        mpb_EffectMaterialPropertyBlock.SetFloat(str_propertyName, f_value);
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
