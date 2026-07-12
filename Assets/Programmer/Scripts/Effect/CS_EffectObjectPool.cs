using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectObjectPool.cs
 概要     : Effect専用ObjectPoolクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/07/09 新規作成
=====================================+
*/

/// <summary>
/// Effect専用のObjectPoolです。
/// CS_ObjectPoolを継承し、Effect使用時の親・位置・回転設定を追加します。
/// </summary>
public class CS_EffectObjectPool : CS_ObjectPool
{
    private const string EFFECT_POOL_PARENT_NAME = "ObjectPoolList_Effect";

    /// <summary>
    /// 再生中に使う親Transformです。
    /// </summary>
    private Transform tr_ActiveParent;

    /// <summary>
    /// 再生位置です。
    /// </summary>
    private Vector3 v3_PlayPosition;

    /// <summary>
    /// 再生回転です。
    /// </summary>
    private Quaternion q_PlayRotation;

    /// <summary>
    /// Effect専用ObjectPoolを作成します。
    /// </summary>
    public CS_EffectObjectPool(
        GameObject effectPrefab,
        int maxPoolCount)
        : base(
              effectPrefab,
              GetEffectPoolParent(),
              maxPoolCount)
    {

    }

    /// <summary>
    /// EffectObjectを取得します。
    /// ObjectPoolList_Effectから取り出して、使用中の親・位置・回転を設定します。
    /// </summary>
    public GameObject GetEffectObject(
        Transform activeParent,
        Vector3 position,
        Quaternion rotation)
    {
        tr_ActiveParent = activeParent;
        v3_PlayPosition = position;
        q_PlayRotation = rotation;

        return GetObject();
    }

    /// <summary>
    /// Effect取得時の処理です。
    /// </summary>
    protected override void OnGetObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        gameObject.transform.SetParent(
            tr_ActiveParent,
            true);

        gameObject.transform.SetPositionAndRotation(
            v3_PlayPosition,
            q_PlayRotation);
    }

    /// <summary>
    /// Effect返却時の処理です。
    /// </summary>
    protected override void OnReturnObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        CS_EffectTransformController cs_EffectTransformController =
            gameObject.GetComponent<CS_EffectTransformController>();

        if (cs_EffectTransformController != null)
        {
            cs_EffectTransformController.StopTransformControl();
        }
    }

    /// <summary>
    /// EffectPool用Parentを取得します。
    /// Scene上に無ければ作成します。
    /// </summary>
    private static GameObject GetEffectPoolParent()
    {
        GameObject go_PoolParent =
            GameObject.Find(EFFECT_POOL_PARENT_NAME);

        if (go_PoolParent == null)
        {
            go_PoolParent = new GameObject(EFFECT_POOL_PARENT_NAME);
        }

        return go_PoolParent;
    }
}
