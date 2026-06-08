/*
+=====================================
 ファイル名 : CS_EffectPlayer.cs
 概要     : エフェクトPrefabを生成して再生する共通プレイヤー
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
            2026/06/03 EffectId指定再生、再生速度上書き、生成スケール指定を追加
=====================================+
*/

using UnityEngine;

/// <summary>
/// エフェクトPrefabを生成して再生するための共通クラスです。
/// </summary>
public static class CS_EffectPlayer
{
    private const string EffectRegistryResourcesPath = "Effect/CSS_EffectRegistry";

    private static CSS_EffectRegistry cachedEffectRegistry;

    /// <summary>
    /// エフェクトPrefabを指定位置に生成して再生します。
    /// デフォルト回転・デフォルト速度・デフォルトスケールを使用します。
    /// </summary>
    /// <param name="cs_EffectPrefab">生成するエフェクトPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(CS_EffectRoot cs_EffectPrefab, Vector3 position)
    {
        return Play(cs_EffectPrefab, position, Quaternion.identity, null);
    }

    /// <summary>
    /// エフェクトPrefabを指定位置・指定回転で生成して再生します。
    /// デフォルト速度・デフォルトスケールを使用します。
    /// </summary>
    /// <param name="cs_EffectPrefab">生成するエフェクトPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(CS_EffectRoot cs_EffectPrefab, Vector3 position, Quaternion rotation)
    {
        return Play(cs_EffectPrefab, position, rotation, null);
    }

