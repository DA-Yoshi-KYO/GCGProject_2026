/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のAIシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17   | 初回作成
 * 2026-04-20   | 探索対象の決定ロジックを追加
 *              | 探索対象の優先順位を追加
 * 2026-04-22   | 耐久値を減少させる処理を追加
 *              | NavMeshAgentを利用して移動する処理を追加
 * 2026-04-23   | 泥棒のデータベースの項目変更に合わせて、Settingメソッドの内容を変更
 *              | 走り状態になる標的オブジェクトのタイプに応じて、移動速度を切り替える処理を追加
 * 2026-04-24   | 探索対象を強制的に変更する処理を追加
 *              | 探索対象の決定ロジックを一つにまとめる(複数個所に分散していたものを、DecideTargetメソッドにまとめる)
 * 2026-04-25   | 次に設定する移動ポイントを決定するロジックの不具合を修正
 * 2026-04-26   | 気絶後の退場処理を仮作成
 *              | 宝物を持って移動する処理を仮作成
 * 2026-04-27   | 部屋移動の閾値に達していたら次の部屋に移動する処理を追加
 * 2026-04-28   | 次の部屋に移動するための移動ポイントを決定するロジックを追加
 * 2026-05-01   | 帰宅ルートを構築するロジックを追加
 * 2026-05-07   | CS_RoomEnemyEntryPointDataを用いた初期部屋の設定の記載
 * 2026-05-08   | 初期部屋の入ってきたドアの位置を保存する処理の記載
 * 2026-05-15   | 同じ部屋の中で、他者が探索しているオブジェクトを探索対象にしないようにする処理を追加
 * 2026-05-17   | DecideTarget内のキャストエラーの不具合を修正
 * 2026-05-18   | A*アルゴリズムを用いて帰宅ルートを構築するロジックを追加
 * 2026-05-21   | CS_SmartNavAgentを用いた危険地帯を考慮した移動処理の追加
 * 2026-05-22   | ファイル名を変更（ThiefAI.cs → CS_ThiefAI.cs）
 *              | クラス名を変更（ThiefAI → CS_ThiefAI）
 *              | リアクション処理を管理するコンポーネントを追加（CS_ThiefReaction）
 * 2026-05-28   | 大解体
 *              | 以下の要素を管理するクラスを新たに作成して、CS_ThiefAIから処理を移動させる。
 *              | 移動処理を管理するクラス(CS_MoveSystem)
 *              | 記憶処理を管理するクラス(CS_MemorySystem)
 *              | 聴覚処理を管理するクラス(CS_HearingSystem)
 *              | 視覚処理を管理するクラス(CS_VisionSensor)
 *              | A*アルゴリズムを用いたルート構築処理を管理するクラス(CS_AStarSystem)
 * 2026-06-04   | 気絶したときの処理をCS_StunThiefTargetに移動
 * 2026-06-10   | 落とし穴ギミックにハマったときの処理を追加
 *              | 落とし穴ギミックから抜けたときの処理を追加
 *              | ナビメッシュエージェントを安全に停止させる処理を追加
 * 2026-06-11   | 落とし穴用の処理をCS_ThiefGimmickActionに移動
 *              | 気絶状態の更新処理を実行するかどうかを設定する処理を追加
 * 2026-06-12   | 退場時のフェードアウト処理を追加
 * 2026-06-19   | 泥棒が宝物を落としたときに近くのグリッドに配置される処理の実装
 * 2026-07-06   | 泥棒が宝物を落としたときグリッド上に正しく落ちない不具合の修正
 *              | read_の変数すべてにNullチェックを追加
 *              | 宝物を落としたときに宙に浮くバグの修正
 *              | update処理に体力が0の時の状態設定を追加
 * 2026-07-07   | 泥棒が宝物を持ち帰った後再び帰ってくる処理の実装
 *              | 泥棒の終了時のエラー修正
 * 2026-07-10   | 宝物をもってスタンをあけたとき再び探索をするバグの修正
 * 
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

