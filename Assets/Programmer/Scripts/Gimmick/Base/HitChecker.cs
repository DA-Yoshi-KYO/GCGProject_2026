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
    [Header("索敵範囲")]
    public BoxCollider search;
    [Header("敵のレイヤー")]
    public LayerMask enemyLayer;

    // ギミック命中時のHitEffect再生クラス
    private CS_GimmickHitEffectPlayer cs_GimmickHitEffectPlayer;

    private bool isLoop = false;
    private bool firstUpdate = true;
    private int hitDamage = 0;
    private int effectDamage = 0;
    private Gimmick gimmick;
    private CS_ThiefGimmickAction thiefGA;
    GameObject parentGameObject;

    // 既にダメージを与えた敵を保存
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();
    private void Awake()
    {
        cs_GimmickHitEffectPlayer = GetComponent<CS_GimmickHitEffectPlayer>();
    }

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

    // 索敵範囲の設定
    public Collider[] GetSearchEnemies()
    {
        return OverlapBoxCollider(search);
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

    /// <summary>
    /// ギミック情報を設定する関数
    /// </summary>
    /// <param name="gimmick"></param>
    public void SetGimmick(Gimmick gimmick)
    {
        this.gimmick = gimmick;
    }

    /// <summary>
    /// 召喚もとのギミックのGameObjectを設定する関数
    /// </summary>
    /// <param name="parentGameObject"></param>
    public void SetParentGameObject(GameObject parentGameObject)
    {
        this.parentGameObject = parentGameObject;
    }

    /// <summary>
    /// Enemyにダメージを与える関数
    /// </summary>
    /// <param name="enemy"></param>
    /// <param name="damage"></param>
    /// <param name="isHit"></param>
    private void EnemyDame(
        GameObject enemy,
        int damage,
        bool isHit = true,
        Collider enemyCollider = null,
        BoxCollider hitBox = null)
    {
        // =====================================================
        // 一度ダメージを与えた敵には再度当てない
        // =====================================================
        if (damagedEnemies.Contains(enemy))
        {
            return;
        }

        CS_ThiefAI thiefAI = enemy.GetComponent<CS_ThiefAI>();

        if (thiefAI != null)
        {
            thiefAI.TakeDamage(damage, gimmick, transform.position, isHit);

            if (cs_GimmickHitEffectPlayer != null)
            {
                cs_GimmickHitEffectPlayer.PlayHitEffect(
                    enemyCollider,
                    hitBox);
            }

            // ダメージ済み登録
            damagedEnemies.Add(enemy);
        }
    }

    private void EnemyCharm(GameObject enemy)
    {
        CS_ThiefAI thiefAI = enemy.GetComponent<CS_ThiefAI>();
        if (thiefAI != null)
        {
            CS_TrapTarget trapTarget = parentGameObject.GetComponent<CS_TrapTarget>();
            if (trapTarget != null)
            {
                thiefAI.read_MemorySystem.SetTarget(trapTarget);
            }
        }
    }

    private void FixedUpdate()
    {
        if (firstUpdate || isLoop)
        {
            firstUpdate = false;

            Collider[] hitEnemies =
                GetHitEnemies();

            Collider[] effectEnemies =
                GetEffectEnemies();

            Collider[] searchEnemies =
                GetSearchEnemies();

            //======================================================
            // 索敵範囲
            //======================================================
            for (int i = 0 ; i < searchEnemies.Length ; i++)
            {
                GameObject enemy = searchEnemies[i].gameObject;
                CS_ThiefAI thiefAI = enemy.GetComponent<CS_ThiefAI>();
                thiefGA = thiefAI.read_ThiefGimmickAction;
                GimmickBase gimmickBase = parentGameObject.GetComponent<GimmickBase>();
                switch (gimmick)
                {
                    case Gimmick.IronBall:
                        //thiefGA.IronBallStart(gimmickBase);
                        //Debug.Log("泥棒逃げる！");
                        break;
                }
            }

            // =====================================================
            // 効果範囲
            // =====================================================
            for (int i = 0 ; i < effectEnemies.Length ; i++)
            {
                bool isHitEnemy = false;

                // hitEnemies に含まれているか確認
                for (int j = 0 ; j < hitEnemies.Length ; j++)
                {
                    if (effectEnemies[i] == hitEnemies[j])
                    {
                        isHitEnemy = true;
                        break;
                    }
                }

                // hit範囲にいない敵のみ
                if (!isHitEnemy)
                {
                    GameObject enemy =
                        effectEnemies[i].gameObject;
                    CS_ThiefAI thiefAI = enemy.GetComponent<CS_ThiefAI>();
                    thiefGA = thiefAI.read_ThiefGimmickAction;
                    if (thiefAI != null)
                    {
                        thiefGA = thiefAI.read_ThiefGimmickAction;
                    }
                    GimmickBase gimmickBase = parentGameObject.GetComponent<GimmickBase>();
                    switch (gimmick)
                    {
                        case Gimmick.Pitfall:
                            thiefGA.PitFallStart(gimmickBase.transform.position);
                            break;
                    }

                    switch (gimmick)
                    {
                        case Gimmick.Pot:
                            EnemyDame(enemy, effectDamage, false, effectEnemies[i], effect);
                            break;
                        case Gimmick.IronBall:
                            EnemyDame(enemy, effectDamage, false, effectEnemies[i], effect);
                            break;
                        case Gimmick.EmptyChest:
                            EnemyCharm(enemy);
                            break;
                        case Gimmick.Nyaki:
                            EnemyDame(enemy, effectDamage, false, effectEnemies[i], effect);
                            break;
                        case Gimmick.Pitfall:
                            Debug.Log("Pitfall hit effect");
                            EnemyDame(enemy, effectDamage, false, effectEnemies[i], effect);
                            //シーフ落とす関数追加する//
                            parentGameObject.GetComponent<PitfallGimmick>();
                            parentGameObject.GetComponent<PitfallGimmick>().gimmickState = GimmickState.Active;
                            break;
                    }
                }
            }

            // =====================================================
            // 命中範囲
            // =====================================================
            for (int i = 0 ; i < hitEnemies.Length ; i++)
            {
                GameObject enemy = hitEnemies[i].gameObject;
                CS_ThiefAI thiefAI = enemy.GetComponent<CS_ThiefAI>();
                thiefGA = thiefAI.read_ThiefGimmickAction;
                GimmickBase gimmickBase = parentGameObject.GetComponent<GimmickBase>();
                switch (gimmick)
                {
                    case Gimmick.Pitfall:
                        thiefGA.PitFallStart(gimmickBase.transform.position);
                        break;
                }

                if (thiefAI != null)
                {
                    switch (gimmick)
                    {
                        case Gimmick.Pot:
                            EnemyDame(enemy, hitDamage, true, hitEnemies[i], hit);
                            break;
                        case Gimmick.IronBall:
                            EnemyDame(enemy, hitDamage, true, hitEnemies[i], hit);
                            break;
                        case Gimmick.EmptyChest:
                            EnemyCharm(enemy);
                            EmptyChestGimmick emptyChestGimmick =
                                parentGameObject.GetComponent<EmptyChestGimmick>();
                            if (emptyChestGimmick != null)
                            {
                                emptyChestGimmick.Durability_Value_Decreased();

                                Debug.Log("Durability decreased");
                            }
                            break;
                        case Gimmick.Nyaki:
                            EnemyDame(enemy, hitDamage, true, hitEnemies[i], hit);
                            break;
                        case Gimmick.Pitfall:
                            Debug.Log("Pitfall hit enemy");
                            //シーフ落とす関数追加する//
                            thiefGA.PitFallStart(transform.position);
                            parentGameObject.GetComponent<PitfallGimmick>();
                            parentGameObject.GetComponent<PitfallGimmick>().gimmickState = GimmickState.Active;
                            break;
                    }
                }
            }
        }
    }
    public CS_ThiefGimmickAction GetThiefGA()
    {
        return thiefGA;
    }
}
