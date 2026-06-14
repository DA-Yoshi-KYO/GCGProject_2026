using UnityEngine;

/*
+=====================================
 ファイル名 : CS_GimmickHitEffectPlayer.cs
 概要     : ギミック命中時のHitEffect再生クラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/11 新規作成
=====================================+
*/

/// <summary>
/// ギミック命中時のHitEffect再生を担当するクラスです。
/// HitCheckerからEffect再生処理を分離します。
/// </summary>
public class CS_GimmickHitEffectPlayer : MonoBehaviour
{
    private const string HIT_EFFECT_ROOT_NAME = "GimmickHitEffectRoot";

    [Header("ギミック命中時に再生するHitEffectPrefab")]
    [SerializeField]
    private GameObject go_HitEffectPrefab;

    [Header("HitEffectの少し上げる量")]
    [SerializeField]
    private float f_EffectUpOffset = 0.05f;

    private Transform tr_HitEffectRoot;

    /// <summary>
    /// HitEffectを再生します。
    /// </summary>
    /// <param name="enemyCollider">当たった敵Collider。</param>
    /// <param name="hitBox">当たり判定Box。</param>
    public void PlayHitEffect(Collider enemyCollider, BoxCollider hitBox)
    {
        if (go_HitEffectPrefab == null)
        {
            return;
        }

        if (enemyCollider == null || hitBox == null)
        {
            return;
        }

        Vector3 v3_EffectPosition =
            CalculateHitEffectPosition(enemyCollider, hitBox);

        Quaternion q_EffectRotation =
            go_HitEffectPrefab.transform.rotation;

        CSAD_EffectCommonProcessBase csad_Effect =
            CS_EffectFactory.CreateEffect(
                go_HitEffectPrefab,
                v3_EffectPosition,
                q_EffectRotation,
                GetHitEffectRoot());

        if (csad_Effect == null)
        {
            return;
        }

        csad_Effect.SetOnEffectEndAction(DestroyEffect);

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(v3_EffectPosition);
        csst_EffectPlayData.SetRotation(q_EffectRotation);

        csad_Effect.PlayEffect(csst_EffectPlayData);
    }

    /// <summary>
    /// OverlapBox判定用のHitEffect位置を計算します。
    /// </summary>
    /// <param name="enemyCollider">敵Collider。</param>
    /// <param name="hitBox">ギミック側判定Box。</param>
    /// <returns>HitEffect再生位置。</returns>
    private Vector3 CalculateHitEffectPosition(Collider enemyCollider, BoxCollider hitBox)
    {
        Vector3 v3_HitBoxCenter =
            hitBox.transform.TransformPoint(hitBox.center);

        Vector3 v3_EnemyClosestPoint =
            enemyCollider.ClosestPoint(v3_HitBoxCenter);

        Vector3 v3_HitBoxClosestPoint =
            hitBox.ClosestPoint(v3_EnemyClosestPoint);

        Vector3 v3_EffectPosition =
            (v3_EnemyClosestPoint + v3_HitBoxClosestPoint) * 0.5f;

        v3_EffectPosition += Vector3.up * f_EffectUpOffset;

        return v3_EffectPosition;
    }

    /// <summary>
    /// HitEffect用Rootを取得します。
    /// </summary>
    /// <returns>HitEffect用Root。</returns>
    private Transform GetHitEffectRoot()
    {
        if (tr_HitEffectRoot != null)
        {
            return tr_HitEffectRoot;
        }

        GameObject go_Root = GameObject.Find(HIT_EFFECT_ROOT_NAME);

        if (go_Root == null)
        {
            go_Root = new GameObject(HIT_EFFECT_ROOT_NAME);
        }

        tr_HitEffectRoot = go_Root.transform;

        return tr_HitEffectRoot;
    }

    /// <summary>
    /// Effect終了時に破棄します。
    /// </summary>
    /// <param name="csad_Effect">終了したEffect。</param>
    private void DestroyEffect(CSAD_EffectCommonProcessBase csad_Effect)
    {
        if (csad_Effect == null)
        {
            return;
        }

        Destroy(csad_Effect.gameObject);
    }
}
