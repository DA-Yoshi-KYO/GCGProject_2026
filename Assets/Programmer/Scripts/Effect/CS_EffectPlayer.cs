using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectPlayer.cs
 概要     : Effect再生のFacadeクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成 
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
    /// Effectを再生します。
    /// Prefab側のPool設定を使用します。
    /// </summary>
    /// <param name="csst_EffectPlayData">Effect再生データ。</param>
    /// <returns>再生したEffect。</returns>
    public CSAD_EffectCommonProcessBase PlayEffect(CSST_EffectPlayData csst_EffectPlayData)
    {
        CS_EffectPoolSetting cs_EffectPoolSetting = GetEffectPoolSetting();

        bool b_UsePool = true;
        int n_MaxPoolCount = 3;

        if (cs_EffectPoolSetting != null)
        {
            b_UsePool = cs_EffectPoolSetting.IsUsePool();
            n_MaxPoolCount = cs_EffectPoolSetting.GetMaxPoolCount();
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
        CS_EffectPoolSetting cs_EffectPoolSetting = GetEffectPoolSetting();

        bool b_UsePool = true;

        if (cs_EffectPoolSetting != null)
        {
            b_UsePool = cs_EffectPoolSetting.IsUsePool();
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
            Debug.LogWarning("[CS_EffectPlayer] EffectPrefabが設定されていません。");
            return null;
        }

        // Hierarchy上にある既存Effectの場合は、生成せずにそのまま再生します。
        if (IsSceneEffectObject())
        {
            return PlaySceneEffectObject(csst_EffectPlayData);
        }

        Vector3 v3_Position = transform.position;
        Quaternion q_Rotation = transform.rotation;

        if (csst_EffectPlayData.v3_Position.HasValue)
        {
            v3_Position = csst_EffectPlayData.v3_Position.Value;
        }

        if (csst_EffectPlayData.q_Rotation.HasValue)
        {
            q_Rotation = csst_EffectPlayData.q_Rotation.Value;
        }

        if (b_UsePool)
        {
            if (cs_EffectPool == null)
            {
                cs_EffectPool = new CS_EffectPool(
                    go_EffectPrefab,
                    transform,
                    n_MaxPoolCount);
            }

            cs_EffectPool.SetMaxPoolCount(n_MaxPoolCount);

            csad_CurrentEffect = cs_EffectPool.GetEffect(
                v3_Position,
                q_Rotation);
        }
        else
        {
            csad_CurrentEffect = CS_EffectFactory.CreateEffect(
                go_EffectPrefab,
                v3_Position,
                q_Rotation,
                transform);

            if (csad_CurrentEffect != null)
            {
                csad_CurrentEffect.SetOnEffectEndAction(DestroyEffect);
            }
        }


        if (csad_CurrentEffect == null)
        {
            return null;
        }

        if (csst_EffectPlayData.v3_Scale.HasValue)
        {
            csad_CurrentEffect.transform.localScale =
                csst_EffectPlayData.v3_Scale.Value;
        }

        csad_CurrentEffect.PlayEffect(csst_EffectPlayData);

        return csad_CurrentEffect;
    }

    /// <summary>
    /// go_EffectPrefabに設定されているObjectがScene上の既存Objectか確認します。
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
        go_EffectPrefab.SetActive(true);

        CSAD_EffectCommonProcessBase csad_EffectProcess =
            go_EffectPrefab.GetComponent<CSAD_EffectCommonProcessBase>();

        if (csad_EffectProcess == null)
        {
            csad_EffectProcess =
                go_EffectPrefab.GetComponentInChildren<CSAD_EffectCommonProcessBase>(true);
        }

        if (csad_EffectProcess == null)
        {
            Debug.LogWarning("[CS_EffectPlayer] Scene上のEffectObjectにCSAD_EffectCommonProcessBase継承クラスがありません : " + go_EffectPrefab.name);
            return null;
        }

        csad_CurrentEffect = csad_EffectProcess;

        csad_CurrentEffect.gameObject.SetActive(true);

        CS_EffectTransformController cs_EffectTransformController =
            csad_CurrentEffect.GetComponent<CS_EffectTransformController>();

        if (cs_EffectTransformController != null)
        {
            cs_EffectTransformController.StopTransformControl();
        }

        // 既存ObjectはScene上に置いた位置を基本そのまま使います。
        // 呼び出し側が指定した時だけ上書きします。
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

        csad_CurrentEffect.PlayEffect(csst_EffectPlayData);

        return csad_CurrentEffect;
    }

    /// <summary>
    /// EffectPrefab側のPool設定を取得します。
    /// </summary>
    /// <returns>Pool設定。</returns>
    private CS_EffectPoolSetting GetEffectPoolSetting()
    {
        if (go_EffectPrefab == null)
        {
            return null;
        }

        return go_EffectPrefab.GetComponent<CS_EffectPoolSetting>();
    }

        /// <summary>
    /// 既にScene上に存在するEffectを再生します。
    /// Prefab生成やPoolは使わず、指定されたEffectをそのまま再生します。
    /// </summary>
    /// <param name="csad_ExistingEffect">既存のEffect。</param>
    /// <param name="csst_EffectPlayData">Effect再生データ。</param>
    /// <returns>再生したEffect。</returns>
    public CSAD_EffectCommonProcessBase PlayExistingEffect(
        CSAD_EffectCommonProcessBase csad_ExistingEffect,
        CSST_EffectPlayData csst_EffectPlayData)
    {
        if (csad_ExistingEffect == null)
        {
            Debug.LogWarning("[CS_EffectPlayer] 既存Effectがnullです。");
            return null;
        }

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

        csad_CurrentEffect = csad_ExistingEffect;

        csad_CurrentEffect.gameObject.SetActive(true);
        csad_CurrentEffect.PlayEffect(csst_EffectPlayData);

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
    private void DestroyEffect(CSAD_EffectCommonProcessBase csad_Effect)
    {
        if (csad_Effect == null)
        {
            return;
        }

        Destroy(csad_Effect.gameObject);
    }
}
