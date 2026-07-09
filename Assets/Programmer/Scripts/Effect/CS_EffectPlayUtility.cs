using System.Collections.Generic;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectPlayUtility.cs
 概要     : EffectPrefabのPool設定を見て再生・終了を行う汎用Utility
 作者     : ヨシモト リョウ
 履歴     : 2026/07/09 新規作成
=====================================+
*/

/// <summary>
/// Effect再生用の汎用Utilityです。
/// CS_EffectFactoryを直接呼ばず、Prefab側のCS_EffectPoolSettingを見て
/// Poolを使うか、終了時にDestroyするかを切り替えます。
/// </summary>
public static class CS_EffectPlayUtility
{
    private const string POOL_ROOT_NAME = "EffectPoolRoot";

    /// <summary>
    /// PrefabごとのEffectPool管理Mapです。
    /// </summary>
    private static readonly Dictionary<GameObject, CS_EffectPool> dic_EffectPoolMap =
        new Dictionary<GameObject, CS_EffectPool>();

    /// <summary>
    /// Pool用Rootです。
    /// </summary>
    private static Transform tr_PoolRoot;

    /// <summary>
    /// 同じEffectが既にActiveなら新規生成せず、そのEffectを返します。
    /// ActiveでなければPool設定を見て再生します。
    /// </summary>
    public static CSAD_EffectCommonProcessBase PlaySingle(
        GameObject effectPrefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        ref CSAD_EffectCommonProcessBase currentEffect)
    {
        if (effectPrefab == null)
        {
            return null;
        }

        if (currentEffect != null &&
            currentEffect.gameObject != null &&
            currentEffect.gameObject.activeInHierarchy)
        {
            return currentEffect;
        }

        currentEffect = Play(
            effectPrefab,
            position,
            rotation,
            parent);

        return currentEffect;
    }

    /// <summary>
    /// Effectを再生します。
    /// PoolがONならPoolから取得し、PoolがOFFなら生成して終了時にDestroyします。
    /// </summary>
    public static CSAD_EffectCommonProcessBase Play(
        GameObject effectPrefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent)
    {
        if (effectPrefab == null)
        {
            return null;
        }

        CS_EffectPoolSetting poolSetting =
            effectPrefab.GetComponent<CS_EffectPoolSetting>();

        bool usePool = false;
        int maxPoolCount = 0;

        if (poolSetting != null)
        {
            usePool = poolSetting.IsUsePool();
            maxPoolCount = poolSetting.GetMaxPoolCount();
        }

        CSAD_EffectCommonProcessBase effect = null;

        if (usePool)
        {
            CS_EffectPool pool = GetOrCreatePool(effectPrefab, maxPoolCount);
            pool.SetMaxPoolCount(maxPoolCount);

            effect = pool.GetEffect(position, rotation);

            if (effect != null)
            {
                effect.transform.SetParent(
                    parent != null ? parent : GetPoolRoot(),
                    true);
            }
        }
        else
        {
            effect = CS_EffectFactory.CreateEffect(
                effectPrefab,
                position,
                rotation,
                parent);

            if (effect != null)
            {
                effect.SetOnEffectEndAction(DestroyEffect);
            }
        }

        if (effect == null)
        {
            return null;
        }

        CSST_EffectPlayData playData = new CSST_EffectPlayData();
        playData.CSST_EffectPlayData_Init();

        playData.SetPosition(position);
        playData.SetRotation(rotation);

        effect.PlayEffect(playData);

        return effect;
    }

    /// <summary>
    /// Effectを終了します。
    /// Pool ONなら終了後Poolに戻り、Pool OFFなら終了後Destroyされます。
    /// </summary>
    public static void EndEffect(ref CSAD_EffectCommonProcessBase currentEffect)
    {
        if (currentEffect == null)
        {
            return;
        }

        currentEffect.EndEffect();

        // すぐnullにしない。
        // 終了演出中に再生要求が来ると、二重生成される可能性があるため。
    }

    /// <summary>
    /// Prefabに対応するPoolを取得します。
    /// 無ければ新しく作成します。
    /// </summary>
    private static CS_EffectPool GetOrCreatePool(
        GameObject effectPrefab,
        int maxPoolCount)
    {
        if (dic_EffectPoolMap.TryGetValue(effectPrefab, out CS_EffectPool pool))
        {
            return pool;
        }

        pool = new CS_EffectPool(
            effectPrefab,
            GetPoolRoot(),
            maxPoolCount);

        dic_EffectPoolMap.Add(effectPrefab, pool);

        return pool;
    }

    /// <summary>
    /// Pool用Rootを取得します。
    /// 無ければScene上に作成します。
    /// </summary>
    private static Transform GetPoolRoot()
    {
        if (tr_PoolRoot != null)
        {
            return tr_PoolRoot;
        }

        GameObject root = GameObject.Find(POOL_ROOT_NAME);

        if (root == null)
        {
            root = new GameObject(POOL_ROOT_NAME);
        }

        tr_PoolRoot = root.transform;

        return tr_PoolRoot;
    }

    /// <summary>
    /// Poolを使わないEffectを終了時に破棄します。
    /// </summary>
    private static void DestroyEffect(CSAD_EffectCommonProcessBase effect)
    {
        if (effect == null)
        {
            return;
        }

        Object.Destroy(effect.gameObject);
    }
}
