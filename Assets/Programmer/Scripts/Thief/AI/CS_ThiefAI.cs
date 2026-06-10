/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のAIシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-20 | 探索対象の決定ロジックを追加
 *            | 探索対象の優先順位を追加
 * 2026-04-22 | 耐久値を減少させる処理を追加
 *            | NavMeshAgentを利用して移動する処理を追加
 * 2026-04-23 | 泥棒のデータベースの項目変更に合わせて、Settingメソッドの内容を変更
 *            | 走り状態になる標的オブジェクトのタイプに応じて、移動速度を切り替える処理を追加
 * 2026-04-24 | 探索対象を強制的に変更する処理を追加
 *            | 探索対象の決定ロジックを一つにまとめる(複数個所に分散していたものを、DecideTargetメソッドにまとめる)
 * 2026-04-25 | 次に設定する移動ポイントを決定するロジックの不具合を修正
 * 2026-04-26 | 気絶後の退場処理を仮作成
 *            | 宝物を持って移動する処理を仮作成
 * 2026-04-27 | 部屋移動の閾値に達していたら次の部屋に移動する処理を追加
 * 2026-04-28 | 次の部屋に移動するための移動ポイントを決定するロジックを追加
 * 2026-05-01 | 帰宅ルートを構築するロジックを追加
 * 2026-05-07 | CS_RoomEnemyEntryPointDataを用いた初期部屋の設定の記載
 * 2026-05-08 | 初期部屋の入ってきたドアの位置を保存する処理の記載
 * 2026-05-15 | 同じ部屋の中で、他者が探索しているオブジェクトを探索対象にしないようにする処理を追加
 * 2026-05-17 | DecideTarget内のキャストエラーの不具合を修正
 * 2026-05-18 | A*アルゴリズムを用いて帰宅ルートを構築するロジックを追加
 * 2026-05-21 | CS_SmartNavAgentを用いた危険地帯を考慮した移動処理の追加
 * 2026-05-22 | ファイル名を変更（ThiefAI.cs → CS_ThiefAI.cs）
 *            | クラス名を変更（ThiefAI → CS_ThiefAI）
 *            | リアクション処理を管理するコンポーネントを追加（CS_ThiefReaction）
 * 2026-05-28 | 大解体
 *            | 以下の要素を管理するクラスを新たに作成して、CS_ThiefAIから処理を移動させる。
 *            | 移動処理を管理するクラス(CS_MoveSystem)
 *            | 記憶処理を管理するクラス(CS_MemorySystem)
 *            | 聴覚処理を管理するクラス(CS_HearingSystem)
 *            | 視覚処理を管理するクラス(CS_VisionSensor)
 *            | A*アルゴリズムを用いたルート構築処理を管理するクラス(CS_AStarSystem)
 * 2026-06-04 | 気絶したときの処理をCS_StunThiefTargetに移動
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField, Tooltip("現在の行動状態")]
    private ThiefState currentState;
    public ThiefState read_CurrentState => currentState;

    [Tooltip("泥棒のマテリアル")]
    private Material thiefMaterial;
    [Tooltip("泥棒のマテリアルのフェードアウトにかかる時間")]
    private float fadeAfterStunTime;

    [SerializeField, Tooltip("泥棒の耐久力")]
    private int durability;
    public int read_Durability => durability;

    [Tooltip("泥棒の最大耐久力")]
    private int maxDurability;
    public int read_MaxDurability => maxDurability;

    [Tooltip("持っている宝物オジェクト")]// 見つけたら設定する
    private GameObject heldTreasure;

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
    private bool isUpdatingStunState;

    [Tooltip("ドロップするソウルの数")]
    private int soulDropCount;

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
    public CS_3DPlaySE read_ThiefSound => thiefSound;

    [SerializeField, Tooltip("視界に入る対象のレイヤー"), Header("視界に入る対象のレイヤー")]
    private LayerMask targetLayer;
    [SerializeField, Tooltip("障害物のレイヤー"), Header("障害物のレイヤー")]
    private LayerMask obstacleLayer;

    [Tooltip("猫を捕まえているときの残り時間")]
    private float remainingHoldCatTime = 0.0f;
    [Tooltip("猫を捕まえている時間の初期値")]
    private float initholdCatTime = 0.0f;

    [Tooltip("アニメーション用")]
    private Animator animator;
    public Animator read_Animator => animator;

    // 分解したクラス一覧
    [Tooltip("移動システム")]
    private CS_MoveSystem moveSystem;
    public CS_MoveSystem read_MoveSystem => moveSystem;

    [Tooltip("記憶システム")]
    private CS_MemorySystem memorySystem;
    public CS_MemorySystem read_MemorySystem => memorySystem;

    [Tooltip("聴覚システム")]
    private CS_HearingSystem hearingSystem;
    public CS_HearingSystem read_HearingSystem => hearingSystem;

    [Tooltip("視覚システム")]
    private CS_VisionSensor visionSensor;
    public CS_VisionSensor read_VisionSensor => visionSensor;

    [Tooltip("A*アルゴリズムシステム")]
    private CS_AStarSystem aStarSystem;
    public CS_AStarSystem read_AStarSystem => aStarSystem;

    /// <summary>
    /// 泥棒のステータスを設定する処理
    /// </summary>
    /// <param name="typedata">泥棒のタイプごとのステータスデータ</param>
    /// <param name="data">泥棒の共通ステータスデータ</param>
    /// <param name="playerSpeed">プレイヤーの移動速度（泥棒の移動速度をプレイヤーの移動速度に対する倍率で設定するため）</param>
    /// <param name="entryRoom">最初に入ってくる部屋のオブジェクト</param>
    /// <param name="entryPoint">最初に入ってくるドアの位置</param>
    public void Setting(CO_ThiefStatusData typedata, CO_ThiefCommonStatusData data, float playerSpeed, CS_RoomNode entryRoom, Transform entryPoint)
    {
        /*未実装、未設定　*///data.jumpHeight;

        durability = typedata.durability;
        maxDurability = typedata.durability;
        soulDropCount = typedata.soulDropCount;

        // 移動システムの初期化
        moveSystem = new CS_MoveSystem(this, GetComponent<NavMeshAgent>(), GetComponent<CS_SmartNavAgent>(), typedata, playerSpeed);

        // 聴覚システムの初期化
        hearingSystem = new CS_HearingSystem(this);

        // 視覚システムの初期化
        visionSensor = new CS_VisionSensor(this, typedata, targetLayer, obstacleLayer);

        // A*アルゴリズムシステムの初期化
        aStarSystem = new CS_AStarSystem(this);

        // 記憶システムの初期化
        memorySystem = new CS_MemorySystem(this, entryRoom, entryPoint, typedata, data);
        memorySystem.FindNowRoomNode();

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
        thiefReactionUI = GetComponent<CS_ThiefReactionUI>();
        thiefReactionUI.RegisterReaction(data.reactionUISprites);

        // 泥棒のリアクションを管理するコンポーネントを取得
        thiefReaction = GetComponentInChildren<CS_ThiefReaction>();

        fadeAfterStunTime = data.fadeAfterStunTime;

        thiefMaterial = transform.GetComponentInChildren<Renderer>().material;
        if (thiefMaterial == null)
        {
            Debug.LogError("ThiefAI: 泥棒のマテリアルが見つかりません。");
        }
        thiefMaterial.SetFloat("_DisappearTime", fadeAfterStunTime);
        thiefMaterial.SetFloat("_Timer", fadeAfterStunTime);

        // アニメーション用のコンポーネントを取得
        animator = GetComponentInChildren<Animator>();

        // サウンドマネージャーから泥棒のサウンドを管理するコンポーネントを取得
        GameObject soundManager = GameObject.Find("AudioManager");
        // 子オブジェクトからCS_3DPlaySEコンポーネントを取得
        if (soundManager != null) thiefSound = soundManager.GetComponentInChildren<CS_3DPlaySE>();
        if (thiefSound == null) Debug.LogWarning("CS_3DPlaySEコンポーネントが見つかりません。サウンドが再生されません。");
    }

    private void Update()
    {
        // 無敵時間の経過を管理
        // 気絶状態のときは無敵時間の経過を管理しない（気絶状態のときは攻撃を受けない想定のため）
        if (remainingInvincibleTime > 0)
        {
            // 無敵時間が残っているときは青くする
            thiefMaterial.color = Color.Lerp(Color.red, Color.blue, remainingInvincibleTime / invincibleTime);

            if (currentState != ThiefState.Stunned)
            {
                remainingInvincibleTime -= Time.deltaTime;
                if (remainingInvincibleTime < 0)
                {
                    remainingInvincibleTime = 0;
                }
            }
        }

        // 猫を捕まえているときの経過時間を管理
        if (remainingHoldCatTime > 0.0f)
        {
            remainingHoldCatTime -= Time.deltaTime;
            if (remainingHoldCatTime < 0.0f)
            {
                remainingHoldCatTime = 0.0f;
                memorySystem.ClearTarget();
            }
            return;
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
    }

    // 探索状態の行動
    private void Explore()
    {
        // 移動システムのスタックを修正する処理
        moveSystem.FixStuck();

        thiefReaction.ClearReaction();

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
        // 宝物を持つ
        heldTreasure = memorySystem.read_CurrentTarget.gameObject;
        heldTreasure.transform.parent = this.transform; // 泥棒の子オブジェクトにする
        heldTreasure.GetComponent<Collider>().enabled = false; // 宝物のコライダーを無効にする
        heldTreasure.transform.localPosition = new Vector3(0.0f, this.transform.position.y, 0.0f); // 宝物の位置を泥棒の位置に合わせる

        // 状態を逃走に変更
        ChangeStatus(ThiefState.Escape);

        // 取得した宝物を他の泥棒の記憶から消去する
        GameObject.FindObjectOfType<CS_ThiefManager>().EraseTheMemoryToAllThief(heldTreasure.GetComponent<CS_ThiefTarget>());
        // 探索対象をリセット
        memorySystem.ClearTarget();
    }

    // 逃走状態の行動
    private void Escape()
    {
        // 移動システムのスタックを修正する処理
        moveSystem.FixStuck();

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
            Debug.Log("泥棒が退場しました。");
            Destroy(this.gameObject);
        }
    }

    // 気絶状態の行動
    private void Stunned()
    {
        // ナビメッシュエージェントを停止させる（安全に）
        SetAgentStopped(true);

        // 経過時間を加算
        if(isUpdatingStunState) elapsedTimeAfterStun += Time.deltaTime;

        // 耐久値が残っている場合は、気絶時間が経過したら無敵時間を付与して、状態を探索に戻す
        if (durability > 0)
        {
            // 経過時間が気絶時間を超えた場合は、耐久力を減少させて、状態を探索に戻す
            if (elapsedTimeAfterStun >= damageStunTime)
            {
                currentState = ThiefState.Explore; // 状態を探索に戻す
                SetAgentStopped(false); // ナビメッシュエージェントを再開させる（安全に）
            }
        }
        // 耐久力が0以下の場合は、時間経過後に退場する
        else
        {
            // 経過時間が退場するまでの時間を超えた場合は、退場する処理を追加する
            if (elapsedTimeAfterStun >= exitAfterStunTime)
            {
                thiefMaterial.SetFloat("_Timer", fadeAfterStunTime - (elapsedTimeAfterStun - exitAfterStunTime));

                Transform faceTransform = this.transform.GetChild(1);
                Vector3 facePos = faceTransform.position;
                faceTransform.position = new Vector3(facePos.x, facePos.y + 0.01f * (elapsedTimeAfterStun - exitAfterStunTime), facePos.z);

                if (thiefMaterial.GetFloat("_Timer") <= 0.0f)
                {
                    // 気絶したときのSEを再生する処理を追加する
                    if (thiefSound != null) thiefSound.PlayOneShotSE("ThiefDeath", gameObject.transform.position, "ThiefDeath");

                    Destroy(this.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 耐久値を減らす処理
    /// </summary>
    /// <param name="damage">与える減少値</param>
    /// <param name="type">ギミックの種類</param>
    /// <param name="isHit">直接命中したかどうか</param>
    public void TakeDamage(int damage, Gimmick type, bool isHit = true)
    {
        if (remainingInvincibleTime > 0) return;

        durability -= damage;

        // 泥棒のアニメーション状態をDamageにする
        if (animator != null)animator.SetTrigger("DamageTrigger");

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
        // ダメージを受けたときのSEを再生する処理を追加する
        if (thiefSound != null) thiefSound.PlayOneShotSE("ThiefDamage", gameObject.transform.position, "ThiefDamage");

        // プレイヤーの追跡情報をリセットする
        memorySystem.ResetIgnorePlayer();


        switch (type)
        {
            case Gimmick.Pot:
                thiefReactionUI.SetReactionUI(CS_ThiefReactionUI.ThiefReactionUIType.Pot);
                break;
            case Gimmick.IronBall:
                thiefReactionUI.SetReactionUI(CS_ThiefReactionUI.ThiefReactionUIType.IronBall);
                break;
            case Gimmick.EmptyChest:
            case Gimmick.None:
            default:
                break;
        }

        currentState = ThiefState.Stunned; // 状態を気絶に変更
        elapsedTimeAfterStun = 0.0f; // 気絶時間の経過時間をリセット

        remainingInvincibleTime = invincibleTime; // 無敵時間を付与

        memorySystem.ResetIgnorePlayer(); // プレイヤーを無視する状態をリセット


        // 耐久力が0以下になった場合は、耐久力を0に補正して気絶状態にする
        if (durability <= 0)
        {
            durability = 0;

            // StunThiefTargetに通知する
            GetComponent<CS_StunThiefTarget>().Notify();

            // プレイヤーにソウルを入手させる
            CS_PlayerAction playerAction = GameObject.FindObjectOfType<CS_PlayerAction>();

            // playerActionが見つかった場合は、ソウルを加算する処理を実行する。見つからない場合は、エラーログを出力する。
            if (playerAction != null) playerAction.AddSoul(soulDropCount);
            else Debug.LogError("PlayerActionが見つかりませんでした。ThiefAIのTakeDamageメソッドで、プレイヤーにソウルを入手させる処理が正常に動作しない可能性があります。");
        }
    }

    /// <summary>
    /// 現在の状態を変更する処理
    /// </summary>
    /// <param name="newState">変更する状態</param>
    public void ChangeStatus(ThiefState newState)
    {
        currentState = newState;
    }

    /// <summary>
    /// 猫を捕まえた処理
    /// </summary>
    public void CatchCat()
    {
        // 猫を捕まえている時間を設定
        remainingHoldCatTime = initholdCatTime;

        // 猫を捕まえているSEを再生する
        if (thiefSound != null) thiefSound.PlayOneShotSE("ThiefCatch", gameObject.transform.position, "ThiefCatch");
    }

    /// <summary>
    /// 泥棒のアニメーションを変更する処理(Triigerのみ)
    /// </summary>
    /// <param name="parameter">変更するアニメーションのパラメーター</param>
    /// memo: 
    /// NotFoundTrigger | 空の宝箱を探索後のアニメーション
    /// FoundTrigger | 宝物を探索後のアニメーション
    /// DamageTrigger | ダメージを受けたときのアニメーション
    public void SetAnimation(string parameter)
    {
        if (animator != null) animator.SetTrigger(parameter);
    }

    // == 落とし穴用項目
    /// <summary>
    /// 落とし穴ギミックにハマった
    /// </summary>
    [ContextMenu("落とし穴ギミックにハマったときの処理")]
    public void PitFallStart()
    {
        // 状態を気絶に変更
        ChangeStatus(ThiefState.Stunned);
        // 気絶時間の経過時間をリセット
        elapsedTimeAfterStun = 0.0f;
        // 気絶状態の更新処理を実行するようにする
        isUpdatingStunState = false;

        // NavMeshAgentを停止させる
        moveSystem.read_NavMeshAgent.ResetPath();
        moveSystem.read_NavMeshAgent.enabled = false;
        // SmartNavAgentも停止させる
        moveSystem.read_SmartNavAgent.enabled = false;

        // 落とし穴にハマったときの見た目の位置調整
        // 体の8割ほど埋める
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y - transform.localScale.y * 0.8f, pos.z);
    }
    /// <summary>
    /// 落とし穴ギミックから抜けた
    /// </summary>
    [ContextMenu("落とし穴ギミックから抜けたときの処理")]
    public void PitFallEnd()
    {
        // 気絶状態の更新処理を実行するようにする
        isUpdatingStunState = true; 
        // NavMeshAgentを再度有効にする
        moveSystem.read_NavMeshAgent.enabled = true;
        // SmartNavAgentも再度有効にする
        moveSystem.read_SmartNavAgent.enabled = true;
    }

    /// <summary>
    /// ナビメッシュエージェントを安全に停止させる処理
    /// </summary>
    /// <param name="stop">停止させるかどうか</param>
    private void SetAgentStopped(bool stop)
    {
        var agent = moveSystem?.read_NavMeshAgent;
        if (agent == null) return;

        // エージェントが無効化されている場合は操作しない（PitFall中など）
        if (!agent.enabled) return;

        // NavMesh上に置かれていないエージェントに対して isStopped を呼ぶと例外になるためチェック
        if (agent.isOnNavMesh)
        {
            agent.isStopped = stop;
        }
        else
        {
            // 安全のためログを残す（頻発する場合は削除）
            Debug.LogWarning("SetAgentStopped: NavMeshAgent が NavMesh 上にありません。操作をスキップします。", this);
        }
    }

    private void OnDrawGizmos()
    {
        // 視界の範囲を描画
        if (visionSensor != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionSensor.viewDistance);
            Vector3 forward = transform.forward * visionSensor.viewDistance;
            Vector3 leftBoundary = Quaternion.Euler(0, -visionSensor.viewAngle / 2, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, visionSensor.viewAngle / 2
                , 0) * forward;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }

        // 現在の移動目的地までを線で描画
        if (memorySystem != null)
        {
            if (memorySystem.read_CurrentTarget == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, memorySystem.read_CurrentTarget.transform.position);
        }
    }
}
