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

    [Tooltip("現在いる部屋の情報")]
    private RoomNode currentRoom;
    private GameObject currentRoomObject;
    [Tooltip("部屋に関する記憶")]
    private Dictionary<RoomNode, RoomMemory> roomMemories;
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

    // 泥棒の耐久力と移動速度を設定するメソッド
    public void Setting(ThiefTypeData typedata, ThiefData data, float playerSpeed, RoomNode firstRoom)
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

        // 初期状態を探索に設定
        currentState = ThiefState.Explore;

        // 初期部屋を設定（仮）
        currentRoom = firstRoom;

        // 部屋の記憶を初期化
        roomMemories = new Dictionary<RoomNode, RoomMemory>();

        // 初期部屋の記憶を作成
        roomMemories[currentRoom] = new RoomMemory();
        roomMemories[currentRoom].FirstSetting();

        // ナビメッシュエージェントの速度を設定
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.baseOffset = 1.0f; // キャラクターの高さに合わせてオフセットを設定
        navMeshAgent.speed = this.walkSpeed;

        // 泥棒のリアクションを管理するコンポーネントを取得
        thiefReaction = GetComponent<ThiefReaction>();
    }

    private void Start()
    {
        FindNowRoomNode();
    }

    private void Update()
    {
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

        if (isNextRoomMovePointDecided || roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
        {
            if (nextRoomMovePoint == null) NextDoorElection();

            navMeshAgent.SetDestination(nextRoomMovePoint.position);

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
            if (((VisionTarget)currentTarget).isExplored)
            {
                DecideTarget();
            }
            // 探索対象が未探索の場合
            else
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

                // 探索対象に向かって移動
                navMeshAgent.SetDestination(currentTarget.transform.position);

                // 探索対象に十分近づいたら、探索度を進める
                if (Vector3.Distance(transform.position, currentTarget.transform.position) < ((VisionTarget)currentTarget).exploredDistanceThreshold)
                {
                    ChangeFace(ReactionSpriteType.Search); // 探索完了の表情に変更する処理を追加する

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
                            ((VisionTarget)currentTarget).isExplored = true;
                            // 探索度を加算
                            roomMemories[currentRoom].explorationLevel += ((VisionTarget)currentTarget).explorationValue;

                            // 探索度が閾値を超えた場合は、次の部屋に移動するための処理を追加する
                            if (roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
                            {
                                // 探索対象をリセット
                                currentTarget = null;

                                isNextRoomMovePointDecided = true;
                            }
                            // それ以外の場合は、次の探索対象を決定する
                            else DecideTarget();
                        }
                    }
                }
                else ((VisionTarget)currentTarget).explorationProgress = 0; // 探索対象から十分に離れた場合は、探索進行度をリセットする
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
    // TODO: 現在位置から出口までのルート構築処理を追加する(A*アルゴリズムを利用)
    //     : 宝物発見バフの適応処理を追加する
    private void Found()
    {
        // 宝物を持つ
        heldTreasure = currentTarget.gameObject;
        currentTarget.gameObject.transform.parent = this.transform; // 泥棒の子オブジェクトにする
        currentTarget.GetComponent<Collider>().enabled = false; // 宝物のコライダーを無効にする

        // 状態を逃走に変更
        currentState = ThiefState.Escape;

        // 取得した宝物を他の泥棒の記憶から消去する
        GameObject.FindObjectOfType<ThiefManager>().EraseTheMemoryToAllThief(currentTarget);
        // 探索対象をリセット
        currentTarget = null;

        float distanceToTarget = Mathf.Infinity;
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

    // 逃走状態の行動
    // (仮)移動ポイントを探索対象にして移動する処理を追加する
    // ----------------
    // TODO: 構築したルートに沿って移動する処理を追加する
    private void Escape()
    {

        // 探索対象に向かって移動
        navMeshAgent.SetDestination(currentTarget.transform.position);

        // 探索対象に十分近づいたら、次の探索対象を決定
        if (Vector3.Distance(transform.position, currentTarget.transform.position) < 2.0f)
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

    // 気絶状態の行動
    // ----------------
    // TODO: その場で動けなくなる処理を追加する
    //     : 退場するときは徐々に消えるようにする処理を追加する
    private void Stunned()
    {
        // ナビメッシュエージェントを停止させる
        navMeshAgent.isStopped = true;

        // 経過時間を加算
        elapsedTimeAfterStun += Time.deltaTime;
        // 経過時間が退場するまでの時間を超えた場合は、退場する処理を追加する
        if (elapsedTimeAfterStun >= exitAfterStunTime)
        {
            // 退場する処理を追加する
            Debug.Log("泥棒が退場");
            
            //Destroy(gameObject);
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
            if (isAlreadyRecognized) continue;

            // 新しいオブジェクトを記憶に追加
            roomMemories[currentRoom].recognizedObjects.Add(target);
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
            // 未探索のオブジェクトがある場合はtrueを返す
            if (!entry.isExplored) return true;
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
            foreach (var entry in roomMemories[currentRoom].recognizedObjects)
            {
                // 既に探索済みのオブジェクトはスキップ
                if (entry.isExplored) continue;

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
    }

    /// <summary>
    ///  耐久値を減らす処理
    /// </summary>
    /// <param name="damage">与える減少値</param>
    public void TakeDamage(int damage, Gimmick type)
    {
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


        // 耐久力が0以下になった場合は、耐久力を0に補正して気絶状態にする
        if (durability <= 0)
        {
            durability = 0;
            currentState = ThiefState.Stunned;

            // 泥棒の表情を気絶の表情に変更する処理を追加する
            ChangeFace(ReactionSpriteType.Stun);

            // プレイヤーにソウルを入手させる
            PlayerAction playerAction = GameObject.FindObjectOfType<PlayerAction>();

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
        if (currentobject == null) Debug.LogWarning("【泥棒】現在いる部屋に関するオブジェクトの取得に失敗しました");
        currentRoom = currentobject.transform.GetComponentInChildren<RoomNode>();
        currentRoomObject = currentobject;
    }

    /// <summary>
    /// 次に設定する移動ポイントを決定するロジック
    /// </summary>
    private void NextDoorElection()
    {
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

        // 接続している部屋の方向をランダムに選択
        int randomIndex = Random.Range(0, connectDirs.Count);

        // 選択した方向にあるドアの位置を次の移動ポイントに設定
        nextRoomMovePoint = currentRoom.GetDirectionWallToDoor(connectDirs[randomIndex]);
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

        // 探索対象の探索にかかる時間を経過させる
        //　((VisionTarget)currentTarget).explorationProgress　: 対象の探索度(MAX : 100.0f)
        // searchTime : 探索対象の探索にかかる時間
        ((VisionTarget)currentTarget).explorationProgress += (100.0f / searchTime) * Time.deltaTime;

        // 探索対象の探索にかかる時間が経過した場合は、trueを返す
        if (((VisionTarget)currentTarget).explorationProgress >= 100.0f) return true;

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

    //////////////////////////////////////////////////////////////////
    /// デバック用の処理

    [ContextMenu("ダメージを与える")]
    private void DebugTakeDamage()
    {
        TakeDamage(1, Gimmick.Pot);
    }

}

