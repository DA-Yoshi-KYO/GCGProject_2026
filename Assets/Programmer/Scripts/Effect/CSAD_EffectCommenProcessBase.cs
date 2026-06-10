using System;
using System.Collections;
using UnityEngine;

/*
+=====================================
 ファイル名 : CSAD_EffectCommonProcessBase.cs
 概要     : Effectの共通処理の基底クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

/// <summary>
/// Effectの共通処理を行う基底クラスです。
/// Template Methodとして、再生と終了の流れを固定します。
/// </summary>
public abstract class CSAD_EffectCommonProcessBase : MonoBehaviour, CSI_EffectPlayable
{
    /// <summary>
    /// Prefab側で設定するEffect再生データです。
    /// 呼び出し側のCSST_EffectPlayDataに値がない場合、この値を使用します。
    /// </summary>
    [Header("Prefab側Effect再生データ")]
    [SerializeField]
    protected CSST_EffectPlayData csst_DefaultEffectPlayData;

    /// <summary>
    /// 実際に再生で使うEffect再生データです。
    /// </summary>
    protected CSST_EffectPlayData csst_EffectPlayData;

    /// <summary>
    /// 自動終了処理のCoroutineです。
    /// </summary>
    private Coroutine co_AutoEndCoroutine;

    /// <summary>
    /// Effect終了時に通知する処理です。
    /// ObjectPoolへ戻す時などに使います。
    /// </summary>
    private Action<CSAD_EffectCommonProcessBase> action_OnEffectEnd;

    /// <summary>
    /// 終了要求済みかどうかです。
    /// </summary>
    protected bool bool_IsEndRequested { get; private set; }

    /// <summary>
    /// 終了完了済みかどうかです。
    /// </summary>
    private bool bool_IsEndFinished;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    public virtual void InitEffect()
    {

    }

    /// <summary>
    /// Effect終了時の通知処理を設定します。
    /// </summary>
    /// <param name="action_onEffectEnd">Effect終了時に呼ぶ処理。</param>
    public void SetOnEffectEndAction(Action<CSAD_EffectCommonProcessBase> action_onEffectEnd)
    {
        action_OnEffectEnd = action_onEffectEnd;
    }

    /// <summary>
    /// Effectを再生します。
    /// </summary>
    /// <param name="csst_effectData">呼び出し側の再生データ。</param>
    public void PlayEffect(CSST_EffectPlayData csst_effectData)
    {
        bool_IsEndRequested = false;
        bool_IsEndFinished = false;

        StopAutoEndTimer();

        csst_EffectPlayData = CreateMergedEffectPlayData(csst_effectData);

        gameObject.SetActive(true);

        InitEffect();

        ApplyCommonPlayData();

        PlayEffectProcess();

        StartAutoEndTimer();
    }

    /// <summary>
    /// 呼び出し側の再生データとPrefab側再生データを合成します。
    /// 呼び出し側に値がある場合は呼び出し側を優先します。
    /// </summary>
    /// <param name="csst_effectData">呼び出し側の再生データ。</param>
    /// <returns>合成後の再生データ。</returns>
    private CSST_EffectPlayData CreateMergedEffectPlayData(CSST_EffectPlayData csst_effectData)
    {
        CSST_EffectPlayData csst_ResultData = new CSST_EffectPlayData();
        csst_ResultData.CSST_EffectPlayData_Init();

        if (csst_effectData.v3_Position.HasValue)
        {
            csst_ResultData.SetPosition(csst_effectData.v3_Position.Value);
        }
        else if (csst_DefaultEffectPlayData.v3_Position.HasValue)
        {
            csst_ResultData.SetPosition(csst_DefaultEffectPlayData.v3_Position.Value);
        }

        if (csst_effectData.q_Rotation.HasValue)
        {
            csst_ResultData.SetRotation(csst_effectData.q_Rotation.Value);
        }
        else if (csst_DefaultEffectPlayData.q_Rotation.HasValue)
        {
            csst_ResultData.SetRotation(csst_DefaultEffectPlayData.q_Rotation.Value);
        }

        if (csst_effectData.v3_Scale.HasValue)
        {
            csst_ResultData.SetScale(csst_effectData.v3_Scale.Value);
        }
        else if (csst_DefaultEffectPlayData.v3_Scale.HasValue)
        {
            csst_ResultData.SetScale(csst_DefaultEffectPlayData.v3_Scale.Value);
        }

        if (csst_effectData.f_PlayTime.HasValue)
        {
            csst_ResultData.SetPlayTime(csst_effectData.f_PlayTime.Value);
        }
        else if (csst_DefaultEffectPlayData.f_PlayTime.HasValue)
        {
            csst_ResultData.SetPlayTime(csst_DefaultEffectPlayData.f_PlayTime.Value);
        }

        if (csst_effectData.f_EndTime.HasValue)
        {
            csst_ResultData.SetEndTime(csst_effectData.f_EndTime.Value);
        }
        else if (csst_DefaultEffectPlayData.f_EndTime.HasValue)
        {
            csst_ResultData.SetEndTime(csst_DefaultEffectPlayData.f_EndTime.Value);
        }

        if (csst_effectData.b_LoopFlag.HasValue)
        {
            csst_ResultData.SetLoopFlag(csst_effectData.b_LoopFlag.Value);
        }
        else if (csst_DefaultEffectPlayData.b_LoopFlag.HasValue)
        {
            csst_ResultData.SetLoopFlag(csst_DefaultEffectPlayData.b_LoopFlag.Value);
        }

        if (csst_effectData.b_HideOnEnd.HasValue)
        {
            csst_ResultData.SetHideOnEnd(csst_effectData.b_HideOnEnd.Value);
        }
        else if (csst_DefaultEffectPlayData.b_HideOnEnd.HasValue)
        {
            csst_ResultData.SetHideOnEnd(csst_DefaultEffectPlayData.b_HideOnEnd.Value);
        }

        if (csst_effectData.f_PlayEndTime.HasValue)
        {
            csst_ResultData.SetPlayEndTime(csst_effectData.f_PlayEndTime.Value);
        }
        else if (csst_DefaultEffectPlayData.f_PlayEndTime.HasValue)
        {
            csst_ResultData.SetPlayEndTime(csst_DefaultEffectPlayData.f_PlayEndTime.Value);
        }

        return csst_ResultData;
    }

    /// <summary>
    /// Effectの終了開始を行います。
    /// 終了演出がある場合は派生クラス側で行い、
    /// 終了演出完了後にFinishEndEffectを呼びます。
    /// </summary>
    public void EndEffect()
    {
        if (bool_IsEndRequested)
        {
            return;
        }

        bool_IsEndRequested = true;

        StopAutoEndTimer();

        EndEffectProcess();
    }

    /// <summary>
    /// 終了処理を完了します。
    /// 終了演出完了後に派生クラスから呼びます。
    /// </summary>
    protected void FinishEndEffect()
    {
        if (bool_IsEndFinished)
        {
            return;
        }

        bool_IsEndFinished = true;

        ApplyCommonEndData();

        NotifyEffectEnd();
    }

    /// <summary>
    /// 再生時の共通データを反映します。
    /// </summary>
    private void ApplyCommonPlayData()
    {
        if (csst_EffectPlayData.v3_Position.HasValue)
        {
            transform.position = csst_EffectPlayData.v3_Position.Value;
        }

        if (csst_EffectPlayData.q_Rotation.HasValue)
        {
            transform.rotation = csst_EffectPlayData.q_Rotation.Value;
        }

        if (csst_EffectPlayData.v3_Scale.HasValue)
        {
            transform.localScale = csst_EffectPlayData.v3_Scale.Value;
        }
    }

    /// <summary>
    /// 終了時の共通データを反映します。
    /// </summary>
    private void ApplyCommonEndData()
    {
        if (csst_EffectPlayData.b_HideOnEnd.HasValue == false)
        {
            return;
        }

        if (csst_EffectPlayData.b_HideOnEnd.Value)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 自動終了開始処理を開始します。
    /// 生成時間 + 自動終了待機時間 の後に EndEffect を呼びます。
    /// </summary>
    private void StartAutoEndTimer()
    {
        StopAutoEndTimer();

        if (csst_EffectPlayData.f_PlayEndTime.HasValue == false)
        {
            return;
        }

        float f_AutoEndDelay = 0.0f;

        if (csst_EffectPlayData.f_PlayTime.HasValue)
        {
            f_AutoEndDelay += Mathf.Max(0.0f, csst_EffectPlayData.f_PlayTime.Value);
        }

        f_AutoEndDelay += Mathf.Max(0.0f, csst_EffectPlayData.f_PlayEndTime.Value);

        co_AutoEndCoroutine = StartCoroutine(AutoEndCoroutine(f_AutoEndDelay));
    }

    /// <summary>
    /// 自動終了開始処理を停止します。
    /// </summary>
    private void StopAutoEndTimer()
    {
        if (co_AutoEndCoroutine == null)
        {
            return;
        }

        StopCoroutine(co_AutoEndCoroutine);
        co_AutoEndCoroutine = null;
    }

    /// <summary>
    /// 指定時間後に終了開始します。
    /// </summary>
    /// <param name="f_delayTime">待機時間。</param>
    /// <returns>Coroutine。</returns>
    private IEnumerator AutoEndCoroutine(float f_delayTime)
    {
        yield return new WaitForSeconds(f_delayTime);

        co_AutoEndCoroutine = null;

        EndEffect();
    }

    /// <summary>
    /// Effect終了を通知します。
    /// </summary>
    private void NotifyEffectEnd()
    {
        if (action_OnEffectEnd == null)
        {
            return;
        }

        action_OnEffectEnd(this);
    }

    /// <summary>
    /// 派生クラス側でEffectごとの再生処理を行います。
    /// </summary>
    protected abstract void PlayEffectProcess();

    /// <summary>
    /// 派生クラス側でEffectごとの終了開始処理を行います。
    /// 終了演出が無い場合は即終了します。
    /// </summary>
    protected virtual void EndEffectProcess()
    {
        FinishEndEffect();
    }
}
