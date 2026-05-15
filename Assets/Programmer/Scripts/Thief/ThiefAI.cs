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
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 泥棒のAIシステム
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(VisionSensor))]
public class ThiefAI : MonoBehaviour
{
    [Tooltip("泥棒の行動状態を定義する列挙型")]
    private enum ThiefState
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

    [SerializeField]
    [Tooltip("現在の行動状態")]
    private ThiefState currentState;

    [SerializeField, Header("泥棒のリアクションスプライト(仮)")]
    private List<Sprite> reactionSprites;
    enum ReactionSpriteType
    {
        Normal, // 通常
        Search, // 探索
        Stun    // 気絶
    }

    [Tooltip("泥棒のマテリアル")]
    private Material thiefMaterial;
    [Tooltip("泥棒のマテリアルのフェードアウトにかかる時間")]
    private float fadeAfterStunTime;

    [Tooltip("現在いる部屋の情報")]
    private RoomNode currentRoom;
    private GameObject currentRoomObject;
    [Tooltip("部屋に関する記憶")]
    private Dictionary<RoomNode, RoomMemory> roomMemories;

    [Tooltip("視認オブジェクトの記憶")]
    private Dictionary<VisionTarget, VisionTargetMemory> visionTargetMemories;

    [Tooltip("探索対象")]
    private ThiefTarget currentTarget;
    public ThiefTarget CurrentTarget => currentTarget;

    [Tooltip("次の部屋に移動するための移動ポイント")]
    private Transform nextRoomMovePoint;
    [Tooltip("次の部屋に移動するための移動ポイントを決定したかどうかを判定するフラグ")]
    private bool isNextRoomMovePointDecided;

    [Tooltip("持っている宝物オジェクト")]// 見つけたら設定する
    private GameObject heldTreasure;

    [SerializeField, Tooltip("泥棒の耐久力")]
    private int durability;
    [SerializeField, Tooltip("泥棒の移動速度")]
    private float walkSpeed;
    private float runSpeed;

    [Tooltip("泥棒が探索するのにかかる秒数")]
    private int searchTime;

    [Tooltip("攻撃を受けた後の気絶時間")]
    private float damageStunTime;

    [Tooltip("攻撃を受けた後の無敵時間")]
    private float invincibleTime;
    [Tooltip("無敵時間の現在残り時間")]
    private float remainingInvincibleTime;

    [Tooltip("気絶後に退場するまでの時間")]
    private int exitAfterStunTime;
    [Tooltip("気絶後の経過時間")]
    private float elapsedTimeAfterStun;

    [Tooltip("ドロップするソウルの数")]
    private int soulDropCount;

    [Tooltip("走り状態になる標的オブジェクトのタイプリスト")]
    private List<VisionTarget.TargetType> runTargetTypes;

    [Tooltip("次の部屋探索に切り替える探索度の閾値")]
    private int nextRoomSearchThreshold;

    [Tooltip("ナビメッシュエージェント")]
    private NavMeshAgent navMeshAgent;
    [Tooltip("泥棒のリアクションを管理するコンポーネント")]
    private ThiefReaction thiefReaction;

    [Tooltip("最初の部屋のオブジェクト")]
    private RoomNode firstRoom;
    private Transform firstEntryPoint; // 最初に入ってきたドアの位置(逃走ルートの最終目的位置)

    [Tooltip("帰宅ルート")]
    private List<Transform> escapeRoute;


    // 泥棒の耐久力と移動速度を設定するメソッド
    public void Setting(CSS_ThiefStatusData typedata, CSS_ThiefCommonStatusData data, float playerSpeed, RoomNode entryRoom, Transform entryPoint)
    {
        /*未実装、未設定　*///data.jumpHeight;
        /*未設定、未設定　*///data.alertTime;

        durability = typedata.durability;
        walkSpeed = playerSpeed * typedata.walkSpeedMultiplier;
        runSpeed = playerSpeed * typedata.runSpeedMultiplier;
        nextRoomSearchThreshold = typedata.nextRoomSearchThreshold;
        runTargetTypes = typedata.runTargetTypes;
        soulDropCount = typedata.soulDropCount;
        searchTime = typedata.searchTime;

        exitAfterStunTime = data.exitAfterStunTime;
        damageStunTime = data.stunTime;
        invincibleTime = data.invincibleTime;
        remainingInvincibleTime = 0.0f;

        // 初期状態を探索に設定
        currentState = ThiefState.Explore;

        // 初期部屋を設定（仮）
        currentRoom = entryRoom;

        // 部屋の記憶を初期化
        roomMemories = new Dictionary<RoomNode, RoomMemory>();

        // 視認オブジェクトの記憶を初期化
        visionTargetMemories = new Dictionary<VisionTarget, VisionTargetMemory>();

        // 初期部屋の記憶を作成
        roomMemories[currentRoom] = new RoomMemory();
        roomMemories[currentRoom].FirstSetting();
        roomMemories[currentRoom].explorationLevel = currentRoom.initialExplorationLevel;

        // ナビメッシュエージェントの速度を設定
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.baseOffset = 1.0f; // キャラクターの高さに合わせてオフセットを設定
        navMeshAgent.speed = this.walkSpeed;

        // リジットボディの設定
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // ナビメッシュエージェントで移動させるため、リジットボディをキネマティックに設定
        rb.useGravity = false; // 重力の影響を受けないようにする

        // 泥棒のリアクションを管理するコンポーネントを取得
        thiefReaction = GetComponent<ThiefReaction>();

        // 最初の部屋と入ってきたドアの位置を保存
        firstRoom = entryRoom;
        firstEntryPoint = entryPoint;
    }

