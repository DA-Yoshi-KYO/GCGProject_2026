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
    /// 受け取ったEffect再生データです。
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
    /// <param name="csst_effectData">Effect再生データ。</param>
    public void PlayEffect(CSST_EffectPlayData csst_effectData)
    {
        csst_EffectPlayData = csst_effectData;

        gameObject.SetActive(true);

        InitEffect();

        ApplyCommonPlayData();

        PlayEffectProcess();

        StartAutoEndTimer();
    }

    /// <summary>
    /// Effectを終了します。
    /// </summary>
    public void EndEffect()
    {
        StopAutoEndTimer();

        EndEffectProcess();

        ApplyCommonEndData();

        NotifyEffectEnd();
    }

    /// <summary>
    /// 再生時の共通データを反映します。
    /// nullの場合は反映しません。
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
    /// 自動終了処理を開始します。
    /// </summary>
    private void StartAutoEndTimer()
    {
        StopAutoEndTimer();

        if (csst_EffectPlayData.f_PlayEndTime.HasValue == false)
        {
            return;
        }

        co_AutoEndCoroutine = StartCoroutine(AutoEndCoroutine());
    }

    /// <summary>
    /// 自動終了処理を停止します。
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
    /// 指定時間後にEffectを終了します。
    /// </summary>
    private IEnumerator AutoEndCoroutine()
    {
        yield return new WaitForSeconds(csst_EffectPlayData.f_PlayEndTime.Value);

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
    /// 派生クラス側でEffectごとの終了処理を行います。
    /// </summary>
    protected virtual void EndEffectProcess()
    {

    }
}