    /// <summary>
    /// エフェクトPrefabを指定位置・指定回転・親Transform付きで生成して再生します。
    /// デフォルト速度・デフォルトスケールを使用します。
    /// </summary>
    /// <param name="cs_EffectPrefab">生成するエフェクトPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(
        CS_EffectRoot cs_EffectPrefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent)
    {
        return PlayInternal(
            cs_EffectPrefab,
            position,
            rotation,
            parent,
            false,
            1.0f,
            false,
            Vector3.one);
    }

    /// <summary>
    /// エフェクトPrefabを指定位置・指定回転・親Transform付きで生成して再生します。
    /// 再生速度を上書きします。
    /// </summary>
    /// <param name="cs_EffectPrefab">生成するエフェクトPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <param name="overridePlaySpeed">上書き再生速度。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(
        CS_EffectRoot cs_EffectPrefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        float overridePlaySpeed)
    {
        return PlayInternal(
            cs_EffectPrefab,
            position,
            rotation,
            parent,
            true,
            overridePlaySpeed,
            false,
            Vector3.one);
    }

    /// <summary>
    /// エフェクトPrefabを指定位置・指定回転・指定スケールで生成して再生します。
    /// デフォルト再生速度を使用します。
    /// </summary>
    /// <param name="cs_EffectPrefab">生成するエフェクトPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <param name="scale">生成スケール。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(
        CS_EffectRoot cs_EffectPrefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        Vector3 scale)
    {
        return PlayInternal(
            cs_EffectPrefab,
            position,
            rotation,
            parent,
            false,
            1.0f,
            true,
            scale);
    }

    /// <summary>
    /// エフェクトPrefabを指定位置・指定回転・指定スケール・指定速度で生成して再生します。
    /// </summary>
    /// <param name="cs_EffectPrefab">生成するエフェクトPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <param name="scale">生成スケール。</param>
    /// <param name="overridePlaySpeed">上書き再生速度。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(
        CS_EffectRoot cs_EffectPrefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        Vector3 scale,
        float overridePlaySpeed)
    {
        return PlayInternal(
            cs_EffectPrefab,
            position,
            rotation,
            parent,
            true,
            overridePlaySpeed,
            true,
            scale);
    }

    /// <summary>
    /// エフェクトPrefabを生成して再生する内部処理です。
    /// </summary>
    /// <param name="cs_EffectPrefab">生成するエフェクトPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <param name="bool_IsOverridePlaySpeed">再生速度を上書きするか。</param>
    /// <param name="overridePlaySpeed">上書き再生速度。</param>
    /// <param name="bool_IsOverrideScale">生成スケールを上書きするか。</param>
    /// <param name="overrideScale">上書き生成スケール。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    private static CS_EffectHandle PlayInternal(
        CS_EffectRoot cs_EffectPrefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        bool bool_IsOverridePlaySpeed,
        float overridePlaySpeed,
        bool bool_IsOverrideScale,
        Vector3 overrideScale)
    {
        if (cs_EffectPrefab == null)
        {
            Debug.LogWarning("[EffectPlayer] 生成するEffectPrefabがnullです。");
            return null;
        }

        GameObject effectObject = Object.Instantiate(
            cs_EffectPrefab.gameObject,
            position,
            rotation,
            parent);

        effectObject.name = cs_EffectPrefab.gameObject.name + "_Instance";

        // Instantiate直後のScaleをPrefab初期Scaleとして保存します。
        Vector3 defaultLocalScale = effectObject.transform.localScale;

        if (bool_IsOverrideScale)
        {
            effectObject.transform.localScale =
                CS_EffectScaleUtility.CalculateScale(defaultLocalScale, overrideScale);
        }

        CS_EffectRoot cs_EffectRoot = effectObject.GetComponent<CS_EffectRoot>();

        if (cs_EffectRoot == null)
        {
            Debug.LogWarning("[EffectPlayer] 生成したPrefabにCS_EffectRootがありません : " + effectObject.name);
            Object.Destroy(effectObject);
            return null;
        }

        if (!effectObject.activeSelf)
        {
            effectObject.SetActive(true);
        }

        CSI_EffectPlayable effectPlayable = cs_EffectRoot.GetEffectPlayable();

        if (effectPlayable == null)
        {
            Debug.LogWarning("[EffectPlayer] CSI_EffectPlayableが見つかりません : " + effectObject.name);
            Object.Destroy(effectObject);
            return null;
        }

        float finalPlaySpeed = cs_EffectRoot.DefaultPlaySpeed;

        if (bool_IsOverridePlaySpeed)
        {
            finalPlaySpeed = Mathf.Max(0.01f, overridePlaySpeed);
        }

        effectPlayable.SetPlaySpeed(finalPlaySpeed);
        effectPlayable.PlayEffect();

        CS_EffectHandle effectHandle = new CS_EffectHandle(
            effectObject,
            cs_EffectRoot,
            effectPlayable,
            defaultLocalScale);

        if (cs_EffectRoot.ShouldAutoDestroyByDefault)
        {
            effectHandle.Destroy(cs_EffectRoot.DefaultLifeTime);
        }

        return effectHandle;
    }

    /// <summary>
    /// EffectRegistryを取得します。
    /// </summary>
    /// <returns>EffectRegistry。</returns>
    private static CSS_EffectRegistry GetEffectRegistry()
    {
        if (cachedEffectRegistry != null)
        {
            return cachedEffectRegistry;
        }

        cachedEffectRegistry = Resources.Load<CSS_EffectRegistry>(EffectRegistryResourcesPath);

        if (cachedEffectRegistry == null)
        {
            Debug.LogError("[EffectPlayer] CSS_EffectRegistry が見つかりません : Resources/" + EffectRegistryResourcesPath);
        }

        return cachedEffectRegistry;
    }

    /// <summary>
    /// EffectIdからEffectPrefabを取得します。
    /// </summary>
    /// <param name="effectId">再生したいEffectId。</param>
    /// <returns>EffectPrefab。</returns>
    private static CS_EffectRoot FindEffectPrefab(CSE_EffectId effectId)
    {
        if (effectId == CSE_EffectId.None)
        {
            Debug.LogWarning("[EffectPlayer] Noneは再生できません。");
            return null;
        }

        CSS_EffectRegistry effectRegistry = GetEffectRegistry();

        if (effectRegistry == null)
        {
            return null;
        }

        CS_EffectRoot effectPrefab = effectRegistry.FindEffectPrefab(effectId.ToString());

        if (effectPrefab == null)
        {
            Debug.LogWarning("[EffectPlayer] EffectIdに対応するPrefabが見つかりません : " + effectId);
        }

        return effectPrefab;
    }

    /// <summary>
    /// EffectIdを指定して、エフェクトを指定位置に生成して再生します。
    /// デフォルト回転・デフォルト速度・デフォルトスケールを使用します。
    /// </summary>
    /// <param name="effectId">再生するEffectId。</param>
    /// <param name="position">生成位置。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(CSE_EffectId effectId, Vector3 position)
    {
        return Play(effectId, position, Quaternion.identity);
    }

    /// <summary>
    /// EffectIdを指定して、エフェクトを指定位置・指定回転で生成して再生します。
    /// デフォルト速度・デフォルトスケールを使用します。
    /// </summary>
    /// <param name="effectId">再生するEffectId。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(CSE_EffectId effectId, Vector3 position, Quaternion rotation)
    {
        return Play(effectId, position, rotation, null);
    }

    /// <summary>
    /// EffectIdを指定して、エフェクトを指定位置・指定回転・親Transform付きで生成して再生します。
    /// デフォルト速度・デフォルトスケールを使用します。
    /// </summary>
    /// <param name="effectId">再生するEffectId。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(
        CSE_EffectId effectId,
        Vector3 position,
        Quaternion rotation,
        Transform parent)
    {
        CS_EffectRoot effectPrefab = FindEffectPrefab(effectId);

        if (effectPrefab == null)
        {
            return null;
        }

        return Play(effectPrefab, position, rotation, parent);
    }

    /// <summary>
    /// EffectIdを指定して、エフェクトを指定位置・指定回転・親Transform付きで生成して再生します。
    /// 再生速度を上書きします。
    /// </summary>
    /// <param name="effectId">再生するEffectId。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <param name="overridePlaySpeed">上書き再生速度。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(
        CSE_EffectId effectId,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        float overridePlaySpeed)
    {
        CS_EffectRoot effectPrefab = FindEffectPrefab(effectId);

        if (effectPrefab == null)
        {
            return null;
        }

        return Play(effectPrefab, position, rotation, parent, overridePlaySpeed);
    }

    /// <summary>
    /// EffectIdを指定して、エフェクトを指定位置・指定回転・指定スケールで生成して再生します。
    /// デフォルト再生速度を使用します。
    /// </summary>
    /// <param name="effectId">再生するEffectId。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <param name="scale">生成スケール。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(
        CSE_EffectId effectId,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        Vector3 scale)
    {
        CS_EffectRoot effectPrefab = FindEffectPrefab(effectId);

        if (effectPrefab == null)
        {
            return null;
        }

        return Play(effectPrefab, position, rotation, parent, scale);
    }

    /// <summary>
    /// EffectIdを指定して、エフェクトを指定位置・指定回転・指定スケール・指定速度で生成して再生します。
    /// </summary>
    /// <param name="effectId">再生するEffectId。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">親Transform。</param>
    /// <param name="scale">生成スケール。</param>
    /// <param name="overridePlaySpeed">上書き再生速度。</param>
    /// <returns>生成したエフェクトのハンドル。</returns>
    public static CS_EffectHandle Play(
        CSE_EffectId effectId,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        Vector3 scale,
        float overridePlaySpeed)
    {
        CS_EffectRoot effectPrefab = FindEffectPrefab(effectId);

        if (effectPrefab == null)
        {
            return null;
        }

        return Play(effectPrefab, position, rotation, parent, scale, overridePlaySpeed);
    }

    /// <summary>
    /// EffectIdからEffectDataを取得します。
    /// </summary>
    /// <param name="effectId">確認したいEffectId。</param>
    /// <returns>EffectData。見つからない場合はnull。</returns>
    private static CSS_EffectData FindEffectData(CSE_EffectId effectId)
    {
        if (effectId == CSE_EffectId.None)
        {
            return null;
        }

        CSS_EffectRegistry effectRegistry = GetEffectRegistry();

        if (effectRegistry == null)
        {
            return null;
        }

        return effectRegistry.FindEffectData(effectId.ToString());
    }

    /// <summary>
    /// EffectIdからエフェクト種別を取得します。
    /// </summary>
    /// <param name="effectId">確認したいEffectId。</param>
    /// <returns>エフェクト種別。見つからない場合はCustomCSを返します。</returns>
    public static CSE_EffectType GetEffectType(CSE_EffectId effectId)
    {
        CSS_EffectData effectData = FindEffectData(effectId);

        if (effectData == null)
        {
            Debug.LogWarning("[EffectPlayer] EffectTypeを取得できません : " + effectId);
            return CSE_EffectType.CustomCS;
        }

        return effectData.EffectType;
    }
}