    private void Start()
    {
        fadeAfterStunTime = GameObject.FindObjectOfType<ThiefManager>().GetThiefCommonDB().fadeAfterStunTime;

        thiefMaterial = GetComponent<Renderer>().material;
        if (thiefMaterial == null)
        {
            Debug.LogError("ThiefAI: 泥棒のマテリアルが見つかりません。");
        }
        thiefMaterial.SetFloat("_DisappearTime", fadeAfterStunTime);
        thiefMaterial.SetFloat("_Timer", fadeAfterStunTime);

        FindNowRoomNode();
    }

    private void Update()
    {
        // 無敵時間の経過を管理
        // 気絶状態のときは無敵時間の経過を管理しない（気絶状態のときは攻撃を受けない想定のため）
        if (remainingInvincibleTime > 0)
        {
            if (currentState != ThiefState.Stunned)
            {
                remainingInvincibleTime -= Time.deltaTime;
                if (remainingInvincibleTime <= 0)
                {
                    remainingInvincibleTime = 0;
                }
            }
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
        // 探索対象を決定
        RecognizeObjects();

        ChangeFace(ReactionSpriteType.Normal);

        // 探索対象がない場合や、探索度が次の部屋に移動するための閾値を超えている場合は、次の部屋に移動するための処理を追加する
        if (isNextRoomMovePointDecided || roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
        {
            if (nextRoomMovePoint == null)
            {
                // 次の部屋に移動するための移動ポイントが決まっていない場合は、次の部屋に移動するための移動ポイントを決定するロジックを実行する
                NextDoorElection();
                // 再取得してもnullの場合
                if (nextRoomMovePoint == null) return;

                navMeshAgent.SetDestination(nextRoomMovePoint.position);
            }
            return;
        }

        // 探索対象がない場合は、部屋の移動ポイントに沿って移動する処理を追加する
        if (currentTarget == null)
        {
            DecideTarget();
            return;
        }
        // 現在の探索対象が視認オブジェクト(VisionTarget)かどうかを判定
        if (currentTarget is VisionTarget)
        {
            // 探索対象が既に探索済みの場合
            if (visionTargetMemories[((VisionTarget)currentTarget)].isExplored)
            {
                DecideTarget();
            }
            // 探索対象が未探索の場合
            else
            {
                // 現在の探索対象が他者にも探索されているオブジェクトである場合は、探索対象をリセットして、次の探索対象を決定する
                if (visionTargetMemories[((VisionTarget)currentTarget)].searchThief != null && visionTargetMemories[((VisionTarget)currentTarget)].searchThief != this.gameObject)
                {
                    Debug.Log("【"+ this.gameObject.name +"】Explore: 探索対象 " + currentTarget.name + " は" + ((VisionTarget)currentTarget).searchThief.gameObject.name + "が探索しているため、探索対象をリセットします。");

                    
                    currentTarget = null;
                    return;
                }

                // 探索対象に十分近づいたら、探索度を進める
                if (Vector3.Distance(transform.position, currentTarget.transform.position) < ((VisionTarget)currentTarget).exploredDistanceThreshold)
                {
                    ChangeFace(ReactionSpriteType.Search); // 探索完了の表情に変更する処理を追加する

                    ((VisionTarget)currentTarget).searchThief = this.gameObject; // 探索対象に対して、探索している人を設定する

                    if (ProgressTargetSearchTime())
                    {
                        // 宝物を探索にしていて、完了した場合は、発見状態に切り替える
                        if (((VisionTarget)currentTarget).targetType == VisionTarget.TargetType.Treasure)
                        {
                            // 発見状態に切り替える
                            currentState = ThiefState.Found;
                        }
                        // それ以外のオブジェクトを探索して完了した場合は、次の探索対象を決定する
                        else
                        {
                            // 探索対象を探索済みに設定
                            visionTargetMemories[((VisionTarget)currentTarget)].isExplored = true;
                            // 探索度を加算
                            roomMemories[currentRoom].explorationLevel += ((VisionTarget)currentTarget).explorationValue;

                            // 探索度が閾値を超えた場合は、次の部屋に移動するための処理を追加する
                            if (roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
                            {
                                // 探索対象をリセット
                                ((VisionTarget)currentTarget).searchThief = null; // 探索対象の探索している人をリセットする
                                currentTarget = null;

                                isNextRoomMovePointDecided = true;
                            }
                            // それ以外の場合は、次の探索対象を決定する
                            else
                            {
                                ((VisionTarget)currentTarget).searchThief = null; // 探索対象の探索している人をリセットする
                                currentTarget = null;
                                DecideTarget();
                            }
                        }
                    }
                }
                else
                {
                    visionTargetMemories[((VisionTarget)currentTarget)].explorationProgress = 0; // 探索対象から離れた場合は、探索の進行度をリセットする
                }
            }
        }
        else
        {
            // 探索対象に向かって移動
            navMeshAgent.SetDestination(currentTarget.transform.position);

            // 探索対象に十分近づいたら、次の探索対象を決定
            if (Vector3.Distance(transform.position, currentTarget.transform.position) < 2.0f)
            {
                DecideTarget();
            }

            // 未探索のオブジェクトを視認した場合はそっちの探索に切り替える
            if (HasUnexploredTargets())
            {
                DecideTarget();
            }
        }
    }

    // 発見状態の行動
    // ----------------
    // TODO: 宝物発見バフの適応処理を追加する
    private void Found()
    {
        // 宝物を持つ
        heldTreasure = currentTarget.gameObject;
        currentTarget.gameObject.transform.parent = this.transform; // 泥棒の子オブジェクトにする
        currentTarget.GetComponent<Collider>().enabled = false; // 宝物のコライダーを無効にする
        currentTarget.gameObject.transform.localPosition = new Vector3(0.0f, this.transform.position.y, 0.0f); // 宝物の位置を泥棒の位置に合わせる

        // 状態を逃走に変更
        currentState = ThiefState.Escape;

        // 取得した宝物を他の泥棒の記憶から消去する
        GameObject.FindObjectOfType<ThiefManager>().EraseTheMemoryToAllThief(currentTarget);
        // 探索対象をリセット
        currentTarget = null;
    }

    // 逃走状態の行動
    private void Escape()
    {
        // ルートが未構築なら構築する
        if (escapeRoute == null || escapeRoute.Count == 0)
        {
            EscapeRoute(); // ここで escapeRoute が埋まる＆ nextRoomMovePoint も先頭が入る想定
        }

        // それでも無いなら、何らかの理由でルートが作れなかった
        if (escapeRoute == null || escapeRoute.Count == 0)
        {
            // フォールバック（暫定）：従来の movePoints 逃走などに戻したい場合はここに書く
            Debug.LogWarning("【泥棒】Escape: escapeRoute が空のため移動できません。");
            return;
        }

        // 次に向かうドア
        Transform door = escapeRoute[0];
        if (door == null)
        {
            // 参照切れ対策：無効な要素を捨てて次へ
            escapeRoute.RemoveAt(0);
            return;
        }

        // ドアへ移動
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(door.position);
    }

    // 気絶状態の行動
    // ----------------
    // TODO: その場で動けなくなる処理を追加する
    private void Stunned()
    {
        // ナビメッシュエージェントを停止させる
        navMeshAgent.isStopped = true;

        // 経過時間を加算
        elapsedTimeAfterStun += Time.deltaTime;

        // 耐久値が残っている場合は、気絶時間が経過したら無敵時間を付与して、状態を探索に戻す
        if (durability > 0)
        {
            // 経過時間が気絶時間を超えた場合は、耐久力を減少させて、状態を探索に戻す
            if (elapsedTimeAfterStun >= damageStunTime)
            {
                currentState = ThiefState.Explore; // 状態を探索に戻す
                navMeshAgent.isStopped = false; // ナビメッシュエージェントを再開させる
            }
        }
        // 耐久力が0以下の場合は、時間経過後に退場する
        else
        {
            // 経過時間が退場するまでの時間を超えた場合は、退場する処理を追加する
            if (elapsedTimeAfterStun >= exitAfterStunTime)
            {
                thiefMaterial.SetFloat("_Timer", fadeAfterStunTime - (elapsedTimeAfterStun - exitAfterStunTime));

                Transform faceTransform = this.transform.GetChild(0);
                Vector3 facePos = faceTransform.position;
                faceTransform.position = new Vector3(facePos.x, facePos.y + 0.01f * (elapsedTimeAfterStun - exitAfterStunTime), facePos.z);

                if (thiefMaterial.GetFloat("_Timer") <= 0.0f)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 部屋のオブジェクトを視認して記憶に保存する処理
    /// </summary>
    private void RecognizeObjects()
    {
        // 視界内オブジェクトを取得
        List<ThiefTarget> visionTargets = this.GetComponent<VisionSensor>().Scan();
        
        bool isNewObjectRecognized = false; // 新たに視認したオブジェクトがあるかどうかを判定するフラグ
        // 視認したオブジェクトを記憶に保存
        foreach (ThiefTarget target in visionTargets)
        {
            // 現在の部屋の記憶がない場合は新たに作成
            if (roomMemories[currentRoom] == null)
            {
                roomMemories[currentRoom] = new RoomMemory();
                roomMemories[currentRoom].FirstSetting();
            }

            // 現在の部屋の記憶に認識しているオブジェクトのリストがない場合は新たに作成
            if (roomMemories[currentRoom].recognizedObjects == null) roomMemories[currentRoom].recognizedObjects = new List<ThiefTarget>();

            bool isAlreadyRecognized = false; // 既に記憶しているオブジェクトかどうかを判定するフラグ
            foreach (var entry in roomMemories[currentRoom].recognizedObjects)
            {
                // 既に記憶しているオブジェクトの場合はスキップ
                if (entry == target) isAlreadyRecognized = true;
            }
            if (isAlreadyRecognized)
            {
                if(target is VisionTarget)
                {
                    // 既に記憶しているオブジェクトが視認オブジェクト(VisionTarget)の場合は、探索している人がいるかどうかの情報を更新する
                    if (visionTargetMemories.ContainsKey((VisionTarget)target))
                    {
                        visionTargetMemories[((VisionTarget)target)].searchThief = ((VisionTarget)target).searchThief;
                    }
                }

                continue;
            }
            

            // 新しいオブジェクトを記憶に追加
            roomMemories[currentRoom].recognizedObjects.Add(target);
            // 記憶領域の作成
            if(target is VisionTarget)visionTargetMemories[((VisionTarget)target)] = new VisionTargetMemory();

            isNewObjectRecognized = true; // 新たに視認したオブジェクトがある場合はフラグを立てる
        }

        // 新たに視認したオブジェクトを記憶に保存した後、探索対象を決定する処理を追加する
        if(isNewObjectRecognized)DecideTarget();
    }

    /// <summary>
    /// 視認しているオブジェクトの中に未探索のものがあるかどうかを判定する処理
    /// </summary>
    /// <returns>
    /// true:未探索のオブジェクトがある | false:認識している全てのオブジェクトが探索済み
    /// </returns>
    private bool HasUnexploredTargets()
    {
        // 現在の部屋の記憶がない場合や、認識しているオブジェクトがない場合は、未探索のオブジェクトがないと判定してfalseを返す
        if (roomMemories[currentRoom] == null || roomMemories[currentRoom].recognizedObjects == null) return false;

        // 視認しているオブジェクトの中に未探索のものがあるかどうかを判定
        foreach (var entry in roomMemories[currentRoom].recognizedObjects)
        {
            // 未探索のオブジェクトで、かつ他者にも探索されているオブジェクトがある場合は、未探索のオブジェクトがあると判定してtrueを返す
            if (!visionTargetMemories[((VisionTarget)entry)].isExplored && (visionTargetMemories[((VisionTarget)entry)].searchThief == null)) return true;
        }

        // 全てのオブジェクトが探索済みの場合はfalseを返す
        return false;
    }

    /// <summary>
    /// 探索対象を決める処理
    /// </summary>
    private void DecideTarget()
    {
        // 探索対象との距離
        float distanceToTarget = Mathf.Infinity;

        // 未探索のオブジェクトがある場合は、未探索のオブジェクトを優先して探索対象に設定
        if (HasUnexploredTargets())
        {
            if (currentTarget is VisionTarget)
            {
                if (currentTarget != null && (VisionTarget)currentTarget)
                {
                    ((VisionTarget)currentTarget).searchThief = null; // 現在の探索対象の探索している人をリセットする
                }
            }

            foreach (var entry in roomMemories[currentRoom].recognizedObjects)
            {
                // 既に探索済みのオブジェクトはスキップ
                if (visionTargetMemories[((VisionTarget)entry)].isExplored) continue;

                // 他者にも探索されているオブジェクトはスキップ
                if (visionTargetMemories[((VisionTarget)entry)].searchThief != null) continue;

                // 既に探索対象に設定しているオブジェクトはスキップ
                if (entry == currentTarget) continue;

                // 現在の探索対象が視認オブジェクト(VisionTarget)かどうか
                if (entry is VisionTarget)
                {
                    // 探索対象の優先順位を決めるロジック
                    switch (((VisionTarget)entry).targetType)
                    {
                        case VisionTarget.TargetType.Treasure:
                            {
                                if (currentTarget is VisionTarget)
                                {
                                    // 現在の探索対象が宝物でない場合は、問答無用で宝物を探索対象に設定
                                    if (((VisionTarget)currentTarget).targetType != VisionTarget.TargetType.Treasure)
                                    {                                      
                                        currentTarget = entry;
                                        break;
                                    }
                                    // 現在の探索対象も宝物の場合は、距離が近い方を探索対象に設定する
                                    else
                                    {
                                        // オブジェクトとの距離を計算
                                        float distance = Vector3.Distance(transform.position, entry.transform.position);

                                        // より近いオブジェクトを探索対象に設定
                                        if (distance < distanceToTarget)
                                        {
                                            distanceToTarget = distance;
                                            currentTarget = entry;
                                        }
                                        else continue;
                                    }
                                }
                                else if (currentTarget is TrapTarget)
                                {

                                    // 空の宝箱型の罠の場合ではない場合は、スキップ
                                    if (entry is TrapTarget tt && tt.gimmickScript.gimmick != Gimmick.EmptyChest) continue;

                                    // 宝物罠の場合は、距離判定で探索対象を切り替える
                                    // オブジェクトとの距離を計算
                                    float distance = Vector3.Distance(transform.position, entry.transform.position);

                                    // より近いオブジェクトを探索対象に設定
                                    if (distance < distanceToTarget)
                                    {
                                        distanceToTarget = distance;
                                        currentTarget = entry;
                                    }
                                    else continue;
                                }
                                else
                                {
                                    // プレイヤーを探索対象にしている場合は、問答無用で宝物を探索対象に設定
                                    currentTarget = entry;
                                }
                            }
                            break;
                        case VisionTarget.TargetType.RoomObject:
                            {
                                // 現在の探索対象が宝物の場合は、スキップ
                                if (currentTarget is VisionTarget vt && vt.targetType == VisionTarget.TargetType.Treasure) continue;
                                // 現在の探索対象が空の宝箱型の罠の場合は、スキップ
                                if (currentTarget is TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest) continue;

                                // オブジェクトとの距離を計算
                                float distance = Vector3.Distance(transform.position, entry.transform.position);

                                // より近いオブジェクトを探索対象に設定
                                if (distance < distanceToTarget)
                                {
                                    distanceToTarget = distance;
                                    currentTarget = entry;
                                }
                                else continue;
                            }
                            break;
                    }
                }
                else if (entry is PlayerTarget)
                {
                    // 宝物を探索対象にしている場合は、スキップ
                    if (currentTarget is VisionTarget vt && vt.targetType == VisionTarget.TargetType.Treasure) continue;
                    // 空の宝箱型の罠を探索対象にしている場合は、スキップ
                    if (currentTarget is TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest) continue;

                }
                else if (entry is TrapTarget)
                {
                    // 宝物を探索対象にしている場合は、スキップ
                    if (currentTarget is VisionTarget vt && vt.targetType == VisionTarget.TargetType.Treasure) continue;
                    // 宝物の罠を探索対象にしている場合は、スキップ
                    if (currentTarget is TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest) continue;

                    // オブジェクトとの距離を計算
                    float distance = Vector3.Distance(transform.position, entry.transform.position);
                    // より近いオブジェクトを探索対象に設定
                    if (distance < distanceToTarget)
                    {
                        distanceToTarget = distance;
                        currentTarget = entry;
                    }
                    else continue;
                }
            }

        }
        // 未探索のオブジェクトがない場合は、部屋の移動ルートに沿って移動する処理を追加する
        else
        {
            // 前回の探索対象がThiefTargetの派生クラスかどうか(前回が移動ポイントでない場合)
            if (currentTarget == null || currentTarget is VisionTarget || currentTarget is TrapTarget || currentTarget is PlayerTarget)
            {

                if (currentTarget != null && (VisionTarget)currentTarget)
                {
                    ((VisionTarget)currentTarget).searchThief = null; // 現在の探索対象の探索している人をリセットする
                    currentTarget = null;
                }
                // 視認オブジェクトから移動ポイントにする場合は一番近いものを探索対象に設定
                foreach (ThiefTarget target in currentRoom.movePoints)
                {
                    if (target == null) continue;

                    // オブジェクトとの距離を計算
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    // より近いオブジェクトを探索対象に設定
                    if (distance < distanceToTarget)
                    {
                        distanceToTarget = distance;
                        currentTarget = target;
                    }
                    else continue;
                }
            }
            // 移動ポイントから移動ポイントにする場合は、右回りの場合リストを加算、左回りの場合リストを減算して設定
            else
            {
                // 現在の移動ポイントがリストのどこにあるかを判定
                for (int i = 0 ; i < currentRoom.movePoints.Count ; i++)
                {
                    // 現在の移動ポイントがリストのどこにあるかを判定
                    if (currentRoom.movePoints[i] == currentTarget)
                    {
                        int nextIndex = 0;

                        // 右回りの場合
                        if (currentRoom.isListDown)
                        {
                            // 次のインデックスを計算
                            nextIndex = i + 1;

                            // インデックスがリストの範囲を超える場合は、リストの先頭に戻す
                            if (nextIndex >= currentRoom.movePoints.Count) nextIndex = 0;

                            // リストを加算して次の移動ポイントを探索対象に設定
                            currentTarget = currentRoom.movePoints[nextIndex];
                            break;
                        }
                        // 左回りの場合
                        else
                        {
                            // 次のインデックスを計算
                            nextIndex = i - 1;

                            // インデックスがリストの範囲を超える場合は、リストの末尾に戻す
                            if (nextIndex < 0) nextIndex = currentRoom.movePoints.Count - 1;

                            // リストを減算して次の移動ポイントを探索対象に設定
                            currentTarget = currentRoom.movePoints[nextIndex];
                            break;
                        }
                    }
                }
            }
        }

        if (currentTarget is VisionTarget)
        {
            // 探索対象が走り状態になる標的オブジェクトのタイプリストに含まれている場合は、走り状態に切り替える
            if (runTargetTypes.Contains(((VisionTarget)currentTarget).targetType))
            {
                navMeshAgent.speed = runSpeed;
            }
            else
            {
                navMeshAgent.speed = walkSpeed;
            }
        }
        else
        {
            navMeshAgent.speed = walkSpeed;
        }

        // 探索対象に向かって移動
        navMeshAgent.SetDestination(currentTarget.transform.position);
    }

    /// <summary>
    /// 耐久値を減らす処理
    /// </summary>
    /// <param name="damage">与える減少値</param>
    public void TakeDamage(int damage, Gimmick type)
    {
        if (remainingInvincibleTime > 0)
        {
            return;
        }

        durability -= damage;

        switch (type)
        {
            case Gimmick.Pot:
                thiefReaction.SetReactionUI(ThiefReaction.ThiefReactionType.Pot);
                break;
            case Gimmick.IronBall:
                thiefReaction.SetReactionUI(ThiefReaction.ThiefReactionType.IronBall);
                break;
            case Gimmick.EmptyChest:
            case Gimmick.None:
            default:
                break;
        }

        currentState = ThiefState.Stunned; // 状態を気絶に変更
        elapsedTimeAfterStun = 0.0f; // 気絶時間の経過時間をリセット

        remainingInvincibleTime = invincibleTime; // 無敵時間を付与

        // 泥棒の表情を気絶の表情に変更する処理を追加する
        ChangeFace(ReactionSpriteType.Stun);


        // 耐久力が0以下になった場合は、耐久力を0に補正して気絶状態にする
        if (durability <= 0)
        {
            durability = 0;

            // プレイヤーにソウルを入手させる
            CS_PlayerAction playerAction = GameObject.FindObjectOfType<CS_PlayerAction>();

            // playerActionが見つかった場合は、ソウルを加算する処理を実行する。見つからない場合は、エラーログを出力する。
            if (playerAction != null)playerAction.AddSoul(soulDropCount);
            else Debug.LogError("PlayerActionが見つかりませんでした。ThiefAIのTakeDamageメソッドで、プレイヤーにソウルを入手させる処理が正常に動作しない可能性があります。");
        }
    }

    /// <summary>
    /// 現在いる部屋に関するオブジェクトをRaycastで取得して、currentRoomに設定する処理
    /// </summary>
    private void FindNowRoomNode()
    {
        GameObject currentobject = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(this.gameObject);
        if (currentobject == null)
        {
            Debug.LogWarning("【泥棒】現在いる部屋に関するオブジェクトの取得に失敗しました");
            return;
        }

        currentRoom = currentobject.transform.GetComponentInChildren<RoomNode>();
        currentRoomObject = currentobject;

        // 現在いる部屋の記憶がない場合は新たに作成
        if (!roomMemories.ContainsKey(currentRoom))
        {
            roomMemories[currentRoom] = new RoomMemory();
            roomMemories[currentRoom].FirstSetting();
            roomMemories[currentRoom].explorationLevel = currentRoom.initialExplorationLevel;
        }
    }

    /// <summary>
    /// 次に設定する移動ポイントを決定するロジック
    /// </summary>
    private void NextDoorElection()
    {
        if (currentRoomObject == null)
        {
            FindNowRoomNode();
            Debug.LogError("【泥棒】現在いる部屋のオブジェクトが見つかりませんでした。ThiefAIのNextDoorElectionメソッドで、次に設定する移動ポイントを決定するロジックが正常に動作しない可能性があります。");
            return;
        }

        CS_RoomCreatePoint roomCreatePoint = currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>();
        if (roomCreatePoint == null)
        {
            Debug.LogError("【泥棒】現在いる部屋のRoomCreatePointが見つかりませんでした。ThiefAIのNextDoorElectionメソッドで、次に設定する移動ポイントを決定するロジックが正常に動作しない可能性があります。");
            return;
        }

        // 現在いる部屋の接続している方向を取得
        List<CSE_RoomDoorDirection> connectDirs = roomCreatePoint.GetConnectDirections();
        if (connectDirs.Count == 0)
        {
            Debug.LogWarning("【泥棒】現在いる部屋の接続方向が見つかりませんでした。ThiefAIのNextDoorElectionメソッドで、次に設定する移動ポイントを決定するロジックが正常に動作しない可能性があります。");
            return;
        }

        // 入ってきたドアをリストから除外
        // もし行ったことのない部屋がある場合は行ったことのある方向をリストから除外
        bool hasUnvisitedNextRooms = HasUnvisitedNextRooms(); // 次の部屋候補の中に行ったことのない部屋があるかどうかを判定するフラグ
        for (int i = 0 ; i < connectDirs.Count ; i++)
        {
            // 入ってきたドアの方向と同じ方向がある場合は、リストから除外
            if (connectDirs[i] == roomMemories[currentRoom].enteredDoorDirection)
            {
                connectDirs.RemoveAt(i);
                i--;
                continue;
            }
            // 次の部屋候補の中に行ったことのない部屋がある場合は、行ったことのある方向があればリストから除外
            if (hasUnvisitedNextRooms)
            {
                CS_RoomMoveConnection nextRoom;
                roomCreatePoint.TryGetConnection(connectDirs[i], out nextRoom);
                if (roomMemories.ContainsKey(nextRoom.TargetCreatePoint.GetComponentInChildren<RoomNode>()))
                {
                    connectDirs.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }

        // 接続している部屋の方向をランダムに選択
        int randomIndex = Random.Range(0, connectDirs.Count);

        // 選択しなかった方向のドアを記憶
        for (int i = 0 ; i < connectDirs.Count ; i++)
        {
            if (i == randomIndex) continue;

            roomMemories[currentRoom].unchosenDoors.Add(connectDirs[i]);
        }

        // 選択した方向にあるドアの位置を次の移動ポイントに設定
        nextRoomMovePoint = currentRoom.GetDirectionWallToDoor(connectDirs[randomIndex]);
    }

    /// <summary>
    /// 次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理
    /// </summary>
    /// <returns>
    /// true:次の部屋候補の中に行ったことのない部屋がある | false:次の部屋候補の中に行ったことのない部屋がない
    /// </returns>
    private bool HasUnvisitedNextRooms()
    {
        if (currentRoomObject == null)
        {
            FindNowRoomNode();
            Debug.LogError("【泥棒】現在いる部屋のオブジェクトが見つかりませんでした。ThiefAIのHasUnvisitedNextRoomsメソッドで、次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理が正常に動作しない可能性があります。");
            return false;
        }
        CS_RoomCreatePoint roomCreatePoint = currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>();
        if (roomCreatePoint == null)
        {
            Debug.LogError("【泥棒】現在いる部屋のRoomCreatePointが見つかりませんでした。ThiefAIのHasUnvisitedNextRoomsメソッドで、次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理が正常に動作しない可能性があります。");
            return false;
        }
        // 現在いる部屋の接続している方向を取得
        List<CSE_RoomDoorDirection> connectDirs = roomCreatePoint.GetConnectDirections();
        if (connectDirs.Count == 0)
        {
            Debug.LogWarning("【泥棒】現在いる部屋の接続方向が見つかりませんでした。ThiefAIのHasUnvisitedNextRoomsメソッドで、次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理が正常に動作しない可能性があります。");
            return false;
        }
        // 接続している部屋の中に行ったことのない部屋があるかどうかを判定
        foreach (var dir in connectDirs)
        {
            CS_RoomMoveConnection nextRoom;
            roomCreatePoint.TryGetConnection(dir, out nextRoom);
            if (!roomMemories.ContainsKey(nextRoom.TargetCreatePoint.GetComponentInChildren<RoomNode>())) return true;
        }
        return false;
    }

    /// <summary>
    /// 探索対象を強制的に変更する処理
    /// (対象：プレイヤーが攻撃してきたときや、ミミックの罠にかかったときなど)
    /// </summary>
    /// <param name="target">新しく設定する探索対象</param>
    public void SetTarget(ThiefTarget target)
    {
        currentTarget = target;
    }

    /// <summary>
    /// 指定のオブジェクトに関する記憶を消去する処理
    /// </summary>
    /// <param name="obj">指定オブジェクト</param>
    public void EraseTheMemory(ThiefTarget obj)
    {
        foreach(var room in roomMemories)
        {
            // 指定のオブジェクトに関する記憶がない場合はスキップ
            if (room.Value.recognizedObjects == null) continue;

            foreach (var entry in room.Value.recognizedObjects)
            {
                // 指定のオブジェクトに関する記憶がある場合は、記憶から削除する
                if (entry == obj)
                {
                    room.Value.recognizedObjects.Remove(entry);
                    break;
                }
            }
        }

        if (currentTarget == obj)
        {
            currentTarget = null;
        }
    }

    /// <summary>
    /// 探索対象の探索にかかる時間を経過させる処理
    /// </summary>
    /// <returns>探索が終了しているかどうか</returns>
    private bool ProgressTargetSearchTime()
    {
        // 探索対象がない場合は、falseを返す
        if (currentTarget == null) return false;

        // 現在の探索対象が視認オブジェクト(VisionTarget)でない場合は、falseを返す
        if (!(currentTarget is VisionTarget)) return false;

        // 探索対象の探索にかかる時間を経過させる
        //　((VisionTarget)currentTarget).explorationProgress　: 対象の探索度(MAX : 100.0f)
        // searchTime : 探索対象の探索にかかる時間
        visionTargetMemories[((VisionTarget)currentTarget)].explorationProgress += (100.0f / searchTime) * Time.deltaTime;

        // 探索対象の探索にかかる時間が経過した場合は、trueを返す
        if (visionTargetMemories[((VisionTarget)currentTarget)].explorationProgress >= 100.0f) return true;

        return false;
    }

    /// <summary>
    /// 泥棒の表情を変更する処理
    /// </summary>
    /// <param name="reaction">変更するタイプ</param>
    private void ChangeFace(ReactionSpriteType reaction)
    {
        // 子オブジェクトを取得
        GameObject child = transform.GetChild(0).gameObject;
        // 取得できなかった場合は、エラーログを出力して処理を終了する
        if (child == null) Debug.LogError("子オブジェクトが見つかりませんでした。ThiefAIのChangeFaceメソッドで、泥棒の表情を変更する処理が正常に動作しない可能性があります。");

        // 子オブジェクトからMeshRendererを取得
        MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
        // 取得できなかった場合は、エラーログを出力して処理を終了する
        if (meshRenderer == null) Debug.LogError(" MeshRendererが見つかりませんでした。ThiefAIのChangeFaceメソッドで、泥棒の表情を変更する処理が正常に動作しない可能性があります。");

        // Materialの取得
        Material material = meshRenderer.material;
        // 取得できなかった場合は、エラーログを出力して処理を終了する
        if (material == null) Debug.LogError(" Materialが見つかりませんでした。ThiefAIのChangeFaceメソッドで、泥棒の表情を変更する処理が正常に動作しない可能性があります。");

        // 表情のスプライトを変更する
        material.mainTexture = reactionSprites[(int)reaction].texture;
    }

    /// <summary>
    /// 指定した位置にワープする処理
    /// </summary>
    /// <param name="targetPos">指定位置</param>
    /// <param name="entryDoorDir">入ってきたドアの方向</param>
    public void WarpAction(Vector3 targetPos, CSE_RoomDoorDirection entryDoorDir)
    {
        // 現在の経路をリセットして、ワープ後に新しい経路を計算させる
        navMeshAgent.ResetPath(); 
        // NavMeshAgentのWarpメソッドを使用して、指定した位置にワープする
        navMeshAgent.Warp(targetPos);

        transform.position = targetPos;
        FindNowRoomNode();

        roomMemories[currentRoom].enteredDoorDirection = entryDoorDir;

        isNextRoomMovePointDecided = false;
        nextRoomMovePoint = null;
        currentTarget = null;

        // 現在の状態が逃走状態の場合
        if (currentState == ThiefState.Escape)
        {
            // ワープする = ドアを通過した
            // escapeRouteの先頭が次の移動ポイントになる想定なので、先頭を削除して次の移動ポイントを設定する
            if (escapeRoute != null && escapeRoute.Count > 0)
            {
                escapeRoute.RemoveAt(0);
                if (escapeRoute.Count > 0)
                {
                    nextRoomMovePoint = escapeRoute[0];
                    isNextRoomMovePointDecided = true;
                }
                else
                {
                    isNextRoomMovePointDecided = false;
                    nextRoomMovePoint = null;
                    Debug.Log("【泥棒】逃走完了");
                    Destroy(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 逃走ルートを構築する処理
    /// </summary>
    /// <remarks>
    /// currentRoom から firstRoom まで、部屋接続(隣接)を BFS で探索してルート(部屋列)を復元する。
    /// ルート上で通る各ドアの Transform を `escapeRoute` に順番に保存する。
    /// </remarks>
    private void EscapeRoute()
    {
        // 前提チェック
        if (currentRoom == null)
        {
            Debug.LogWarning("【泥棒】EscapeRoute: currentRoom が null です。");
            return;
        }
        if (firstRoom == null)
        {
            Debug.LogWarning("【泥棒】EscapeRoute: firstRoom が null です。Setting() で entryRoom を設定しているか確認してください。");
            return;
        }

        // リスト初期化
        if (escapeRoute == null) escapeRoute = new List<Transform>();
        escapeRoute.Clear();

        // 最終目的地（最初に入ってきた入口）を末尾に追加
        if (firstEntryPoint != null)
        {
            escapeRoute.Add(firstEntryPoint);
        }
        else
        {
            Debug.LogWarning("【泥棒】EscapeRoute: firstEntryPoint が null のため最終目的地を追加できません。entry時に設定しているか確認してください。");
        }

        // 既に最初の部屋にいるなら、帰宅ルート不要
        if (currentRoom == firstRoom)
        {
            isNextRoomMovePointDecided = false;
            nextRoomMovePoint = null;
            return;
        }

        // BFS 用
        var queue = new Queue<RoomNode>();
        var visited = new HashSet<RoomNode>();
        var parent = new Dictionary<RoomNode, RoomNode>();

        queue.Enqueue(currentRoom);
        visited.Add(currentRoom);
        parent[currentRoom] = null;

        bool found = false;

        // currentRoom から firstRoom を探索
        while (queue.Count > 0)
        {
            RoomNode room = queue.Dequeue();
            if (room == null) continue;

            if (room == firstRoom)
            {
                found = true;
                break;
            }

            Transform createPointTr = room.gameObject.transform.parent;
            if (createPointTr == null) continue;

            CS_RoomCreatePoint roomCreatePoint = createPointTr.GetComponent<CS_RoomCreatePoint>();
            if (roomCreatePoint == null) continue;

            for (int i = 0 ; i < 4 ; i++)
            {
                CS_RoomMoveConnection connection;
                if (!roomCreatePoint.TryGetConnection((CSE_RoomDoorDirection)i, out connection)) continue;
                if (connection == null || connection.TargetCreatePoint == null) continue;

                RoomNode adjacentRoom = connection.TargetCreatePoint.gameObject.GetComponentInChildren<RoomNode>();
                if (adjacentRoom == null) continue;

                if (visited.Contains(adjacentRoom)) continue;

                visited.Add(adjacentRoom);
                parent[adjacentRoom] = room;
                queue.Enqueue(adjacentRoom);
            }
        }

        if (!found)
        {
            Debug.LogWarning("【泥棒】EscapeRoute: firstRoom までのルートが見つかりませんでした。");
            return;
        }

        // 経路復元: currentRoom -> ... -> firstRoom
        var pathRooms = new List<RoomNode>();
        RoomNode cur = firstRoom;
        while (cur != null)
        {
            pathRooms.Add(cur);
            parent.TryGetValue(cur, out cur);
        }
        pathRooms.Reverse(); // currentRoom が先頭になる

        if (pathRooms.Count < 2)
        {
            Debug.LogWarning("【泥棒】EscapeRoute: 復元した経路が不正です。");
            return;
        }

        // 部屋列から「通るドア列(escapeRoute)」を作る
        for (int idx = 0 ; idx < pathRooms.Count - 1 ; idx++)
        {
            RoomNode fromRoom = pathRooms[idx];
            RoomNode toRoom = pathRooms[idx + 1];
            if (fromRoom == null || toRoom == null) continue;

            Transform fromCreatePointTr = fromRoom.gameObject.transform.parent;
            if (fromCreatePointTr == null) continue;

            CS_RoomCreatePoint fromCreatePoint = fromCreatePointTr.GetComponent<CS_RoomCreatePoint>();
            if (fromCreatePoint == null) continue;

            CSE_RoomDoorDirection? dirToNext = null;

            // fromRoom 側の接続を見て、toRoom に繋がる方向を特定
            for (int d = 0 ; d < 4 ; d++)
            {
                CS_RoomMoveConnection connection;
                if (!fromCreatePoint.TryGetConnection((CSE_RoomDoorDirection)d, out connection)) continue;
                if (connection == null || connection.TargetCreatePoint == null) continue;

                RoomNode adjacentRoom = connection.TargetCreatePoint.gameObject.GetComponentInChildren<RoomNode>();
                if (adjacentRoom == toRoom)
                {
                    dirToNext = (CSE_RoomDoorDirection)d;
                    break;
                }
            }

            if (dirToNext == null)
            {
                Debug.LogWarning("【泥棒】EscapeRoute: 次の部屋への接続方向を特定できませんでした。");
                continue;
            }

            // NextDoorElection() と同様に、RoomNode からドアTransformを取得して保存
            Transform doorTr = fromRoom.GetDirectionWallToDoor(dirToNext.Value);
            if (doorTr == null)
            {
                Debug.LogWarning("【泥棒】EscapeRoute: ドア Transform の取得に失敗しました。");
                continue;
            }

            escapeRoute.Add(doorTr);
        }

        // ついでに「次に向かうドア」を nextRoomMovePoint に入れておく（既存仕様と互換）
        if (escapeRoute.Count > 0)
        {
            nextRoomMovePoint = escapeRoute[0];
            isNextRoomMovePointDecided = true;
        }
    }

    //////////////////////////////////////////////////////////////////
    /// デバック用の処理

    [ContextMenu("ダメージを与える")]
    private void DebugTakeDamage()
    {
        TakeDamage(1, Gimmick.Pot);
    }
}

