using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectPool.cs
 概要     : Effectを再利用するObjectPoolクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
            2026/07/09 CS_EffectObjectPoolを使う形へ変更
=====================================+
*/

/// <summary>
/// Effectを再利用するObjectPoolクラスです。
/// 内部ではCS_ObjectPoolを継承したCS_EffectObjectPoolを使用します。
/// </summary>
public class CS_EffectPool
{
    /// <summary>
    /// 再利用するEffectPrefabです。
    /// </summary>
    private GameObject go_EffectPrefab;

    /// <summary>
    /// Effect専用ObjectPoolです。
    /// </summary>
    private CS_EffectObjectPool cs_EffectObjectPool;

    /// <summary>
    /// 再生中の基本Parentです。
    /// </summary>
    private Transform tr_DefaultActiveParent;

    /// <summary>
    /// Pool最大数です。
    /// </summary>
    private int n_MaxPoolCount;

    /// <summary>
    /// EffectPoolを作成します。
    /// </summary>
    public CS_EffectPool(
        GameObject go_effectPrefab,
        Transform tr_poolParent,
        int n_maxPoolCount)
    {
        go_EffectPrefab = go_effectPrefab;
        tr_DefaultActiveParent = tr_poolParent;
        n_MaxPoolCount = Mathf.Max(0, n_maxPoolCount);

        cs_EffectObjectPool = new CS_EffectObjectPool(
            go_EffectPrefab,
            n_MaxPoolCount);
    }

    /// <summary>
    /// Pool最大数を設定します。
    /// </summary>
    public void SetMaxPoolCount(int n_maxPoolCount)
    {
        n_MaxPoolCount = Mathf.Max(0, n_maxPoolCount);

        if (cs_EffectObjectPool != null)
        {
            cs_EffectObjectPool.SetMaxPoolSize(n_MaxPoolCount);
        }
    }

    /// <summary>
    /// Effectを取得します。
    /// </summary>
    public CSAD_EffectCommonProcessBase GetEffect(
        Vector3 v3_Position,
        Quaternion q_Rotation)
    {
        return GetEffect(
            v3_Position,
            q_Rotation,
            tr_DefaultActiveParent);
    }

    /// <summary>
    /// Effectを取得します。
    /// </summary>
    public CSAD_EffectCommonProcessBase GetEffect(
        Vector3 v3_Position,
        Quaternion q_Rotation,
        Transform tr_ActiveParent)
    {
        if (go_EffectPrefab == null)
        {
            return null;
        }

        if (cs_EffectObjectPool == null)
        {
            cs_EffectObjectPool = new CS_EffectObjectPool(
                go_EffectPrefab,
                n_MaxPoolCount);
        }

        GameObject go_EffectObject =
            cs_EffectObjectPool.GetEffectObject(
                tr_ActiveParent,
                v3_Position,
                q_Rotation);

        if (go_EffectObject == null)
        {
            return null;
        }

        CSAD_EffectCommonProcessBase csad_EffectProcess =
            go_EffectObject.GetComponent<CSAD_EffectCommonProcessBase>();

        if (csad_EffectProcess == null)
        {
            Debug.LogWarning("[CS_EffectPool] EffectPrefabにCSAD_EffectCommonProcessBase継承クラスがありません : " + go_EffectObject.name);

            cs_EffectObjectPool.ReturnObject(go_EffectObject);
            return null;
        }

        if (go_EffectPrefab != null)
        {
            csad_EffectProcess.transform.localScale =
                go_EffectPrefab.transform.localScale;
        }

        // Effect終了時にPoolへ戻します。
        csad_EffectProcess.SetOnEffectEndAction(ReturnEffect);

        return csad_EffectProcess;
    }

    /// <summary>
    /// EffectをPoolへ戻します。
    /// </summary>
    private void ReturnEffect(CSAD_EffectCommonProcessBase csad_EffectProcess)
    {
        if (csad_EffectProcess == null)
        {
            return;
        }

        if (cs_EffectObjectPool == null)
        {
            Object.Destroy(csad_EffectProcess.gameObject);
            return;
        }

        cs_EffectObjectPool.ReturnObject(
            csad_EffectProcess.gameObject);
    }
}
