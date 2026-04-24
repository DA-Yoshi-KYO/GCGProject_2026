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
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 泥棒のAIシステム
[RequireComponent(typeof(NavMeshAgent))]
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

    [Tooltip("現在の行動状態")]
    private ThiefState currentState;

    [Tooltip("現在いる部屋の情報")]
    private RoomNode currentRoom;
    [Tooltip("部屋に関する記憶")]
    private Dictionary<RoomNode, RoomMemory> roomMemories;
    [Tooltip("探索対象")]
    private ThiefTarget currentTarget;
    public ThiefTarget CurrentTarget => currentTarget;

    [Tooltip("泥棒の耐久力")]
    private int durability;
    [Tooltip("泥棒の移動速度")]
    private float walkSpeed;
    private float runSpeed;

    [Tooltip("ドロップするソウルの数")]
    private int soulDropCount;
    public int SoulDropCount => soulDropCount;

    [Tooltip("走り状態になる標的オブジェクトのタイプリスト")]
    private List<VisionTarget.TargetType> runTargetTypes;

    [Tooltip("次の部屋探索に切り替える探索度の閾値")]
    private int nextRoomSearchThreshold;

    [Tooltip("ナビメッシュエージェント")]
    private NavMeshAgent navMeshAgent;

    // 泥棒の耐久力と移動速度を設定するメソッド
    public void Setting(ThiefData data, float playerSpeed, RoomNode firstRoom)
    {
        /*未実装、未設定　*///data.jumpHeight;
        /*未設定、未設定　*///data.alertTime;

        durability = data.durability;
        walkSpeed = playerSpeed * data.walkSpeedMultiplier;
        runSpeed = playerSpeed * data.runSpeedMultiplier;
        nextRoomSearchThreshold = data.nextRoomSearchThreshold;
        runTargetTypes = data.runTargetTypes;
        soulDropCount = data.soulDropCount;

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

                // 探索対象に十分近づいたら、探索完了とする
                if (Vector3.Distance(transform.position, currentTarget.transform.position) < ((VisionTarget)currentTarget).exploredDistanceThreshold)
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
                            Debug.Log("次の部屋に移動");
                        }
                        // それ以外の場合は、次の探索対象を決定する
                        else DecideTarget();
                    }
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
    // TODO: 現在位置から出口までのルート構築処理を追加する(A*アルゴリズムを利用)
    //     : 宝物発見バフの適応処理を追加する
    private void Found()
    { 
        // 状態を逃走に変更
        currentState = ThiefState.Escape;
    }

    // 逃走状態の行動
    // ----------------
    // TODO: 構築したルートに沿って移動する処理を追加する
    private void Escape()
    { 
    }

    // 気絶状態の行動
    // ----------------
    // TODO:その場で動けなくなる処理を追加する
    private void Stunned()
    {

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

            // オブジェクトが部屋の記憶にない場合は新たに追加
            if (roomMemories[currentRoom].recognizedObjects == null) roomMemories[currentRoom].recognizedObjects = new List<ThiefTarget>();

            // 現在の部屋の記憶にオブジェクトを追加

            foreach (var entry in roomMemories[currentRoom].recognizedObjects)
            {
                // 既に記憶しているオブジェクトの場合はスキップ
                if (entry == target) continue;
            }

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
            // 視認したオブジェクトの中から、未探索で近いものを優先して探索対象に設定
            foreach (var entry in roomMemories[currentRoom].recognizedObjects)
            {
                // 既に探索済みのオブジェクトはスキップ
                if (entry.isExplored) continue;

                // 現在の探索対象が視認オブジェクト(VisionTarget)かどうか
                if (currentTarget is VisionTarget)
                {
                    // 探索対象の優先順位を決めるロジック
                    switch (((VisionTarget)currentTarget).targetType)
                    {
                        case VisionTarget.TargetType.Treasure:
                            {
                                // より近い宝物を探索対象に設定
                                if (entry is VisionTarget)
                                {
                                    if (((VisionTarget)entry).targetType != VisionTarget.TargetType.Treasure) continue;
                                }
                                else if (entry is TrapTarget)
                                {
                                    // 宝物罠の場合のみ、処理を継続する
                                }
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
                        case VisionTarget.TargetType.Player:
                            {
                                // 現在の探索対象が宝物の場合は、プレイヤーを探索対象に設定しない
                                if (((VisionTarget)currentTarget).targetType == VisionTarget.TargetType.Treasure) continue;
                                // 宝物罠の場合も、プレイヤーを探索対象に設定しない
                                // if (currentTarget is TrapTarget) continue;

                                currentTarget = entry;
                            }
                            break;
                        case VisionTarget.TargetType.RoomObject:
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
                            break;
                    }
                }
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
        }
        // 未探索のオブジェクトがない場合は、部屋の移動ルートに沿って移動する処理を追加する
        else
        {
            // 前回の探索対象が視認オブジェクト(VisionTarget)かどうか
            if (currentTarget == null || currentTarget is VisionTarget || currentTarget is TrapTarget)
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
                        if (currentRoom.isRight)
                        {
                            // 次のインデックスを計算
                            nextIndex = i + 1;

                            // インデックスがリストの範囲を超える場合は、リストの先頭に戻す
                            if (nextIndex >= currentRoom.movePoints.Count) nextIndex = 0;

                            // リストを加算して次の移動ポイントを探索対象に設定
                            currentTarget = currentRoom.movePoints[nextIndex];
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
    public void TakeDamage(int damage)
    {
        durability -= damage;

        // 耐久力が0以下になった場合は、耐久力を0に補正して気絶状態にする
        if (durability <= 0)
        {
            durability = 0;
            currentState = ThiefState.Stunned;
        }
    }

    /// <summary>
    /// 探索対象を強制的に変更する処理
    /// (対象：プレイヤーが攻撃してきたときや、ミミックの罠にかかったときなど)
    /// </summary>
    /// <param name="target"></param>
    public void SetTarget(ThiefTarget target)
    {
        currentTarget = target;
    }
}

