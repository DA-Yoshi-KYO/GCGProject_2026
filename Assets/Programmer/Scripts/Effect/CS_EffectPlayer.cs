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
        }

        if (csad_CurrentEffect == null)
        {
            return null;
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
}
