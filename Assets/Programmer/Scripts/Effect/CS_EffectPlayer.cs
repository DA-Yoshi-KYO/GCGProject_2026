using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectPlayer.cs
 概要     : Effect再生のFacadeクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
            2026/07/26 Prefab側Rotation反映処理を追加
            2026/07/26 再生中Prefab Rotation変更反映処理を追加
=====================================+
*/

/// <summary>
/// Effect再生のFacadeクラスです。
/// 呼び出し側はこのクラスを通してEffectを再生します。
/// </summary>
public class CS_EffectPlayer : MonoBehaviour
{
    [Header("再生するEffectPrefab")]
    [SerializeField]
    private GameObject go_EffectPrefab;

    /// <summary>
    /// EffectPoolです。
    /// </summary>
    private CS_EffectPool cs_EffectPool;

    /// <summary>
    /// 最後に再生したEffectです。
    /// </summary>
    private CSAD_EffectCommonProcessBase csad_CurrentEffect;

    /// <summary>
    /// 現在再生中Effectの基準Rotationです。
    /// 呼び出し側から指定されたRotationを保持します。
    /// </summary>
    private Quaternion q_CurrentBaseRotation =
        Quaternion.identity;

    /// <summary>
    /// 前回確認したPrefab側Rotationです。
    /// 実行中にPrefabのRotationが変更されたか確認するために使用します。
    /// </summary>
    private Quaternion q_LastPrefabRotation =
        Quaternion.identity;

    /// <summary>
    /// Prefab側Rotationの変更を、
    /// 現在再生中Effectへ反映するかどうかです。
    /// </summary>
    private bool b_IsPrefabRotationTracking = false;


    /// <summary>
    /// 更新処理です。
    /// 実行中にPrefab側Rotationが変更された場合、
    /// 現在再生中のEffectにも反映します。
    /// </summary>
    private void Update()
    {
        UpdatePrefabRotation();
    }

    /// <summary>
    /// Prefab側Rotationの変更を確認し、
    /// 現在再生中Effectへ反映します。
    /// </summary>
    private void UpdatePrefabRotation()
    {
        if (b_IsPrefabRotationTracking == false)
        {
            return;
        }

        if (go_EffectPrefab == null)
        {
            return;
        }

        if (csad_CurrentEffect == null)
        {
            return;
        }

        if (csad_CurrentEffect.gameObject.activeInHierarchy == false)
        {
            return;
        }

        // Scene上に直接存在するEffectは、
        // Transformを直接変更できるため対象外です。
        if (IsSceneEffectObject())
        {
            return;
        }

        Quaternion q_CurrentPrefabRotation =
            go_EffectPrefab.transform.localRotation;

        // Prefab側Rotationが変更されていなければ何もしません。
        if (Quaternion.Angle(
            q_LastPrefabRotation,
            q_CurrentPrefabRotation) <= 0.001f)
        {
            return;
        }

        q_LastPrefabRotation =
            q_CurrentPrefabRotation;

        // 呼び出し側Rotation
        // ×
        // Prefab側Rotation
        //
        // として現在のEffectへ反映します。
        csad_CurrentEffect.transform.rotation =
            q_CurrentBaseRotation *
            q_CurrentPrefabRotation;
    }

    /// <summary>
    /// Effectを再生します。
    /// Prefab側のPool設定を使用します。
    /// </summary>
    /// <param name="csst_EffectPlayData">Effect再生データ。</param>
    /// <returns>再生したEffect。</returns>
    public CSAD_EffectCommonProcessBase PlayEffect(
        CSST_EffectPlayData csst_EffectPlayData)
    {
        CS_EffectPoolSetting cs_EffectPoolSetting =
            GetEffectPoolSetting();

        bool b_UsePool = true;
        int n_MaxPoolCount = 3;

        if (cs_EffectPoolSetting != null)
        {
            b_UsePool =
                cs_EffectPoolSetting.IsUsePool();

            n_MaxPoolCount =
                cs_EffectPoolSetting.GetMaxPoolCount();
        }

        return PlayEffectInternal(
            csst_EffectPlayData,
            b_UsePool,
            n_MaxPoolCount);
    }

    /// <summary>
    /// Effectを再生します。
    /// 引数でPool最大数を上書きします。
    /// </summary>
    /// <param name="csst_EffectPlayData">Effect再生データ。</param>
    /// <param name="n_OverrideMaxPoolCount">上書きするPool最大数。</param>
    /// <returns>再生したEffect。</returns>
    public CSAD_EffectCommonProcessBase PlayEffect(
        CSST_EffectPlayData csst_EffectPlayData,
        int n_OverrideMaxPoolCount)
    {
        CS_EffectPoolSetting cs_EffectPoolSetting =
            GetEffectPoolSetting();

        bool b_UsePool = true;

        if (cs_EffectPoolSetting != null)
        {
            b_UsePool =
                cs_EffectPoolSetting.IsUsePool();
        }

        return PlayEffectInternal(
            csst_EffectPlayData,
            b_UsePool,
            n_OverrideMaxPoolCount);
    }

