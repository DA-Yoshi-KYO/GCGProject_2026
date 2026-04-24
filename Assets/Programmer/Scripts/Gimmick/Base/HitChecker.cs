// == HitChecker.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/24 作成開始
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static CriWare.CriAtomExMic;

public class HitChecker : MonoBehaviour
{
    [Header("命中範囲")]
    public BoxCollider hit;
    [Header("効果範囲")]
    public BoxCollider effect;
    [Header("敵のレイヤー")]
    public LayerMask enemyLayer;

    private bool isLoop = false;
    private bool firstUpdate = true;
    private int hitDamage = 0;
    private int effectDamage = 0;

    /// <summary>
    /// 当たり判定の処理をループさせるかどうか
    /// </summary>
    /// <param name="IsLoop">ループさせるかどうか</param>
    public void HitLoop(bool IsLoop)
    {
        isLoop = IsLoop;
    }

    /// <summary>
    /// 命中範囲内の敵を検出する関数
    /// </summary>
    /// <returns></returns>
    public Collider[] GetHitEnemies()
    {
        return OverlapBoxCollider(hit);
    }

    /// <summary>
    /// 効果範囲内の敵を検出する関数
    /// </summary>
    /// <returns></returns>
    public Collider[] GetEffectEnemies()
    {
        return OverlapBoxCollider(effect);
    }

    /// <summary>
    /// BoxColliderを使用して、命中範囲内の敵を検出する関数
    /// </summary>
    /// <param name="box">検出範囲のBoxCollider</param>
    /// <returns>検出された敵のコライダー配列</returns>
    private Collider[] OverlapBoxCollider(BoxCollider box)
    {
        if (box == null) return new Collider[0];

        // コライダーのワールド座標でのCenter・Size・回転を取得
        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 worldHalfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
        Quaternion worldRotation = box.transform.rotation;

        return Physics.OverlapBox(worldCenter, worldHalfExtents, worldRotation, enemyLayer);
    }

    /// <summary>
    /// 命中範囲内の敵に与えるダメージを設定する関数
    /// </summary>
    /// <param name="damage"></param>
    public void SetHitDamage(int damage)
    {
        hitDamage = damage;
    }

    /// <summary>
    /// 効果範囲内の敵に与えるダメージを設定する関数
    /// </summary>
    /// <param name="damage"></param>
    public void SetEffectDamage(int damage)
    {
        effectDamage = damage;
    }

    private void FixedUpdate()
    {
        if(firstUpdate || isLoop)
        {
            firstUpdate = false;

            Collider[] hitEnemies = GetHitEnemies();
            Collider[] effectEnemies = GetEffectEnemies();


            for(int i = 0; i < effectEnemies.Length; i++)
            {
                for(int j = 0; j < hitEnemies.Length; j++)
                {
                    // 効果範囲内のみの敵に対する処理
                    if (effectEnemies[i] != hitEnemies[j])
                    {
                        GameObject enemy = effectEnemies[i].gameObject;
                        ThiefAI thiefAI = enemy.GetComponent<ThiefAI>();
                        if (thiefAI != null)
                        {
                            thiefAI.TakeDamage(effectDamage);
                        }
                    }
                }
            }
            // 命中範囲内の敵に対する処理
            for (int i = 0 ; i < hitEnemies.Length ; i++)
            {
                GameObject enemy = hitEnemies[i].gameObject;
                ThiefAI thiefAI = enemy.GetComponent<ThiefAI>();
                if (thiefAI != null)
                {
                    thiefAI.TakeDamage(hitDamage);
                }
            }
        }
    }
}
