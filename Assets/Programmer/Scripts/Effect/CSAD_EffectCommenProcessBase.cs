using System.Collections;
using UnityEngine;

/*
+=====================================
 ファイル名 : CSAD_EffectCommonProcessBase.cs
 概要     : Effectの共通処理の基底クラス受けとるEffect再生データを保持するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

public abstract class CSAD_EffectCommonProcessBase : MonoBehaviour
{
    /// <summary>
    /// 受け取ったEffect再生データです。
    /// 実行時用の一時データとして保持します。
    /// </summary>
    protected CSST_EffectPlayData csst_EffectPlayData;

    /// <summary>
    /// 自動終了処理のCoroutineです。
    /// </summary>
    private Coroutine co_AutoEndCoroutine;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public virtual void InitEffect()
    {

    }

    /// <summary>
    /// 再生処理
    /// </summary>
    /// <param name="csst_effectData">Effect再生データ。</param>
    public void PlayEffect(CSST_EffectPlayData csst_effectData)
    {
        csst_EffectPlayData = csst_effectData;

        InitEffect();

        ApplyCommonPlayData();

        PlayEffectProcess();

        StartAutoEndTimer();
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public void EndEffect()
    {
        StopAutoEndTimer();

        EndEffectProcess();

        ApplyCommonEndData();
    }

    /// <summary>
    /// 再生時の共通データを反映します。
    /// nullの場合は何もしません。
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