    /// <summary>
    /// Effect再生の内部処理です。
    /// </summary>
    private CSAD_EffectCommonProcessBase PlayEffectInternal(
        CSST_EffectPlayData csst_EffectPlayData,
        bool b_UsePool,
        int n_MaxPoolCount)
    {
        if (go_EffectPrefab == null)
        {
            Debug.LogWarning(
                "[CS_EffectPlayer] EffectPrefabが設定されていません。");

            return null;
        }

        // Hierarchy上にある既存Effectの場合は、
        // 生成せずにそのまま再生します。
        if (IsSceneEffectObject())
        {
            b_IsPrefabRotationTracking = false;

            return PlaySceneEffectObject(
                csst_EffectPlayData);
        }

        /*
         * ==================================================
         * Position
         * ==================================================
         */

        Vector3 v3_Position =
            transform.position;

        if (csst_EffectPlayData.v3_Position.HasValue)
        {
            v3_Position =
                csst_EffectPlayData.v3_Position.Value;
        }

        /*
         * ==================================================
         * Rotation
         * ==================================================
         */

        // 呼び出し側のRotationを取得します。
        Quaternion q_BaseRotation =
            transform.rotation;

        if (csst_EffectPlayData.q_Rotation.HasValue)
        {
            q_BaseRotation =
                csst_EffectPlayData.q_Rotation.Value;
        }

        // Prefabそのものに設定されているRotationです。
        Quaternion q_PrefabRotation =
            go_EffectPrefab.transform.localRotation;

        // 呼び出し側Rotationに、
        // Prefab側Rotationを追加します。
        Quaternion q_FinalRotation =
            q_BaseRotation *
            q_PrefabRotation;

        // 実行中のPrefab Rotation変更確認用に保存します。
        q_CurrentBaseRotation =
            q_BaseRotation;

        q_LastPrefabRotation =
            q_PrefabRotation;

        b_IsPrefabRotationTracking = true;

        /*
         * ==================================================
         * Effect取得
         * ==================================================
         */

        if (b_UsePool)
        {
            if (cs_EffectPool == null)
            {
                cs_EffectPool =
                    new CS_EffectPool(
                        go_EffectPrefab,
                        transform,
                        n_MaxPoolCount);
            }

            cs_EffectPool.SetMaxPoolCount(
                n_MaxPoolCount);

            csad_CurrentEffect =
                cs_EffectPool.GetEffect(
                    v3_Position,
                    q_FinalRotation);
        }
        else
        {
            csad_CurrentEffect =
                CS_EffectFactory.CreateEffect(
                    go_EffectPrefab,
                    v3_Position,
                    q_FinalRotation,
                    transform);

            if (csad_CurrentEffect != null)
            {
                csad_CurrentEffect.SetOnEffectEndAction(
                    DestroyEffect);
            }
        }

        if (csad_CurrentEffect == null)
        {
            b_IsPrefabRotationTracking = false;
            return null;
        }

        /*
         * ==================================================
         * Scale
         * ==================================================
         */

        if (csst_EffectPlayData.v3_Scale.HasValue)
        {
            csad_CurrentEffect.transform.localScale =
                csst_EffectPlayData.v3_Scale.Value;
        }

        /*
         * ==================================================
         * 再生
         * ==================================================
         */

        // CSAD_EffectCommonProcessBase側でも
        // PlayEffect時にRotationが適用されるため、
        // 最終Rotationを再生データにも設定します。
        CSST_EffectPlayData csst_RuntimePlayData =
            csst_EffectPlayData;

        csst_RuntimePlayData.SetRotation(
            q_FinalRotation);

        csad_CurrentEffect.PlayEffect(
            csst_RuntimePlayData);

        return csad_CurrentEffect;
    }

    /// <summary>
    /// go_EffectPrefabに設定されているObjectが
    /// Scene上の既存Objectか確認します。
    /// </summary>
    /// <returns>Scene上のObjectならtrue。</returns>
    private bool IsSceneEffectObject()
    {
        if (go_EffectPrefab == null)
        {
            return false;
        }

        return go_EffectPrefab.scene.IsValid() &&
               go_EffectPrefab.scene.isLoaded;
    }

