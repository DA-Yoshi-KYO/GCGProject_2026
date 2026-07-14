// == HitChecker.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/24 作成開始
using System.Collections.Generic;
using UnityEngine;

public enum HitTargetType
{
    Enemy,
    Player,
}

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
        return OverlapBoxCollider(hit, HitTargetType.Enemy);
    }

    /// <summary>
    /// 効果範囲内の敵を検出する関数
    /// </summary>
    /// <returns></returns>
    public Collider[] GetEffectEnemies()
    {
        return OverlapBoxCollider(effect, HitTargetType.Enemy);
    }

    // 索敵範囲の設定
    public Collider[] GetSearchEnemies()
    {
        return OverlapBoxCollider(search, HitTargetType.Enemy);
    }

    public Collider[] GetHitObject(HitTargetType targetType)
    {
        return OverlapBoxCollider(hit, targetType);
    }

    public Collider[] GetEffectObject(HitTargetType targetType)
    {
        return OverlapBoxCollider(effect, targetType);
    }

    private Collider[] OverlapBoxCollider(BoxCollider box, HitTargetType targetType)
    {
        if (box == null)
            return System.Array.Empty<Collider>();

        Vector3 worldCenter =
            box.transform.TransformPoint(box.center);

        Vector3 worldHalfExtents =
            Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);

        Quaternion worldRotation =
            box.transform.rotation;

        switch (targetType)
        {
            //敵:::::::::::::::::
            case HitTargetType.Enemy:
                return Physics.OverlapBox(
                    worldCenter,
                    worldHalfExtents,
                    worldRotation,
                    enemyLayer);
            //プレイヤー:::::::::
            case HitTargetType.Player:
                    Collider[] hitColliders =
                        Physics.OverlapBox(
                            worldCenter,
                            worldHalfExtents,
                            worldRotation);

                    foreach (Collider hitCollider in hitColliders)
                    {
                        Transform root =
                            hitCollider.transform.root;

                        if (root.CompareTag("Player"))
                        {
                            return new Collider[] { hitCollider };
                        }
                    }
                    break;
            //それ以外:::::::::::
            default:
                return System.Array.Empty<Collider>();
        }
        return System.Array.Empty<Collider>();
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
            Collider[] hitPlayer = 
                GetHitObject(HitTargetType.Player);
            Collider[] effectPlayer = 
                GetEffectObject(HitTargetType.Player);

            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // プレイヤーの判定

            //==================================================
            // 命中範囲
            //==================================================
            for (int i = 0 ; i < hitPlayer.Length ; i++)
            {
                Transform root =
                    hitPlayer[i].transform.root;

                if (!root.CompareTag("Player"))
                    continue;

                CS_PlayerMove playerMove =
                    root.GetComponent<CS_PlayerMove>();

                if (playerMove == null)
                    continue;

                switch (gimmick)
                {
                    case Gimmick.MagicAnkh:
                        playerMove.SetAnkhCatStunTime(5.0f);
                        break;
                }

                break;
            }
            //==================================================
            // 効果範囲
            //==================================================
            for (int i = 0 ; i < effectPlayer.Length ; i++)
            {
                bool isInHitRange = false;

                for (int j = 0 ; j < hitPlayer.Length ; j++)
                {
                    if (effectPlayer[i].transform.root ==
                        hitPlayer[j].transform.root)
                    {
                        isInHitRange = true;
                        break;
                    }
                }

                if (isInHitRange)
                    continue;

                Transform root =
                    effectPlayer[i].transform.root;

                if (!root.CompareTag("Player"))
                    continue;

                CS_PlayerMove playerMove =
                    root.GetComponent<CS_PlayerMove>();

                if (playerMove == null)
                    continue;

                switch (gimmick)
                {
                    case Gimmick.MagicAnkh:
                        playerMove.SetAnkhCatStunTime(5.0f);
                        break;
                }

                break;
            }

            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // 敵の判定

            //======================================================
            // 索敵範囲
            //======================================================
            for (int i = 0 ; i < searchEnemies.Length ; i++)
            {
                GameObject enemy = searchEnemies[i].gameObject;
                CS_ThiefAI thiefAI = enemy.GetComponent<CS_ThiefAI>();
                if (thiefAI == null) continue;
                thiefGA = thiefAI.read_ThiefGimmickAction;
                GimmickBase gimmickBase = parentGameObject.GetComponent<GimmickBase>();
                switch (gimmick)
                {
                    case Gimmick.IronBall:
                        thiefGA.IronBallStart(gimmickBase);
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
                    if (thiefAI == null) continue;
                    thiefGA = thiefAI.read_ThiefGimmickAction;
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
                            thiefAI.read_ThiefGimmickAction.EmptyChestStart(gimmickBase);
                            ((EmptyChestGimmick)gimmickBase).AddTargetThiefAI(thiefAI);
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
                        case Gimmick.MagicAnkh:
                            EnemyDame(enemy, effectDamage, false, effectEnemies[i], effect);
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
                if (thiefAI == null) continue;
                thiefGA = thiefAI.read_ThiefGimmickAction;
                GimmickBase gimmickBase = parentGameObject.GetComponent<GimmickBase>();

                if (thiefAI != null)
                {
                    switch (gimmick)
                    {
                        case Gimmick.Pot:
                            EnemyDame(enemy, hitDamage, true, hitEnemies[i], hit);
                            break;
                        case Gimmick.IronBall:
                            EnemyDame(enemy, hitDamage, true, hitEnemies[i], hit);
                            if(gimmickBase.GetGimmickSound() != null)
                            {
                                gimmickBase.GetGimmickSound().
                                    PlayOneShotSE(
                                    "Gimmick_RockHit",
                                    gimmickBase.transform.position,
                                    "RockSound");
                            }
                            break;
                        case Gimmick.EmptyChest:
                            EmptyChestGimmick emptyChestGimmick =
                                parentGameObject.GetComponent<EmptyChestGimmick>();
                            if (emptyChestGimmick != null && thiefAI.read_CurrentState != CS_ThiefAI.ThiefState.Stunned)
                            {
                                emptyChestGimmick.Durability_Value_Decreased();
                                thiefAI.read_ThiefReaction.ChangeReaction(CS_ThiefReaction.ThiefReactionType.Searching);

                                thiefAI?.read_AnimatorSystem?.ResetAnimationState();
                                thiefAI?.read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Hunting);

                                thiefAI.read_MoveSystem.Stop();

                                Debug.Log("Durability decreased");
                            }
                            break;
                        case Gimmick.Nyaki:
                            EnemyDame(enemy, hitDamage, true, hitEnemies[i], hit);
                            break;
                        case Gimmick.Pitfall:
                            Debug.Log("Pitfall hit enemy");
                            thiefGA.PitFallStart(transform.position);
                            parentGameObject.GetComponent<PitfallGimmick>();
                            parentGameObject.GetComponent<PitfallGimmick>().gimmickState = GimmickState.Active;
                            break;
                        case Gimmick.MagicAnkh:
                            EnemyDame(enemy, hitDamage, true, hitEnemies[i], hit);
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