/// <summary>
/// 泥棒の行動を管理するクラスです。
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CS_ThiefAI : MonoBehaviour
{
    [Tooltip("泥棒の行動状態を定義する列挙型")]
    public enum ThiefState
    {
        [Tooltip("探索状態")]
        Explore,
        [Tooltip("発見状態")]
        Found,
        [Tooltip("逃走状態")]
        Escape,
        [Tooltip("気絶状態")]
        Stunned
    }
    [Tooltip("現在の行動状態")]
    private ThiefState currentState;
    public ThiefState read_CurrentState => currentState;

    [Tooltip("泥棒のマテリアル")]
    private Material[] thiefMaterial;
    [Tooltip("泥棒のマテリアルのフェードアウトにかかる時間")]
    private float fadeAfterStunTime;

    [Serializable]
    public struct ThiefMaterials
    {
        public Material materialA;
        public Material materialB;
        public Material materialC;
    }
    [SerializeField, Tooltip("泥棒のカラーバリエーションリスト")]
    private List<ThiefMaterials> thiefMaterialsList = new List<ThiefMaterials>();

    [Tooltip("使用したデータベース")]
    private CO_ThiefStatusData thiefStatusData;

    [Tooltip("アウトラインターゲット")]
    private CS_OutlineTarget outlineTarget;

    [Tooltip("泥棒の耐久力")]
    private int durability;
    public int read_Durability => durability;

    [Tooltip("泥棒の最大耐久力")]
    private int maxDurability;
    public int read_MaxDurability => maxDurability;

    [Tooltip("持っている宝物オジェクト")]// 見つけたら設定する
    private GameObject holdTreasure;

    [Tooltip("攻撃を受けた後の気絶時間")]
    private float damageStunTime;

    [Tooltip("攻撃を受けた後の無敵時間")]
    private float invincibleTime;
    [Tooltip("無敵時間の現在残り時間")]
    private float remainingInvincibleTime;
    public float read_RemainingInvincibleTime => remainingInvincibleTime;

    [Tooltip("気絶後に退場するまでの時間")]
    private int exitAfterStunTime;
    [Tooltip("気絶後の経過時間")]
    private float elapsedTimeAfterStun;
    [Tooltip("気絶状態の更新処理を実行するかどうか")]
    private bool isUpdatingStunState = true;

    [Tooltip("泥棒のリアクションUIを管理するコンポーネント")]
    private CS_ThiefReactionUI thiefReactionUI;

    [Tooltip("泥棒のリアクションを管理するコンポーネント")]
    private CS_ThiefReaction thiefReaction;
    public CS_ThiefReaction read_ThiefReaction => thiefReaction;

    [Tooltip("探索完了とする距離")]
    private const float exploredDistanceThreshold = 1.5f;
    private const float exploredDistanceThresholdMovePoint = 1.0f;
    public float read_ExploredDistanceThreshold => exploredDistanceThreshold;
    public float read_ExploredDistanceThresholdMovePoint => exploredDistanceThresholdMovePoint;

    [Tooltip("泥棒関係のサウンド")]
    private CS_3DPlaySE thiefSound;
    public CS_3DPlaySE read_ThiefSound 
    {
        get 
        {
            if (thiefSound == null)
            {
                thiefSound = GameObject.Find("AudioManager").GetComponentInChildren<CS_3DPlaySE>();
            }
            if (thiefSound == null) Debug.LogWarning("【泥棒】CS_3DPlaySEコンポーネントが見つかりません。サウンドが再生されません。");

            return thiefSound;
        } 
    }

    [SerializeField, Tooltip("視界に入る対象のレイヤー"), Header("視界に入る対象のレイヤー")]
    private List<LayerMask> targetLayer;
    [SerializeField, Tooltip("障害物のレイヤー"), Header("障害物のレイヤー")]
    private LayerMask obstacleLayer;

    [Tooltip("猫を捕まえているときの残り時間")]
    private float remainingHoldCatTime = 0.0f;
    [Tooltip("猫を捕まえている時間の初期値")]
    private float initholdCatTime = 0.0f;
    public float read_RemainingHoldCatTime => remainingHoldCatTime;

    [Tooltip("アニメーション用")]
    private CS_ThiefAnimation animatorSystem;
    public CS_ThiefAnimation read_AnimatorSystem
    {
        get
        {
            if (animatorSystem == null)
            {
                Animator getAnimator = GetComponentInChildren<Animator>();
                animatorSystem = new CS_ThiefAnimation(this, getAnimator);
            }
            if (animatorSystem == null) Debug.LogWarning("【泥棒】AnimatorSystemが見つかりません。アニメーションが再生されません。");

            return animatorSystem;
        }
    }

    [Tooltip("種類アイコン")]
    private Sprite iconSprite;
    public Sprite read_IconSprite => iconSprite;

    [Tooltip("移動システム")]
    private CS_MoveSystem moveSystem;
    public CS_MoveSystem read_MoveSystem
    {
        get
        {
            if (moveSystem == null)
            {
                Debug.LogWarning("【泥棒】CS_MoveSystemが見つかりません。移動処理が正しく動作しません。");
            }
            return moveSystem;
        }
    }

    [Tooltip("記憶システム")]
    private CS_MemorySystem memorySystem;
    public CS_MemorySystem read_MemorySystem
    {
        get
        {
            if (memorySystem == null)
            {
                Debug.LogWarning("【泥棒】CS_MemorySystemが見つかりません。記憶処理が正しく動作しません。");
            }
            return memorySystem;
        }
    }

    [Tooltip("聴覚システム")]
    private CS_HearingSystem hearingSystem;
    public CS_HearingSystem read_HearingSystem
    {
        get
        {
            if (hearingSystem == null)
            {
                Debug.LogWarning("【泥棒】CS_HearingSystemが見つかりません。聴覚処理が正しく動作しません。");
            }
            return hearingSystem;
        }
    }

    [Tooltip("視覚システム")]
    private CS_VisionSensor visionSensor;
    public CS_VisionSensor read_VisionSensor
    {
        get
        {
            if (visionSensor == null)
            {
                visionSensor = GetComponentInChildren<CS_VisionSensor>();
            }
            if (visionSensor == null) Debug.LogWarning("【泥棒】CS_VisionSensorが見つかりません。視覚処理が正しく動作しません。");
            return visionSensor;
        }
    }

    [Tooltip("A*アルゴリズムシステム")]
    private CS_AStarSystem aStarSystem;
    public CS_AStarSystem read_AStarSystem
    {
        get
        {
            if (aStarSystem == null)
            {
                Debug.LogWarning("【泥棒】CS_AStarSystemが見つかりません。A*アルゴリズム処理が正しく動作しません。");
            }
            return aStarSystem;
        }
    }

    [Tooltip("ギミック行動システム")]
    private CS_ThiefGimmickAction thiefGimmickAction;
    public CS_ThiefGimmickAction read_ThiefGimmickAction
    {
        get
        {
            if (thiefGimmickAction == null)
            {
                Debug.LogWarning("【泥棒】CS_ThiefGimmickActionが見つかりません。ギミック行動処理が正しく動作しません。");
            }
            return thiefGimmickAction;
        }
    }

    [Header("猫拘束中Effect")]
    [SerializeField]
    private GameObject go_PettingEffectPrefab;

    [Header("猫拘束中Effectの表示位置Offset")]
    [SerializeField]
    private Vector3 v3_PettingEffectOffset = new Vector3(0.0f, 1.2f, 0.0f);

    /// <summary>
    /// 再生中の猫拘束Effectです。
    /// </summary>
    private CSAD_EffectCommonProcessBase csad_PettingEffect;

    /// <summary>
    /// 泥棒のステータスを設定する処理
    /// </summary>
    /// <param name="typedata">泥棒のタイプごとのステータスデータ</param>
    /// <param name="data">泥棒の共通ステータスデータ</param>
    /// <param name="playerSpeed">プレイヤーの移動速度（泥棒の移動速度をプレイヤーの移動速度に対する倍率で設定するため）</param>
    /// <param name="entryRoom">最初に入ってくる部屋のオブジェクト</param>
    /// <param name="entryPoint">最初に入ってくるドアの位置</param>
    public void Setting(CO_ThiefStatusData typedata, CO_ThiefCommonStatusData data, float playerSpeed, CS_RoomNode entryRoom, CSE_RoomDoorDirection doorDir, Transform entryPoint)
    {
        thiefStatusData = typedata;

        durability = typedata.durability;
        maxDurability = typedata.durability;

        // 移動システムの初期化
        moveSystem = new CS_MoveSystem(this, GetComponent<NavMeshAgent>(), GetComponent<CS_SmartNavAgent>(), typedata, playerSpeed);

        // 聴覚システムの初期化
        hearingSystem = new CS_HearingSystem(this);

        // 視覚システムの初期化
        visionSensor = GetComponentInChildren<CS_VisionSensor>();
        visionSensor.Setting(this, typedata, targetLayer, obstacleLayer);

        // A*アルゴリズムシステムの初期化
        aStarSystem = new CS_AStarSystem(this);

        // 記憶システムの初期化
        memorySystem = new CS_MemorySystem(this, entryRoom, doorDir, entryPoint, typedata, data);
        memorySystem.FindNowRoomNode();

        // ギミック行動システムの初期化
        thiefGimmickAction = new CS_ThiefGimmickAction(this);

        exitAfterStunTime = data.exitAfterStunTime;
        damageStunTime = data.stunTime;
        invincibleTime = data.invincibleTime;
        remainingInvincibleTime = 0.0f;

        // 猫を捕まえている時間の初期値を設定
        initholdCatTime = typedata.holdCatTime;

        // 初期状態を探索に設定
        currentState = ThiefState.Explore;

        // リジットボディの設定
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // ナビメッシュエージェントで移動させるため、リジットボディをキネマティックに設定
        rb.useGravity = false; // 重力の影響を受けないようにする

        // 泥棒のリアクションUIを管理するコンポーネントを取得
        thiefReactionUI = GameObject.FindAnyObjectByType<CS_ThiefReactionUI>();
        thiefReactionUI.RegisterReaction(data.reactionUISprites);

        // 泥棒のリアクションを管理するコンポーネントを取得
        thiefReaction = GetComponentInChildren<CS_ThiefReaction>();

        fadeAfterStunTime = data.fadeAfterStunTime;

        // マテリアルの取得
        SkinnedMeshRenderer skinnedMeshRenderer = transform.GetComponentInChildren<SkinnedMeshRenderer>();

        int colorIndex = UnityEngine.Random.Range(0, thiefMaterialsList.Count);
        Material[] newColorMaterials = new Material[] { thiefMaterialsList[colorIndex].materialA, thiefMaterialsList[colorIndex].materialB, thiefMaterialsList[colorIndex].materialC };
        // カラーバリエーションを適応
        skinnedMeshRenderer.materials = newColorMaterials;
        thiefMaterial = skinnedMeshRenderer.materials;

        iconSprite = typedata.thiefTypeIcon[colorIndex];

        // アウトラインターゲットの取得
        outlineTarget = GetComponentInChildren<CS_OutlineTarget>();

        // アニメーション用のコンポーネントを取得
        Animator getAnimator = GetComponentInChildren<Animator>();
        animatorSystem = new CS_ThiefAnimation(this, getAnimator);
        animatorSystem.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Walk);

        // サウンドマネージャーから泥棒のサウンドを管理するコンポーネントを取得
        GameObject soundManager = GameObject.Find("AudioManager");
        // 子オブジェクトからCS_3DPlaySEコンポーネントを取得
        if (soundManager != null) thiefSound = soundManager.GetComponentInChildren<CS_3DPlaySE>();
        if (thiefSound == null) Debug.LogWarning("CS_3DPlaySEコンポーネントが見つかりません。サウンドが再生されません。");
    }

    private void Update()
    {
        // 初回行動に入る前は当たり判定が無効なのでオンにする
        if (!GetComponent<Collider>().enabled)
        {
            GetComponent<Collider>().enabled = true;
        }

        // 無敵時間の経過を管理
        // 気絶状態のときは無敵時間の経過を管理しない（気絶状態のときは攻撃を受けない想定のため）
        if (remainingInvincibleTime > 0)
        {
            float sineValue = Mathf.Abs(Mathf.Sin(remainingInvincibleTime * 720f * Mathf.Deg2Rad));
            float mappedValue = Mathf.Lerp(0.5f, 1f, sineValue);
            if (currentState != ThiefState.Stunned)
            {
                foreach (var material in thiefMaterial)
                {
                    material.SetFloat("_Alpha", mappedValue);
                }

                remainingInvincibleTime -= Time.deltaTime;
                if (remainingInvincibleTime < 0)
                {
                    remainingInvincibleTime = 0;
                    foreach (var material in thiefMaterial)
                    {
                        material.SetFloat("_Alpha", 1.0f);
                    }
                }
            }
        }

        // 猫を捕まえているときの経過時間を管理
        if (remainingHoldCatTime > 0.0f)
        {
            remainingHoldCatTime -= Time.deltaTime;
              moveSystem.Stop();
            if (remainingHoldCatTime < 0.0f)
            {
                remainingHoldCatTime = 0.0f;

                EndPettingEffect();

                memorySystem.ClearTarget();
            }
            return;
        }

        // 耐久値が0以下になった場合は、耐久値を0に補正して気絶状態にする
        if (durability <= 0)
        {
            durability = 0;
            ChangeStatus(ThiefState.Stunned);
        }

        // 現在の状態に応じた行動を実行
        switch (currentState)
        {
            case ThiefState.Explore:
                Explore();
                break;
            case ThiefState.Found:
                Found();
                break;
            case ThiefState.Escape:
                Escape();
                break;
            case ThiefState.Stunned:
                Stunned();
                break;
        }

        moveSystem.DebugMove();
    }

    private void OnDestroy()
    {
        // 生きている状態で退場した場合
        if (durability > 0)
        {
            CS_ThiefManager thiefManager = GameObject.FindObjectOfType<CS_ThiefManager>();

            if (thiefManager != null && memorySystem != null && memorySystem.read_CurrentRoom != null && memorySystem.read_CurrentRoom.transform != null && memorySystem.read_CurrentRoom.transform.parent != null)
            {
                thiefManager.RegistGenerationInfo(thiefStatusData, memorySystem.read_CurrentRoom.transform.parent.name, memorySystem.read_FirstEntryDirection);
            }
        }
        else
        {
            // 退場したときにウェーブ数を増加させる
            CS_StageManager stageManager = GameObject.FindObjectOfType<CS_StageManager>();
            if (stageManager != null) stageManager.WaveCountUp();
        }
    }

    // 探索状態の行動
    private void Explore()
    {
        thiefReaction.ClearReaction();

        if (thiefGimmickAction == null)
        {
            thiefGimmickAction = new CS_ThiefGimmickAction(this);
        }
        if(thiefGimmickAction.UpdateAction()) return;

        // メモリがnullだった場合
        if (memorySystem == null)
        {
            Debug.LogWarning(this.transform.name);
        }

        // 探索対象を決定
        memorySystem.RecognizeObjects();

        // 探索完了判定
        memorySystem.EvaluateCurrentTarget();

        // 体力が1以上残っている場合は、プレイヤーを発見してからの猶予時間を更新する
        if (durability > 1)memorySystem.UpdateFindPlayerGraceTime();
    }

    // 発見状態の行動
    private void Found()
    {
        // 探索対象が存在しない場合は何もしない
        if (memorySystem.read_CurrentTarget == null)
        {
            Debug.LogError("Found()が呼ばれましたが、探索対象が存在しません。");
            ChangeStatus(ThiefState.Explore);
            return;
        }

        // 宝物を持つ
        holdTreasure = memorySystem.read_CurrentTarget.gameObject;
        // 泥棒の子オブジェクトにする
        holdTreasure.transform.parent = this.transform;
        // 宝物のコライダーを無効にする
        Collider holdTreasureCollider = holdTreasure.GetComponent<Collider>();
        if (holdTreasureCollider != null) holdTreasureCollider.enabled = false;

        // 宝物のサイズを半分にする
        holdTreasure.transform.localScale *= 0.5f;

        //-- 体の前に持つ位置をローカル座標で設定
        // 泥棒の正面0.5m、下0.5mの位置に宝物を配置します。
        holdTreasure.transform.localPosition = new Vector3(0f, -0.5f, 0.5f);

        CS_VisionTarget visionTarget = holdTreasure.GetComponent<CS_VisionTarget>();
        if (visionTarget != null) visionTarget.PlayStolen(this);

        // 状態を逃走に変更
        ChangeStatus(ThiefState.Escape);

        // 取得した宝物を他の泥棒の記憶から消去する
        CS_ThiefManager thiefManager = GameObject.FindObjectOfType<CS_ThiefManager>();
        if (thiefManager != null)
        {
            thiefManager.EraseTheMemoryToAllThief(holdTreasure.GetComponent<CS_ThiefTarget>());
        }
        // 探索対象をリセット
        memorySystem.ClearTarget();
    }

    // 逃走状態の行動
    private void Escape()
    {
        // 宝物を見つけたときのリアクションに変更
        thiefReaction.ChangeReaction(CS_ThiefReaction.ThiefReactionType.FoundTreasure);

        // 帰宅ルートが未構築なら構築する
        if (!aStarSystem.HasRoute)
        {
            aStarSystem.ConstructionRoute(memorySystem.read_FirstEntryPoint);
        }

        // ルートを更新
        aStarSystem.UpdateRoute(exploredDistanceThreshold);

        // 最終目的地に十分近づいたら、退場する
        if (Vector3.Distance(transform.position, aStarSystem.GetTargetPoint()) < 1.0f)
        {
            GameObject Thief_BringTreature = GameObject.Find("Thief_BringTreature_" + transform.name);
            if (Thief_BringTreature == null)
                if (thiefSound != null)
                    thiefSound.PlayOneShotSE("Thief_BringTreature", gameObject.transform.position, "Thief_BringTreature_" + transform.name);
            Debug.Log("泥棒が退場しました。");
            Destroy(this.gameObject);
        }
    }

    // 気絶状態の行動
    private void Stunned()
    {
        // ナビメッシュエージェントを停止させる（安全に）
        moveSystem.Stop();

        // 経過時間を加算
        if(isUpdatingStunState) elapsedTimeAfterStun += Time.deltaTime;

        // 耐久値が残っている場合は、気絶時間が経過したら無敵時間を付与して、状態を探索に戻す
        if (durability > 0)
        {
            // 無敵表現
            // チカチカ処理
            float sineValue = Mathf.Abs(Mathf.Sin(elapsedTimeAfterStun * 720f * Mathf.Deg2Rad));
            float mappedValue = Mathf.Lerp(0.5f, 1f, sineValue);
            foreach (var material in thiefMaterial)
            {
                material.SetFloat("_Alpha", mappedValue);
            }

            // 経過時間が気絶時間を超えた場合は、状態を戻す
            if (elapsedTimeAfterStun >= damageStunTime)
            {
                if (holdTreasure == null)
                    ChangeStatus(ThiefState.Explore); // 状態を探索に戻す
                else
                    ChangeStatus(ThiefState.Escape); // 状態を逃走に戻す


                // アニメーションを歩き状態に設定
                read_AnimatorSystem?.ResetAnimationState();
                read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Walk);
            }
        }
        // 耐久力が0以下の場合は、時間経過後に退場する
        else
        {
            // 宝物を現在の部屋のオブジェクトに親子付けする
            if (holdTreasure != null)
            {
                // 宝物のコライダーを有効にする
                holdTreasure.GetComponent<Collider>().enabled = true;
                // 宝物のサイズを元に戻す
                holdTreasure.transform.localScale = Vector3.one;
                // 宝物を現在の部屋のオブジェクトに親子付けする
                holdTreasure.transform.SetParent(memorySystem.read_CurrentRoom.read_ObjectParent.transform);
                holdTreasure.GetComponent<CS_VisionTarget>().StopStolen();

                // 宝物を設置するグリッドを探す
                RoomGrid roomGrid = memorySystem.read_CurrentRoomPoint.GetComponentInChildren<RoomGrid>();
                Vector2Int startGridIndex = roomGrid.GetGridFromPos(transform.position);

                Vector3 gridPos = Vector3.zero;
                bool foundValidGrid = false;

                // 検索範囲を広げながら渦巻き状に探索
                for (int layer = 0; layer < Mathf.Max(roomGrid.read_GridDivision.x, roomGrid.read_GridDivision.y); layer++)
                {
                    // layer 0 は中心点のみ
                    if (layer == 0)
                    {
                        if (IsGridAvailable(startGridIndex, roomGrid, out gridPos))
                        {
                            foundValidGrid = true;
                            break;
                        }
                        continue;
                    }

                    // 現在のレイヤーのグリッドをチェック
                    for (int i = -layer; i <= layer; i++)
                    {
                        Vector2Int[] offsets = {
                            new Vector2Int(i, layer), // 上辺
                            new Vector2Int(i, -layer),// 下辺
                            new Vector2Int(layer, i), // 右辺
                            new Vector2Int(-layer, i) // 左辺
                        };

                        foreach (var offset in offsets)
                        {
                            Vector2Int checkIndex = startGridIndex + offset;
                            if (IsGridAvailable(checkIndex, roomGrid, out gridPos))
                            {
                                foundValidGrid = true;
                                break;
                            }
                        }
                        if (foundValidGrid) break;
                    }
                    if (foundValidGrid) break;
                }


                if (!foundValidGrid)
                {
                    Debug.LogWarning("有効なグリッドセルが見つからなかったため、宝物をデフォルト位置にドロップします。");
                    gridPos = transform.position; // デフォルトの位置として現在の位置を使用
                }


                GameObject Thief_DropTreatureHit = GameObject.Find("Thief_DropTreatureHit" + transform.name);
                if (Thief_DropTreatureHit == null)
                    if (thiefSound != null)
                        thiefSound.PlayOneShotSE("Thief_DropTreatureHit", gameObject.transform.position, "Thief_DropTreatureHit");

                // 宝物を設置する位置を設定
                Debug.LogWarning("宝物を設置する位置: " + gridPos);
                holdTreasure.transform.position = gridPos;
                holdTreasure = null;
            }
            // 経過時間が退場するまでの時間を超えた場合は、退場する処理を追加する
            if (elapsedTimeAfterStun >= exitAfterStunTime)
            {
                float fadeAmount = fadeAfterStunTime - (elapsedTimeAfterStun - exitAfterStunTime);

                foreach (Material mat in thiefMaterial)
                {
                    mat.SetFloat("_Alpha", fadeAmount);
                }

                outlineTarget.SetOutlineColor(new Color(1.0f, 0.0f, 0.0f, 0.0f));

                GameObject Thief_DeadFade = GameObject.Find("Thief_DeadFade_" + transform.name);
                if (Thief_DeadFade == null)
                    if (thiefSound != null)
                        thiefSound.PlayOneShotSE("Thief_DeadFade", gameObject.transform.position, "Thief_DeadFade_" + transform.name);

                read_AnimatorSystem?.ResetAnimationState();
                read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.RunAway);

                // 退場移動
                moveSystem.StunMove();

                if (fadeAmount <= 0.0f)
                {
                    Destroy(this.gameObject);
                }
            }
            else
            {
                thiefReaction.ClearReaction();
            }
        }
    }

    /// <summary>
    /// 耐久値を減らす処理
    /// </summary>
    /// <param name="damage">与える減少値</param>
    /// <param name="type">ギミックの種類</param>
    /// <param name="gimmickPoint">ギミックの位置</param>
    /// <param name="isHit">直接命中したかどうか</param>
    public void TakeDamage(int damage, Gimmick type, Vector3 gimmickPoint, bool isHit = true)
    {
        if (remainingInvincibleTime > 0) return;

        durability -= damage;

        // ※intのRangeはmin以上max未満の範囲でランダムな整数を返すため、1～3の範囲でランダムな整数を取得する場合は、Random.Range(1, 4)とする必要がある
        int soundIndex = UnityEngine.Random.Range(1, 4);
        if (thiefSound != null)
            thiefSound.PlayOneShotSE("Thief_Hit" + soundIndex, gameObject.transform.position, "Thief_Hit" + soundIndex);

        read_AnimatorSystem?.ResetAnimationState();
        switch (type)
        {
            case Gimmick.IronBall:
            case Gimmick.MagicAnkh:
            read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Damage);
                break;
            case Gimmick.Pot:
            case Gimmick.Pitfall:
            case Gimmick.Nyaki:
            read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Stunned);
                break;
        }

        // ギミックの方を向く
        Vector3 directionToGimmick = gimmickPoint - transform.position;
        directionToGimmick.y = 0; // 水平方向のみにする
        if (directionToGimmick != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToGimmick);
            transform.rotation = targetRotation;
        }

        // ダメージを受けたときのSEを再生する
        if (isHit)
        {
            // 直接ダメージを受けたときのリアクションに変更
            thiefReaction.ChangeReaction(CS_ThiefReaction.ThiefReactionType.HitTrap);
        }
        else
        {
            // 間接的にダメージを受けたときのリアクションに変更
            thiefReaction.ChangeReaction(CS_ThiefReaction.ThiefReactionType.NearHitTrap);
        }

        // プレイヤーの追跡情報をリセットする
        memorySystem.ResetIgnorePlayer();

        // リアクションUIを更新する
        thiefReactionUI.SetReactionUI();

        ChangeStatus(ThiefState.Stunned); // 状態を気絶に変更
        memorySystem.ClearTarget();
        elapsedTimeAfterStun = 0.0f; // 気絶時間の経過時間をリセット

        if (CS_ThiefDebugFlags.EnableInvincibilityAfterDamage)
        {
            remainingInvincibleTime = invincibleTime; // 無敵時間を付与
        }

        memorySystem.ResetIgnorePlayer(); // プレイヤーを無視する状態をリセット


        // 耐久力が0以下になった場合は、耐久力を0に補正して気絶状態にする
        if (durability <= 0)
        {
            durability = 0;

            GameObject Thief_Dead = GameObject.Find("Thief_Dead_" + transform.name);
            if (Thief_Dead == null)
                if (thiefSound != null)
                    thiefSound.PlayOneShotSE("Thief_Dead", gameObject.transform.position, "Thief_Dead");

            // StunThiefTargetに通知する
            GetComponent<CS_StunThiefTarget>().Notify();
        }
    }

    /// <summary>
    /// 現在の状態を変更する処理
    /// </summary>
    /// <param name="newState">変更する状態</param>
    public void ChangeStatus(ThiefState newState)
    {
        CS_VisionConeRenderer visionConeRenderer = GetComponentInChildren<CS_VisionConeRenderer>();
        switch (newState)
            {
            case ThiefState.Explore:
                // 探索状態になったときの処理を追加する
                visionConeRenderer.SetVisible(true);
                break;
            case ThiefState.Found:
                // 発見状態になったときの処理を追加する
                visionConeRenderer.SetVisible(false);
                break;
            case ThiefState.Escape:
                // 逃走状態になったときの処理を追加する
                visionConeRenderer.SetVisible(false);
                break;
            case ThiefState.Stunned:
                // 気絶時間の経過時間をリセット
                visionConeRenderer.SetVisible(false);

                if (currentState != ThiefState.Stunned)
                    elapsedTimeAfterStun = 0.0f;
                aStarSystem.ResetUpdatedFlag();
                break;
        }
        currentState = newState;
    }

    /// <summary>
    /// 猫を捕まえた処理
    /// </summary>
    public void CatchCat()
    {
        // 猫を捕まえている時間を設定
        remainingHoldCatTime = initholdCatTime;

        // 猫拘束中Effectを再生
        PlayPettingEffect();

        // 猫を捕まえているSEを再生する
        GameObject Thief_HitCat = GameObject.Find("Thief_HitCat_" + transform.name);
        if (Thief_HitCat == null)
            if (thiefSound != null)
                thiefSound.PlayOneShotSE("Thief_HitCat", gameObject.transform.position, "Thief_HitCat_" + transform.name);
    }

    /// <summary>
    /// 猫拘束中Effectを再生します。
    /// </summary>
    private void PlayPettingEffect()
    {
        if (go_PettingEffectPrefab == null)
        {
            return;
        }

        Vector3 v3_EffectPosition =
            transform.position + v3_PettingEffectOffset;

        Quaternion q_EffectRotation =
            go_PettingEffectPrefab.transform.rotation;

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();
        csst_EffectPlayData.SetPosition(v3_EffectPosition);
        csst_EffectPlayData.SetRotation(q_EffectRotation);

        if (csad_PettingEffect == null)
        {
            csad_PettingEffect = CS_EffectFactory.CreateEffect(
                go_PettingEffectPrefab,
                v3_EffectPosition,
                q_EffectRotation,
                transform);
        }

        if (csad_PettingEffect == null)
        {
            return;
        }

        csad_PettingEffect.PlayEffect(csst_EffectPlayData);
    }

    /// <summary>
    /// 猫拘束中Effectを終了します。
    /// </summary>
    private void EndPettingEffect()
    {
        if (csad_PettingEffect == null)
        {
            return;
        }

        csad_PettingEffect.EndEffect();
    }

    /// <summary>
    /// 気絶状態の更新処理を実行するかどうかを設定する処理
    /// </summary>
    /// <param name="isUpdating">気絶状態の更新処理を実行するかどうか</param>
    public void SetStunnedUpdateFlag(bool isUpdating)
    {
        isUpdatingStunState = isUpdating;
    }

    /// <summary>
    /// 指定されたグリッドが宝物を設置可能かどうかをチェックします。
    /// </summary>
    /// <param name="gridIndex">チェックするグリッドのインデックス</param>
    /// <param name="roomGrid">部屋のグリッド</param>
    /// <param name="worldPos">設置可能な場合のワールド座標</param>
    /// <returns>設置可能な場合はtrue</returns>
    private bool IsGridAvailable(Vector2Int gridIndex, RoomGrid roomGrid, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        // グリッドが範囲外かどうかをチェック
        if (gridIndex.x < 0 || gridIndex.x >= roomGrid.read_GridDivision.x ||
            gridIndex.y < 0 || gridIndex.y >= roomGrid.read_GridDivision.y)
        {
            return false;
        }

        // グリッドのワールド座標を取得
        Vector3 gridCenter = roomGrid.GetWorldPosFromGrid(gridIndex);

        // 高い位置から下にレイを飛ばして地面を検知
        const float rayOriginY = 255f;
        Ray ray = new Ray(new Vector3(gridCenter.x, rayOriginY, gridCenter.z), Vector3.down);
        
        RaycastHit[] hits = Physics.RaycastAll(ray, 300f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        if (hits.Length == 0)
        {
            // レイが何にも当たらなかった場合（奈落など）は設置不可
            return false;
        }

        // 最も上にある表面の情報を取得
        RaycastHit topSurfaceHit = hits[0];

        // 表面が "Stand" オブジェクトの場合は設置不可
        if (topSurfaceHit.transform.name.Contains("Stand"))
        {
            return false;
        }

        // 最終的な設置位置を、レイキャストが当たった物理的な地面の座標に設定
        worldPos = topSurfaceHit.point;

        // 宝物のコライダー情報を取得
        Collider treasureCollider = holdTreasure.GetComponent<Collider>();
        float treasureHeight = 0.5f; // デフォルトの高さ
        if (treasureCollider != null)
        {
            treasureHeight = treasureCollider.bounds.size.y;
        }

        // 表面の少し上から、宝物の高さ分だけ上に障害物がないかチェック
        Vector3 checkOrigin = topSurfaceHit.point + Vector3.up * 0.01f;
        if (Physics.Raycast(checkOrigin, Vector3.up, treasureHeight, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // 宝物を置くスペースがない場合は設置不可
            return false;
        }

        // 設置候補地点がNavMesh上にあるかを確認（あくまで歩行可能かのチェック）
        UnityEngine.AI.NavMeshHit navHit;
        // 検索範囲を広げてNavMeshを検出しやすくする
        if (UnityEngine.AI.NavMesh.SamplePosition(topSurfaceHit.point, out navHit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            // NavMeshが見つかったので、この場所は有効
            return true;
        }

        // NavMeshが見つからなければ設置不可
        return false;
    }
}