    /// <summary>
    /// Scene上に既に存在するEffectObjectを直接再生します。
    /// </summary>
    /// <param name="csst_EffectPlayData">Effect再生データ。</param>
    /// <returns>再生したEffect。</returns>
    private CSAD_EffectCommonProcessBase PlaySceneEffectObject(
        CSST_EffectPlayData csst_EffectPlayData)
    {
        b_IsPrefabRotationTracking = false;

        go_EffectPrefab.SetActive(true);

        CSAD_EffectCommonProcessBase csad_EffectProcess =
            go_EffectPrefab.GetComponent<
                CSAD_EffectCommonProcessBase>();

        if (csad_EffectProcess == null)
        {
            csad_EffectProcess =
                go_EffectPrefab.GetComponentInChildren<
                    CSAD_EffectCommonProcessBase>(true);
        }

        if (csad_EffectProcess == null)
        {
            Debug.LogWarning(
                "[CS_EffectPlayer] " +
                "Scene上のEffectObjectに" +
                "CSAD_EffectCommonProcessBase継承クラスがありません : " +
                go_EffectPrefab.name);

            return null;
        }

        csad_CurrentEffect =
            csad_EffectProcess;

        csad_CurrentEffect.gameObject.SetActive(true);

        CS_EffectTransformController cs_EffectTransformController =
            csad_CurrentEffect.GetComponent<
                CS_EffectTransformController>();

        if (cs_EffectTransformController != null)
        {
            cs_EffectTransformController.StopTransformControl();
        }

        // 既存ObjectはScene上に置いた位置を
        // 基本そのまま使用します。
        //
        // 呼び出し側から指定された場合だけ上書きします。

        if (csst_EffectPlayData.v3_Position.HasValue)
        {
            go_EffectPrefab.transform.position =
                csst_EffectPlayData.v3_Position.Value;
        }

        if (csst_EffectPlayData.q_Rotation.HasValue)
        {
            go_EffectPrefab.transform.rotation =
                csst_EffectPlayData.q_Rotation.Value;
        }

        if (csst_EffectPlayData.v3_Scale.HasValue)
        {
            go_EffectPrefab.transform.localScale =
                csst_EffectPlayData.v3_Scale.Value;
        }

        csad_CurrentEffect.PlayEffect(
            csst_EffectPlayData);

        return csad_CurrentEffect;
    }

    /// <summary>
    /// EffectPrefabのPool設定を取得します。
    /// </summary>
    /// <returns>Pool設定。</returns>
    private CS_EffectPoolSetting GetEffectPoolSetting()
    {
        if (go_EffectPrefab == null)
        {
            return null;
        }

        return go_EffectPrefab.GetComponent<
            CS_EffectPoolSetting>();
    }

    /// <summary>
    /// Scene上に既に存在するEffectを再生します。
    /// PrefabPoolは使わず、渡されたEffectをそのまま再生します。
    /// </summary>
    /// <param name="csad_ExistingEffect">既存Effect。</param>
    /// <param name="csst_EffectPlayData">Effect再生データ。</param>
    /// <returns>再生したEffect。</returns>
    public CSAD_EffectCommonProcessBase PlayExistingEffect(
        CSAD_EffectCommonProcessBase csad_ExistingEffect,
        CSST_EffectPlayData csst_EffectPlayData)
    {
        if (csad_ExistingEffect == null)
        {
            Debug.LogWarning(
                "[CS_EffectPlayer] ExistingEffectがnullです。");

            return null;
        }

        // Prefab追従対象ではありません。
        b_IsPrefabRotationTracking = false;

        if (csst_EffectPlayData.v3_Position.HasValue)
        {
            csad_ExistingEffect.transform.position =
                csst_EffectPlayData.v3_Position.Value;
        }

        if (csst_EffectPlayData.q_Rotation.HasValue)
        {
            csad_ExistingEffect.transform.rotation =
                csst_EffectPlayData.q_Rotation.Value;
        }

        if (csst_EffectPlayData.v3_Scale.HasValue)
        {
            csad_ExistingEffect.transform.localScale =
                csst_EffectPlayData.v3_Scale.Value;
        }

        csad_CurrentEffect =
            csad_ExistingEffect;

        csad_CurrentEffect.gameObject.SetActive(true);

        csad_CurrentEffect.PlayEffect(
            csst_EffectPlayData);

        return csad_CurrentEffect;
    }

    /// <summary>
    /// 最後に再生したEffectを終了します。
    /// </summary>
    public void EndCurrentEffect()
    {
        if (csad_CurrentEffect == null)
        {
            return;
        }

        csad_CurrentEffect.EndEffect();
    }

    /// <summary>
    /// Effect終了時に破棄します。
    /// </summary>
    private void DestroyEffect(
        CSAD_EffectCommonProcessBase csad_Effect)
    {
        if (csad_Effect == null)
        {
            return;
        }

        if (csad_CurrentEffect == csad_Effect)
        {
            csad_CurrentEffect = null;
            b_IsPrefabRotationTracking = false;
        }

        Destroy(csad_Effect.gameObject);
    }
}
