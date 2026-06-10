using System.Collections.Generic;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectPool.cs
 概要     : Effectを再利用するObjectPoolクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

/// <summary>
/// Effectを再利用するObjectPoolクラスです。
/// </summary>
public class CS_EffectPool
{
    /// <summary>
    /// 再利用するEffectPrefabです。
    /// </summary>
    private GameObject go_EffectPrefab;

    /// <summary>
    /// Pool用の親Transformです。
    /// </summary>
    private Transform tr_PoolParent;

    /// <summary>
    /// Pool内のEffectQueueです。
    /// </summary>
    private Queue<CSAD_EffectCommonProcessBase> queue_EffectPool =
        new Queue<CSAD_EffectCommonProcessBase>();

    /// <summary>
    /// EffectPoolを作成します。
    /// </summary>
    /// <param name="go_effectPrefab">再利用するEffectPrefab。</param>
    /// <param name="tr_poolParent">Pool用親Transform。</param>
    public CS_EffectPool(
        GameObject go_effectPrefab,
        Transform tr_poolParent)
    {
        go_EffectPrefab = go_effectPrefab;
        tr_PoolParent = tr_poolParent;
    }

    /// <summary>
    /// Effectを取得します。
    /// </summary>
    /// <param name="v3_Position">生成位置。</param>
    /// <param name="q_Rotation">生成回転。</param>
    /// <returns>取得したEffect。</returns>
    public CSAD_EffectCommonProcessBase GetEffect(
        Vector3 v3_Position,
        Quaternion q_Rotation)
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

        csad_EffectProcess.transform.SetPositionAndRotation(
            v3_Position,
            q_Rotation);

        csad_EffectProcess.gameObject.SetActive(true);

        csad_EffectProcess.SetOnEffectEndAction(ReturnEffect);

        return csad_EffectProcess;
    }

    /// <summary>
    /// EffectをPoolに戻します。
    /// </summary>
    /// <param name="csad_EffectProcess">戻すEffect。</param>
    private void ReturnEffect(CSAD_EffectCommonProcessBase csad_EffectProcess)
    {
        if (csad_EffectProcess == null)
        {
            return;
        }

        csad_EffectProcess.gameObject.SetActive(false);

        queue_EffectPool.Enqueue(csad_EffectProcess);
    }
}
