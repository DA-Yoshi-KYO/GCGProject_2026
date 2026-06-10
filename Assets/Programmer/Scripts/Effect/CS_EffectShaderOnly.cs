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
/// 生成フェーズ、待機フェーズ、終了フェーズを管理します。
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
    /// 自分自身のRendererです。
    /// </summary>
    protected Renderer rd_EffectRenderer;

    /// <summary>
    /// 自分自身のMaterialPropertyBlockです。
    /// </summary>
    protected MaterialPropertyBlock mpb_EffectMaterialPropertyBlock;

    /// <summary>
    /// 再生中かどうかです。
    /// </summary>
    protected bool bool_IsPlaying;

    /// <summary>
    /// 終了フェーズ中かどうかです。
    /// </summary>
    private bool bool_IsEnding;

    /// <summary>
    /// 生成フェーズが完了したかどうかです。
    /// </summary>
    private bool bool_IsPlayPhaseCompleted;

    /// <summary>
    /// 全体の再生経過時間です。
    /// </summary>
    protected float f_CurrentEffectTime;

    /// <summary>
    /// 終了フェーズの経過時間です。
    /// </summary>
    private float f_CurrentEndTime;

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

        f_CurrentEffectTime += Time.deltaTime;

        SetShaderFloatToSelf(int_EffectTimePropertyId, f_CurrentEffectTime);

        UpdatePlayPhaseProcess();

        UpdateEndPhaseProcess();

        UpdateShaderOnlyEffect();
    }

    /// <summary>
    /// ShaderOnlyEffectの再生処理です。
    /// </summary>
    protected override void PlayEffectProcess()
    {
        bool_IsPlaying = true;
        bool_IsEnding = false;
        bool_IsPlayPhaseCompleted = false;

        f_CurrentEffectTime = 0.0f;
        f_CurrentEndTime = 0.0f;

        SetShaderFloatToSelf(int_EffectPlayPropertyId, 1.0f);
        SetShaderFloatToSelf(int_EffectTimePropertyId, 0.0f);

        OnPlayPhaseStart();
        UpdatePlayPhase(0.0f);

        if (csst_EffectPlayData.f_PlayTime.HasValue == false ||
            csst_EffectPlayData.f_PlayTime.Value <= 0.0f)
        {
            bool_IsPlayPhaseCompleted = true;
            UpdatePlayPhase(1.0f);
            OnPlayPhaseComplete();
        }
    }

    /// <summary>
    /// 生成フェーズを更新します。
    /// </summary>
    private void UpdatePlayPhaseProcess()
    {
        if (bool_IsPlayPhaseCompleted)
        {
            return;
        }

        if (bool_IsEnding)
        {
            return;
        }

        if (csst_EffectPlayData.f_PlayTime.HasValue == false ||
            csst_EffectPlayData.f_PlayTime.Value <= 0.0f)
        {
            bool_IsPlayPhaseCompleted = true;
            UpdatePlayPhase(1.0f);
            OnPlayPhaseComplete();
            return;
        }

        float f_NormalizedPlayTime =
            Mathf.Clamp01(f_CurrentEffectTime / csst_EffectPlayData.f_PlayTime.Value);

        UpdatePlayPhase(f_NormalizedPlayTime);

        if (f_NormalizedPlayTime >= 1.0f)
        {
            bool_IsPlayPhaseCompleted = true;
            OnPlayPhaseComplete();
        }
    }

    /// <summary>
    /// 終了開始処理です。
    /// </summary>
    protected override void EndEffectProcess()
    {
        if (bool_IsEnding)
        {
            return;
        }

        bool_IsEnding = true;
        f_CurrentEndTime = 0.0f;

        OnEndPhaseStart();
        UpdateEndPhase(0.0f);

        if (csst_EffectPlayData.f_EndTime.HasValue == false ||
            csst_EffectPlayData.f_EndTime.Value <= 0.0f)
        {
            UpdateEndPhase(1.0f);
            OnEndPhaseComplete();

            bool_IsEnding = false;
            bool_IsPlaying = false;

            SetShaderFloatToSelf(int_EffectPlayPropertyId, 0.0f);

            FinishEndEffect();
        }
    }

    /// <summary>
    /// 終了フェーズを更新します。
    /// </summary>
    private void UpdateEndPhaseProcess()
    {
        if (bool_IsEnding == false)
        {
            return;
        }

        if (csst_EffectPlayData.f_EndTime.HasValue == false ||
            csst_EffectPlayData.f_EndTime.Value <= 0.0f)
        {
            return;
        }

        f_CurrentEndTime += Time.deltaTime;

        float f_NormalizedEndTime =
            Mathf.Clamp01(f_CurrentEndTime / csst_EffectPlayData.f_EndTime.Value);

        UpdateEndPhase(f_NormalizedEndTime);

        if (f_NormalizedEndTime >= 1.0f)
        {
            OnEndPhaseComplete();

            bool_IsEnding = false;
            bool_IsPlaying = false;

            SetShaderFloatToSelf(int_EffectPlayPropertyId, 0.0f);

            FinishEndEffect();
        }
    }

    /// <summary>
    /// 自分自身のShaderへfloat値を渡します。
    /// </summary>
    /// <param name="int_propertyId">Shader Property ID。</param>
    /// <param name="f_value">設定する値。</param>
    protected void SetShaderFloatToSelf(int int_propertyId, float f_value)
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
    /// 生成フェーズ開始時の処理です。
    /// </summary>
    protected virtual void OnPlayPhaseStart()
    {

    }

    /// <summary>
    /// 生成フェーズ中の更新処理です。
    /// </summary>
    /// <param name="f_NormalizedPlayTime">0.0～1.0の生成進行度。</param>
    protected virtual void UpdatePlayPhase(float f_NormalizedPlayTime)
    {

    }

    /// <summary>
    /// 生成フェーズ完了時の処理です。
    /// </summary>
    protected virtual void OnPlayPhaseComplete()
    {

    }

    /// <summary>
    /// 終了フェーズ開始時の処理です。
    /// </summary>
    protected virtual void OnEndPhaseStart()
    {

    }

    /// <summary>
    /// 終了フェーズ中の更新処理です。
    /// </summary>
    /// <param name="f_NormalizedEndTime">0.0～1.0の終了進行度。</param>
    protected virtual void UpdateEndPhase(float f_NormalizedEndTime)
    {

    }

    /// <summary>
    /// 終了フェーズ完了時の処理です。
    /// </summary>
    protected virtual void OnEndPhaseComplete()
    {

    }

    /// <summary>
    /// ShaderOnlyEffect固有の通常更新処理です。
    /// </summary>
    protected virtual void UpdateShaderOnlyEffect()
    {

    }
}
