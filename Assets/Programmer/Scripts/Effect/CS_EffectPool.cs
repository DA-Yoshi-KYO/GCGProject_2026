using System.Collections.Generic;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectPool.cs
 概要     : Effectを再利用するObjectPoolクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
            2026/07/09 Pool待機中ObjectをObjectPoolList_Effectへ戻す処理を追加
=====================================+
*/

/// <summary>
/// Effectを再利用するObjectPoolクラスです。
/// 使用していないEffectは ObjectPoolList_Effect の子に戻します。
/// </summary>
public class CS_EffectPool
{
    /// <summary>
    /// 再利用するEffectPrefabです。
    /// </summary>
    private GameObject go_EffectPrefab;

    /// <summary>
    /// Poolに戻す時の親Transformです。
    /// </summary>
    private Transform tr_PoolParent;

    /// <summary>
    /// 再生中の親Transformです。
    /// 既存の呼び出し側Parentを保持します。
    /// </summary>
    private Transform tr_DefaultActiveParent;

    /// <summary>
    /// Poolに保持する最大数です。
    /// </summary>
    private int n_MaxPoolCount;

    /// <summary>
    /// Pool内のEffectQueueです。
    /// </summary>
    private Queue<CSAD_EffectCommonProcessBase> queue_EffectPool =
        new Queue<CSAD_EffectCommonProcessBase>();

    /// <summary>
    /// Pool最大数を設定します。
    /// </summary>
    /// <param name="n_maxPoolCount">Pool最大数。</param>
    public void SetMaxPoolCount(int n_maxPoolCount)
    {
        n_MaxPoolCount = n_maxPoolCount;
    }

    /// <summary>
    /// EffectPoolを作成します。
    /// </summary>
    /// <param name="go_effectPrefab">再利用するEffectPrefab。</param>
    /// <param name="tr_poolParent">再生中の基本Parent。</param>
    /// <param name="n_maxPoolCount">Poolに保持する最大数。</param>
    public CS_EffectPool(
        GameObject go_effectPrefab,
        Transform tr_poolParent,
        int n_maxPoolCount)
    {
        go_EffectPrefab = go_effectPrefab;

        // 再生中の親は今まで通り呼び出し側を覚えます。
        tr_DefaultActiveParent = tr_poolParent;

        // Pool待機中は必ずObjectPoolList_Effectへ戻します。
        tr_PoolParent = CS_EffectPoolRoot.GetPoolRoot();

        n_MaxPoolCount = n_maxPoolCount;
    }

    /// <summary>
    /// Effectを取得します。
    /// Poolに待機中のEffectがあればそこから取り出し、無ければ生成します。
    /// </summary>
    /// <param name="v3_Position">生成位置。</param>
    /// <param name="q_Rotation">生成回転。</param>
    /// <param name="tr_ActiveParent">再生中の親Transform。</param>
    /// <returns>取得したEffect。</returns>
    public CSAD_EffectCommonProcessBase GetEffect(
        Vector3 v3_Position,
        Quaternion q_Rotation,
        Transform tr_ActiveParent = null)
    {
        CSAD_EffectCommonProcessBase csad_EffectProcess = null;

        while (queue_EffectPool.Count > 0 && csad_EffectProcess == null)
        {
            csad_EffectProcess = queue_EffectPool.Dequeue();
        }

        if (csad_EffectProcess == null)
        {
            csad_EffectProcess = CS_EffectFactory.CreateEffect(
                go_EffectPrefab,
                v3_Position,
                q_Rotation,
                tr_PoolParent);
        }

        if (csad_EffectProcess == null)
        {
            return null;
        }

        CS_EffectTransformController cs_EffectTransformController =
            csad_EffectProcess.GetComponent<CS_EffectTransformController>();

        if (cs_EffectTransformController != null)
        {
            cs_EffectTransformController.StopTransformControl();
        }

        Transform tr_UseActiveParent = tr_ActiveParent;

        if (tr_UseActiveParent == null)
        {
            tr_UseActiveParent = tr_DefaultActiveParent;
        }

        // 使用中はPoolRootから出します。
        csad_EffectProcess.transform.SetParent(
            tr_UseActiveParent,
            true);

        csad_EffectProcess.transform.SetPositionAndRotation(
            v3_Position,
            q_Rotation);

        if (go_EffectPrefab != null)
        {
            csad_EffectProcess.transform.localScale =
                go_EffectPrefab.transform.localScale;
        }

        csad_EffectProcess.gameObject.SetActive(true);

        // 終了したらPoolへ戻します。
        csad_EffectProcess.SetOnEffectEndAction(ReturnEffect);

        return csad_EffectProcess;
    }

    /// <summary>
    /// EffectをPoolに戻します。
    /// 最大保持数を超える場合は破棄します。
    /// </summary>
    /// <param name="csad_EffectProcess">戻すEffect。</param>
    private void ReturnEffect(CSAD_EffectCommonProcessBase csad_EffectProcess)
    {
        if (csad_EffectProcess == null)
        {
            return;
        }

        if (n_MaxPoolCount <= 0)
        {
            Object.Destroy(csad_EffectProcess.gameObject);
            return;
        }

        if (queue_EffectPool.Count >= n_MaxPoolCount)
        {
            Object.Destroy(csad_EffectProcess.gameObject);
            return;
        }

        CS_EffectTransformController cs_EffectTransformController =
            csad_EffectProcess.GetComponent<CS_EffectTransformController>();

        if (cs_EffectTransformController != null)
        {
            cs_EffectTransformController.StopTransformControl();
        }

        if (tr_PoolParent == null)
        {
            tr_PoolParent = CS_EffectPoolRoot.GetPoolRoot();
        }

        // Pool待機中はObjectPoolList_Effectへ戻します。
        csad_EffectProcess.transform.SetParent(
            tr_PoolParent,
            true);

        csad_EffectProcess.gameObject.SetActive(false);

        queue_EffectPool.Enqueue(csad_EffectProcess);
    }
}
