/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の記憶に関するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 泥棒の記憶に関するシステムを管理するクラス
/// </summary>
public class CS_MemorySystem
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    [Tooltip("探索対象")]
    private CS_ThiefTarget currentTarget;
    public CS_ThiefTarget read_CurrentTarget => currentTarget;

    [Tooltip("現在いる部屋の情報")]
    private CS_RoomNode currentRoom;
    public CS_RoomNode read_CurrentRoom => currentRoom;

    private GameObject currentRoomObject;

    [Tooltip("部屋に関する記憶")]
    private Dictionary<CS_RoomNode, CS_RoomMemory> roomMemories;

    [Tooltip("デバック用：部屋に関する記憶を外部から参照するためのプロパティ")]
    public IReadOnlyDictionary<CS_RoomNode, CS_RoomMemory> read_RoomMemorys => roomMemories;

    [Tooltip("視認オブジェクトの記憶")]
    private Dictionary<CS_VisionTarget, CS_VisionTargetMemory> visionTargetMemories;

    [Tooltip("プレイヤーを無視するフラグ")]
    private bool ignorePlayer = false;
    [Tooltip("プレイヤーを追跡する残り時間")]
    private float remainingIgnorePlayerTime;
    [Tooltip("プレイヤーを追跡する残り時間の初期値")]
    private float initialRemainingIgnorePlayerTime;

    [Tooltip("泥棒が探索するのにかかる秒数")]
    private List<int> searchTime;

    [Tooltip("次の部屋探索に切り替える探索度の閾値")]
    private int nextRoomSearchThreshold;

    [Tooltip("最初の部屋のオブジェクト")]
    private CS_RoomNode firstRoom;
    public CS_RoomNode read_FirstRoom => firstRoom;

    [Tooltip("最初の部屋の入ってきたドアの位置")]
    private Transform firstEntryPoint;
    public Transform read_FirstEntryPoint => firstEntryPoint;

    [Tooltip("この泥棒が回避する DangerZone の zoneID 一覧")]
    private List<int> avoidZoneIDs = new List<int>();

    /// <summary>
    /// 記憶システムの初期設定を行う処理
    /// </summary>
    /// <param name="thiefAI">ThiefAIのメインスクリプト</param>
    /// <param name="entryRoom">最初の部屋の情報</param>
    /// <param name="entryPoint">最初の部屋の入ってきたドアの位置</param>
    public CS_MemorySystem(CS_ThiefAI thiefAI, CS_RoomNode entryRoom, Transform entryPoint, CO_ThiefStatusData typedata)
    {
        // ThiefAIのメインスクリプトを取得
        this.thiefAI = thiefAI;
        // 最初の部屋の情報を保存
        this.firstRoom = entryRoom;
        this.firstEntryPoint = entryPoint;

        // プレイヤーを追跡する残り時間の初期値を設定
        this.initialRemainingIgnorePlayerTime = typedata.pursuitTime;
        this.remainingIgnorePlayerTime = typedata.pursuitTime;

        // 次の部屋探索に切り替える探索度の閾値を設定
        this.nextRoomSearchThreshold = typedata.nextRoomSearchThreshold;

        // 探索対象の探索にかかる時間を設定
        this.searchTime = typedata.searchTime;

        // 記憶領域の作成
        this.roomMemories = new Dictionary<CS_RoomNode, CS_RoomMemory>();
        this.visionTargetMemories = new Dictionary<CS_VisionTarget, CS_VisionTargetMemory>();
    }

    /// <summary>
    /// 部屋のオブジェクトを視認して記憶に保存する処理
    /// </summary>
    public void RecognizeObjects()
    {
        // 視界内オブジェクトを取得
        List<CS_ThiefTarget> visionTargets = thiefAI.read_VisionSensor.Scan();

        // 現在の部屋の記憶がない場合は新たに作成
        if (roomMemories[currentRoom] == null)
        {
            roomMemories[currentRoom] = new CS_RoomMemory();
            roomMemories[currentRoom].FirstSetting();
        }

        bool isPlayerInVision = false;
        if (currentTarget is CS_PlayerTarget)
        {
            // 視認した中にプレイヤーがいない場合は、探索対象からプレイヤーを外す
            foreach (CS_ThiefTarget target in visionTargets)
            {
                if (target is CS_PlayerTarget)
                {
                    isPlayerInVision = true;
                    break;
                }
            }

            // プレイヤーが視認できている場合は、プレイヤーを探索対象に設定し続ける
            if (isPlayerInVision)
            {
                // 耐久値が1以上ある場合は、秒数であきらめる
                if (thiefAI.read_Durability > 1)
                {
                    // 追跡する残り時間が0以下の場合は、プレイヤーを無視するフラグを立てる
                    if (remainingIgnorePlayerTime <= 0.0f)
                    {
                        ignorePlayer = true;
                        isPlayerInVision = false; // プレイヤーを無視するフラグを立てた場合は、プレイヤーが視認できていても、プレイヤーが視認できていない状態にする
                        remainingIgnorePlayerTime = 0.0f;
                        ClearTarget();
                    }
                    else remainingIgnorePlayerTime -= Time.deltaTime;
                }
            }
            else
            {
                // 追跡時間が残っている場合
                if (remainingIgnorePlayerTime > 0.0f)
                {
                    // 最大値まで回復させる
                    remainingIgnorePlayerTime = initialRemainingIgnorePlayerTime;
                }
            }

            if (!isPlayerInVision)
            {
                ClearTarget();
                thiefAI.read_ThiefReaction.ClearReaction();
            }
            else
            {
                thiefAI.read_ThiefReaction.ChangeReaction(CS_ThiefReaction.ThiefReactionType.ChasingCat);
                thiefAI.read_MoveSystem.MoveTo(currentTarget.transform.position);
            }

            return;
        }

        // 視認したオブジェクトを記憶に保存
        foreach (CS_ThiefTarget target in visionTargets)
        {
            // 現在の部屋の記憶に認識しているオブジェクトのリストがない場合は新たに作成
            if (roomMemories[currentRoom].recognizedObjects == null) roomMemories[currentRoom].recognizedObjects = new List<CS_ThiefTarget>();

            bool isAlreadyRecognized = false; // 既に記憶しているオブジェクトかどうかを判定するフラグ
            foreach (var entry in roomMemories[currentRoom].recognizedObjects)
            {
                // 既に記憶しているオブジェクトの場合はスキップ
                if (entry == target) isAlreadyRecognized = true;
            }
            if (isAlreadyRecognized)
            {
                if (target is CS_VisionTarget)
                {
                    // 既に記憶しているオブジェクトが視認オブジェクト(VisionTarget)の場合は、探索している人がいるかどうかの情報を更新する
                    if (visionTargetMemories.ContainsKey((CS_VisionTarget)target))
                    {
                        visionTargetMemories[((CS_VisionTarget)target)].searchThief = ((CS_VisionTarget)target).searchThief;
                    }
                }

                continue;
            }


            if (target is CS_PlayerTarget)
            {
                // 耐久地が1以上あって、プレイヤーを無視するフラグが立っている場合は、プレイヤーを探索対象に設定しない
                if (thiefAI.read_Durability > 1 && ignorePlayer)
                {
                    continue;
                }

                // 現在の探索対象が宝物である場合は、プレイヤーを探索対象に設定しない
                if (currentTarget is CS_VisionTarget && ((CS_VisionTarget)currentTarget).targetType == CS_VisionTarget.TargetType.Treasure)
                {
                    continue;
                }

                // 現在の探索対象が空の宝箱型の罠である場合は、プレイヤーを探索対象に設定しない
                if (currentTarget is CS_TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest)
                {
                    continue;
                }

                // 今回初めてプレイヤーを視認した場合
                if (!isPlayerInVision)
                {
                    // プレイヤーの追跡を開始した場合のSEを再生する
                    if (thiefAI.read_ThiefSound != null)
                    {
                        thiefAI.read_ThiefSound.PlayOneShotSE("ThiefDiscover", thiefAI.gameObject.transform.position, "ThiefDiscover");
                    }
                }

                currentTarget = target;
                continue;
            }

            // 新しいオブジェクトを記憶に追加
            roomMemories[currentRoom].recognizedObjects.Add(target);

            // 記憶領域の作成
            if (target is CS_VisionTarget) visionTargetMemories[((CS_VisionTarget)target)] = new CS_VisionTargetMemory();
        }

        // 新たに視認したオブジェクトを記憶に保存した後、探索対象を決定する処理を追加する
        DecideTarget();
    }

    /// <summary>
    /// 未探索のオブジェクトのみを格納したリストを作成して返す処理
    /// </summary>
    /// <returns>
    ///　未探索のオブジェクトのみを格納したリスト
    /// </returns>
    private List<CS_ThiefTarget> GetUnExploredObjectsList()
    {
        List<CS_ThiefTarget> unexploredObjects = new List<CS_ThiefTarget>();
        // 現在の部屋の記憶がない場合や、認識しているオブジェクトがない場合は、未探索のオブジェクトがないと判定して空のリストを返す
        if (roomMemories[currentRoom] == null || roomMemories[currentRoom].recognizedObjects == null) return unexploredObjects;
        // 視認しているオブジェクトの中から未探索のもののみをリストに追加する
        foreach (var entry in roomMemories[currentRoom].recognizedObjects)
        {
            // 未探索オブジェクト
            if (!visionTargetMemories[((CS_VisionTarget)entry)].isExplored)
            {
                // 探索している人がいない場合や、探索している人が自分である場合は、未探索のオブジェクトとしてリストに追加する
                if (visionTargetMemories[((CS_VisionTarget)entry)].searchThief == null || visionTargetMemories[((CS_VisionTarget)entry)].searchThief == thiefAI.gameObject)
                {
                    unexploredObjects.Add(entry);
                }
            }
        }
        return unexploredObjects;
    }

    /// <summary>
    /// 指定のオブジェクトに関する記憶を消去する処理
    /// </summary>
    /// <param name="obj">指定オブジェクト</param>
    public void EraseTheMemory(CS_ThiefTarget obj)
    {
        foreach (var room in roomMemories)
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
            ClearTarget();
        }
    }

    /// <summary>
    /// 探索対象を決める処理
    /// </summary>
    private void DecideTarget()
    {
        // 音に反応している場合は、探索対象を決めない
        if (thiefAI.read_HearingSystem.read_IsReactingToSound) return;

        // 未探索のオブジェクトのみを格納したリストを取得する
        List<CS_ThiefTarget> unexploredObjects = GetUnExploredObjectsList();

        // 未探索のオブジェクトがない場合は、部屋の移動ルートに沿って移動する処理を追加する
        if (unexploredObjects.Count == 0)
        {
            // 移動ルートを構築している場合は、探索対象を決めない
            if (thiefAI.read_AStarSystem.HasRoute) return;
            // 現在の追跡対象がプレイヤーの場合は、探索対象を決めない
            if (currentTarget is CS_PlayerTarget) return;

            DecideTargetMovePoint();
        }
        else
        {
            // 探索対象との距離
            float distanceToTarget = Mathf.Infinity;

            // 標的を変更したかどうかを判定するフラグ
            bool isChangeTarget = false;

            if (currentTarget != null)
            {
                distanceToTarget = Vector3.Distance(thiefAI.transform.position, currentTarget.transform.position);
            }

            // 未探索のオブジェクトがある場合は、未探索のオブジェクトを優先して探索対象に設定
            foreach (var entry in unexploredObjects)
            {
                // 現在の探索対象が視認オブジェクト(VisionTarget)かどうか
                if (entry is CS_VisionTarget)
                {
                    // 探索対象の優先順位を決めるロジック
                    switch (((CS_VisionTarget)entry).targetType)
                    {
                        case CS_VisionTarget.TargetType.Treasure:
                            {
                                if (currentTarget is CS_VisionTarget)
                                {
                                    // 現在の探索対象が宝物でない場合は、問答無用で宝物を探索対象に設定
                                    if (((CS_VisionTarget)currentTarget).targetType != CS_VisionTarget.TargetType.Treasure)
                                    {
                                        currentTarget = entry;
                                        isChangeTarget = true;
                                        break;
                                    }
                                    // 現在の探索対象も宝物の場合は、距離が近い方を探索対象に設定する
                                    else
                                    {
                                        // オブジェクトとの距離を計算
                                        float distance = Vector3.Distance(thiefAI.transform.position, entry.transform.position);

                                        // より近いオブジェクトを探索対象に設定
                                        if (distance < distanceToTarget)
                                        {
                                            distanceToTarget = distance;
                                            currentTarget = entry;
                                            isChangeTarget = true;
                                        }
                                        else continue;
                                    }
                                }
                                else if (currentTarget is CS_TrapTarget)
                                {

                                    // 空の宝箱型の罠の場合ではない場合は、スキップ
                                    if (entry is CS_TrapTarget tt && tt.gimmickScript.gimmick != Gimmick.EmptyChest) continue;

                                    // 宝物罠の場合は、距離判定で探索対象を切り替える
                                    // オブジェクトとの距離を計算
                                    float distance = Vector3.Distance(thiefAI.transform.position, entry.transform.position);

                                    // より近いオブジェクトを探索対象に設定
                                    if (distance < distanceToTarget)
                                    {
                                        distanceToTarget = distance;
                                        currentTarget = entry;
                                        isChangeTarget = true;
                                    }
                                    else continue;
                                }
                                else
                                {
                                    // プレイヤーを探索対象にしている場合は、問答無用で宝物を探索対象に設定
                                    currentTarget = entry;
                                    isChangeTarget = true;
                                }
                            }
                            break;
                        case CS_VisionTarget.TargetType.Shelf:
                            {
                                // 現在の探索対象が宝物の場合は、スキップ
                                if (currentTarget is CS_VisionTarget vt && vt.targetType == CS_VisionTarget.TargetType.Treasure) continue;
                                // 現在の探索対象が空の宝箱型の罠の場合は、スキップ
                                if (currentTarget is CS_TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest) continue;
                                // 現在の探索対象がプレイヤーの場合は、スキップ
                                if (currentTarget is CS_PlayerTarget) continue;

                                // オブジェクトとの距離を計算
                                float distance = Vector3.Distance(thiefAI.transform.position, entry.transform.position);

                                // より近いオブジェクトを探索対象に設定
                                if (distance < distanceToTarget)
                                {
                                    distanceToTarget = distance;
                                    currentTarget = entry;
                                    isChangeTarget = true;
                                }
                                else continue;
                            }
                            break;
                    }
                }
                else if (entry is CS_TrapTarget)
                {
                    // 宝物を探索対象にしている場合は、スキップ
                    if (currentTarget is CS_VisionTarget vt && vt.targetType == CS_VisionTarget.TargetType.Treasure) continue;
                    // 宝物の罠を探索対象にしている場合は、スキップ
                    if (currentTarget is CS_TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest) continue;

                    // オブジェクトとの距離を計算
                    float distance = Vector3.Distance(thiefAI.transform.position, entry.transform.position);
                    // より近いオブジェクトを探索対象に設定
                    if (distance < distanceToTarget)
                    {
                        distanceToTarget = distance;
                        currentTarget = entry;
                        isChangeTarget = true;
                    }
                    else continue;
                }
            }

            // 探索対象を変更したときは、移動システムに通知する
            if (isChangeTarget)
            {
                // 探索対象を変更したときの移動速度を更新する
                thiefAI.read_MoveSystem.UpdateMoveSpeed(currentTarget);

                // 探索対象に向かって移動
                thiefAI.read_MoveSystem.MoveTo(currentTarget.transform.position);
            }
        }
    }

    /// <summary>
    /// 部屋の移動ルートに沿って移動するための探索対象を決める処理
    /// </summary>
    public void DecideTargetMovePoint()
    {
        bool isChangeTarget = false;
        // 探索対象との距離
        float distanceToTarget = -1;
        // 前回の探索対象がThiefTargetの派生クラスかどうか(前回が移動ポイントでない場合)
        if (currentTarget == null || currentTarget is CS_VisionTarget || currentTarget is CS_TrapTarget || currentTarget is CS_PlayerTarget)
        {
            // 視認オブジェクトから移動ポイントにする場合は一番近いものを探索対象に設定
            foreach (CS_ThiefTarget target in currentRoom.movePoints)
            {
                if (target == null) continue;

                // オブジェクトとの距離を計算
                float distance = Vector3.Distance(thiefAI.transform.position, target.transform.position);
                // より近いオブジェクトを探索対象に設定
                if (distance > distanceToTarget)
                {
                    distanceToTarget = distance;
                    currentTarget = target;
                    isChangeTarget = true;
                }
                else continue;
            }
        }
        // 移動ポイントから移動ポイントにする場合は、右回りの場合リストを加算、左回りの場合リストを減算して設定
        else
        {
            if (Vector3.Distance(thiefAI.transform.position, currentTarget.transform.position) > 1.0f)
            {
                return;
            }

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

                        isChangeTarget = true;
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

                        isChangeTarget = true;
                        break;
                    }
                }
            }
        }

        if (isChangeTarget)
        {
            // 探索対象を変更したときの移動速度を更新する
            thiefAI.read_MoveSystem.UpdateMoveSpeed(currentTarget);
            // 探索対象に向かって移動
            thiefAI.read_MoveSystem.MoveTo(currentTarget.transform.position);
        }
    }

    /// <summary>
    /// 探索対象を強制的に変更する処理
    /// (対象：プレイヤーが攻撃してきたときや、ミミックの罠にかかったときなど)
    /// </summary>
    /// <param name="target">新しく設定する探索対象</param>
    public void SetTarget(CS_ThiefTarget target)
    {
        currentTarget = target;
        // 探索対象を変更したときの移動速度を更新する
        thiefAI.read_MoveSystem.UpdateMoveSpeed(currentTarget);
        // 探索対象に向かって移動
        thiefAI.read_MoveSystem.MoveTo(currentTarget.transform.position);
    }

    /// <summary>
    /// 探索対象をリセット
    /// </summary>
    public void ClearTarget()
    {
        if (currentTarget is CS_VisionTarget)
        {
            ((CS_VisionTarget)currentTarget).searchThief = null; // 探索対象の探索している人をリセットする)
        }

        currentTarget = null;
    }

    /// <summary>
    /// プレイヤーを無視するフラグをリセットする処理
    /// </summary>
    public void ResetIgnorePlayer()
    {
        ignorePlayer = false;
        remainingIgnorePlayerTime = initialRemainingIgnorePlayerTime;
    }

    /// <summary>
    /// 探索対象に到達しているかどうかを判定する処理
    /// </summary>
    /// <param name="exploredDistanceThreshold">探索対象に到達していると判定する距離の閾値</param>
    /// <returns> 探索対象に到達しているかどうか</returns>
    public bool IsAtTarget(float exploredDistanceThreshold)
    {
        // 探索対象がない場合は、探索対象に到達していないと判定してfalseを返す
        if (currentTarget == null) return false;

        // 探索対象への距離を計算
        float distanceToTarget = Vector3.Distance(thiefAI.transform.position, currentTarget.transform.position);

        // 探索対象への距離が閾値以下になっている場合は、探索対象に到達していると判定してtrueを返す
        return distanceToTarget <= exploredDistanceThreshold;
    }

    /// <summary>
    /// 探索対象の探索にかかる時間を経過させる処理
    /// </summary>
    /// <returns>探索が終了しているかどうか</returns>
    public bool ProgressTargetSearchTime()
    {
        // 探索対象がない場合は、falseを返す
        if (currentTarget == null) return false;

        // 現在の探索対象が視認オブジェクト(VisionTarget)でない場合は、falseを返す
        if (!(currentTarget is CS_VisionTarget)) return false;

        // 探索対象の探索にかかる時間を経過させる
        //　((VisionTarget)currentTarget).explorationProgress　: 対象の探索度(MAX : 100.0f)
        // searchTime : 探索対象の探索にかかる時間
        visionTargetMemories[((CS_VisionTarget)currentTarget)].explorationProgress += (100.0f / searchTime[((int)((CS_VisionTarget)currentTarget).targetType)]) * Time.deltaTime;

        // 探索度が100%以上になった場合は、探索が終了していると判定してtrueを返す
        if (visionTargetMemories[((CS_VisionTarget)currentTarget)].explorationProgress >= 100.0f)
        {
            thiefAI.read_ThiefReaction.ClearReaction(); // 探索が終了したときのリアクションをクリアする

            // 探索対象が宝物の場合は、そのままtrueを返す
            if (((CS_VisionTarget)currentTarget).targetType == CS_VisionTarget.TargetType.Treasure) return true;

            // 探索対象を探索済みに設定
            visionTargetMemories[((CS_VisionTarget)currentTarget)].isExplored = true;
            // 探索度を加算
            roomMemories[currentRoom].explorationLevel += ((CS_VisionTarget)currentTarget).explorationValue;

            // 探索度が閾値を超えた場合は、次の部屋に移動するための処理を追加する
            if (roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
            {
                NextDoorElection();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 現在の部屋の探索度が閾値を超えているかどうかを判定する処理
    /// </summary>
    /// <returns>現在の部屋の探索度が閾値を超えているかどうか</returns>
    public bool IsCurrentRoomExplored()
    {
        // 現在の部屋の記憶がない場合は、探索度が閾値を超えていないと判定してfalseを返す
        if (roomMemories[currentRoom] == null) return false;
        // 現在の部屋の探索度が閾値を超えているかどうかを判定して返す
        return roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold;
    }

    /// <summary>
    /// 現在の探索対象の探索度をリセットする処理
    /// </summary>
    public void ResetCurrentTargetExplorationProgress()
    {
        if (currentTarget == null) return;
        if (!(currentTarget is CS_VisionTarget)) return;
        visionTargetMemories[((CS_VisionTarget)currentTarget)].explorationProgress = 0.0f;
    }

    /// <summary>
    /// 現在いる部屋に関するオブジェクトをRaycastで取得して、currentRoomに設定する処理
    /// </summary>
    public void FindNowRoomNode()
    {
        GameObject currentobject = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(thiefAI.gameObject);
        if (currentobject == null)
        {
            Debug.LogWarning("【泥棒】現在いる部屋に関するオブジェクトの取得に失敗しました");
            return;
        }

        currentRoom = currentobject.transform.GetComponentInChildren<CS_RoomNode>();
        currentRoomObject = currentobject;

        // 現在いる部屋の記憶がない場合は新たに作成
        if (!roomMemories.ContainsKey(currentRoom))
        {
            roomMemories[currentRoom] = new CS_RoomMemory();
            roomMemories[currentRoom].FirstSetting();
            roomMemories[currentRoom].explorationLevel = currentRoom.initialExplorationLevel;

            // 最初から探索度が閾値を超えている場合は、次の部屋に移動するための処理を追加する
            if (roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
            {
                NextDoorElection();
            }
        }
    }

    /// <summary>
    /// 現在いる部屋の接続している方向を取得して、次に探索する部屋に行くための移動ポイントを決定する処理
    /// </summary>
    public void NextDoorElection()
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

        // 次の部屋候補の中に行ったことのない部屋がある場合
        if (hasUnvisitedNextRooms)
        {
            for (int i = 0 ; i < connectDirs.Count ; i++)
            {
                // 入ってきたドアの方向と同じ方向がある場合は、リストから除外
                if (connectDirs[i] == roomMemories[currentRoom].enteredDoorDirection)
                {
                    connectDirs.RemoveAt(i);
                    i--;
                    continue;
                }
                // 行ったことのある方向をリストから除外
                CS_RoomMoveConnection nextRoom;
                roomCreatePoint.TryGetConnection(connectDirs[i], out nextRoom);
                if (roomMemories.ContainsKey(nextRoom.TargetCreatePoint.GetComponentInChildren<CS_RoomNode>()))
                {
                    connectDirs.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }
        // 次の部屋候補の中に行ったことのない部屋がない場合は、今までに行ったことのあるすべての部屋で選ばなかった方向をリストに追加
        else
        {
            connectDirs.Clear();
            // 今までに行ったことのあるすべての部屋で選ばなかった方向をリストに追加
            foreach (var room in roomMemories)
            {
                foreach (var dir in room.Value.unchosenDoors)
                {
                    if (!connectDirs.Contains(dir)) connectDirs.Add(dir);
                }
            }
        }

        // 宝部屋判定
        bool hasTreasureRoom = false;
        foreach (var dir in connectDirs)
        {
            CS_RoomMoveConnection nextRoom;
            roomCreatePoint.TryGetConnection(dir, out nextRoom);

            if (nextRoom.TargetCreatePoint.GetComponentInChildren<CS_RoomNode>().transform.tag == "TreasureRoom")
            {
                hasTreasureRoom = true;
                break;
            }
        }
        // 宝部屋がある場合は、宝部屋以外の方向をリストから除外
        if (hasTreasureRoom)
        {
            for (int i = 0 ; i < connectDirs.Count ; i++)
            {
                CS_RoomMoveConnection nextRoom;
                roomCreatePoint.TryGetConnection(connectDirs[i], out nextRoom);

                if (nextRoom.TargetCreatePoint.GetComponentInChildren<CS_RoomNode>().transform.tag != "TreasureRoom")
                {
                    connectDirs.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }

        // 接続している部屋の方向をランダムに選択
        int randomIndex = Random.Range(0, connectDirs.Count);

        if (hasUnvisitedNextRooms)
        {
            // 選択しなかった方向のドアを記憶
            for (int i = 0 ; i < connectDirs.Count ; i++)
            {
                if (i == randomIndex) continue;

                // 重複確認
                foreach (var unchosenDoor in roomMemories[currentRoom].unchosenDoors)
                {
                    if (connectDirs[i] == unchosenDoor)
                    {
                        // すでに記憶している選択しなかった方向のドアの場合は、重複して記憶しないようにスキップする
                        continue;
                    }
                }

                roomMemories[currentRoom].unchosenDoors.Add(connectDirs[i]);
            }

            // 選択した方向にあるドアの位置を次の移動ポイントに設定
            thiefAI.read_AStarSystem.ConstructionRoute(currentRoom.GetDirectionWallToDoor(connectDirs[randomIndex]), false);
        }
        else
        {
            // 選択した方向のドアを記憶から削除
            bool isRemoved = false; // 選択した方向のドアを記憶から削除したかどうかを判定するフラグ
            CS_RoomNode targetRoomNode = null;
            foreach (var room in roomMemories)
            {
                foreach (var dir in room.Value.unchosenDoors)
                {
                    if (dir == connectDirs[randomIndex])
                    {
                        // どの部屋のドアかを記憶
                        targetRoomNode = room.Key;
                        // 記憶から選択した方向のドアを削除
                        room.Value.unchosenDoors.Remove(dir);
                        // 選択した方向のドアを記憶から削除したフラグを立てる
                        isRemoved = true;
                        break;
                    }
                }
                if (isRemoved) break;
            }

            // 選択したドアの位置を取得
            Transform targetDoorPos = targetRoomNode.GetDirectionWallToDoor(connectDirs[randomIndex]);

            if (targetDoorPos == null)
            {
                Debug.LogError("【泥棒】選択したドアの位置が見つかりませんでした。ThiefAIのNextDoorElectionメソッドで、次に設定する移動ポイントを決定するロジックが正常に動作しない可能性があります。");
                return;
            }

            // ドアの位置を最終目的位置としてルートを構築
            thiefAI.read_AStarSystem.ConstructionRoute(targetDoorPos, false);
        }
    }

    /// <summary>
    /// 次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理
    /// </summary>
    /// <returns>
    /// true:次の部屋候補の中に行ったことのない部屋がある | false:次の部屋候補の中に行ったことのない部屋がない
    /// </returns>
    public bool HasUnvisitedNextRooms()
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
            if (!roomMemories.ContainsKey(nextRoom.TargetCreatePoint.GetComponentInChildren<CS_RoomNode>())) return true;
        }
        return false;
    }

    /// <summary>
    ///罠発動などで「この泥棒が回避する DangerZone」を動的に追加する。
    /// </summary>
    /// <param name="zoneID">追加する DangerZone のID</param>
    public void AddAvoidZoneID(int zoneID)
    {
        if (avoidZoneIDs == null) avoidZoneIDs = new List<int>();
        if (!avoidZoneIDs.Contains(zoneID)) avoidZoneIDs.Add(zoneID);

        // SmartNavAgent がある場合は即時反映
        if (thiefAI.read_MoveSystem.read_SmartNavAgent != null)
        {
            thiefAI.read_MoveSystem.read_SmartNavAgent.SetAvoidZoneIDs(avoidZoneIDs);
        }
    }

    /// <summary>
    /// 罠解除などで「この泥棒が回避する DangerZone」を動的に削除する。
    /// </summary>
    /// <param name="zoneID">削除する DangerZone のID</param>
    public void RemoveAvoidZoneID(int zoneID)
    {
        if (avoidZoneIDs == null) return;
        if (avoidZoneIDs.Contains(zoneID)) avoidZoneIDs.Remove(zoneID);
        // SmartNavAgent がある場合は即時反映
        if (thiefAI.read_MoveSystem.read_SmartNavAgent != null)
        {
            thiefAI.read_MoveSystem.read_SmartNavAgent.SetAvoidZoneIDs(avoidZoneIDs);
        }
    }

    /// <summary>
    /// ワープした後の処理
    /// </summary>
    public void WarpAction(CSE_RoomDoorDirection entryDoorDir)
    {
        FindNowRoomNode(); // ワープした後の現在の部屋を取得して設定する処理
        roomMemories[currentRoom].enteredDoorDirection = entryDoorDir; // ワープしてきたドアの方向を記憶する処理

        ClearTarget();
    }

    /// <summary>
    /// 現在の探索対象が指定した型のオブジェクトかどうかを判定する処理
    /// </summary>
    /// <typeparam name="T">指定する型</typeparam>
    /// <returns> true:現在の探索対象が指定した型のオブジェクトである | false:現在の探索対象が指定した型のオブジェクトでない</returns>
    public bool IsCurrentTargetOfType<T>() where T : CS_ThiefTarget
    {
        if (currentTarget == null) return false;
        return currentTarget is T;
    }

    /// <summary>
    /// 現在の探索対象が探索可能なオブジェクトかどうかを判定する処理
    /// </summary>
    /// <returns> true:現在の探索対象が探索可能なオブジェクトである | false:現在の探索対象が探索可能なオブジェクトでない</returns>
    public bool IsCurrentTargetExplorableToVisionTarget()
    {
        // 現在の探索対象がない場合は、探索可能なオブジェクトでないと判定してfalseを返す
        if (currentTarget == null) return false;

        // 現在の探索対象が視認オブジェクト(VisionTarget)の場合
        if (currentTarget is CS_VisionTarget)
        {
            // 現在の探索対象の記憶がない場合は、探索可能なオブジェクトでないと判定してfalseを返す
            if (!visionTargetMemories.ContainsKey((CS_VisionTarget)currentTarget)) return false;

            // 現在の探索対象が探索済みの場合は、探索可能なオブジェクトでないと判定してfalseを返す
            if (visionTargetMemories[((CS_VisionTarget)currentTarget)].isExplored) return false;

            // 現在の探索対象が他者にも探索されている場合は、探索可能なオブジェクトでないと判定してfalseを返す
            if (visionTargetMemories[((CS_VisionTarget)currentTarget)].searchThief != null &&
                visionTargetMemories[((CS_VisionTarget)currentTarget)].searchThief != thiefAI.gameObject)
            {
                ClearTarget(); // 探索対象をリセットする
                return false;
            }

            // 現在の探索対象が探索可能なオブジェクトであると判定してtrueを返す
            return true;
        }

        return false;
    }
}
