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

    [Header("Poolを使うか")]
    [SerializeField]
    private bool bool_UsePool = true;

    /// <summary>
    /// EffectPoolです。
    /// </summary>
    private CS_EffectPool cs_EffectPool;

    /// <summary>
    /// 最後に再生したEffectです。
    /// </summary>
    private CSAD_EffectCommonProcessBase csad_CurrentEffect;

    /// <summary>
    /// 初期化処理です。
    /// </summary>
    private void Awake()
    {
        if (bool_UsePool)
        {
            cs_EffectPool = new CS_EffectPool(
                go_EffectPrefab,
                transform);
        }
    }

    /// <summary>
    /// Effectを再生します。
    /// </summary>
    /// <param name="csst_EffectPlayData">Effect再生データ。</param>
    /// <returns>再生したEffect。</returns>
    public CSAD_EffectCommonProcessBase PlayEffect(CSST_EffectPlayData csst_EffectPlayData)
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

        if (bool_UsePool)
        {
            if (cs_EffectPool == null)
            {
                cs_EffectPool = new CS_EffectPool(
                    go_EffectPrefab,
                    transform);
            }

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
