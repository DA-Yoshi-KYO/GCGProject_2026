/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の記憶に関するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 * 2026-06-04 | 視認したオブジェクトを記憶する処理を作り変え
 *            | プレイヤーを無視するフラグと、プレイヤーを追跡する残り時間の処理を追加
 */
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
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

    [Tooltip("現在いる部屋のCreateRoomPoint")]
    public Transform read_CurrentRoomPoint => currentRoom != null ? currentRoom.gameObject.transform.parent : null;

    private GameObject currentRoomObject;

    [Tooltip("部屋に関する記憶")]
    private Dictionary<CS_RoomNode, CS_RoomMemory> roomMemories;

    [Tooltip("デバック用：部屋に関する記憶を外部から参照するためのプロパティ")]
    public IReadOnlyDictionary<CS_RoomNode, CS_RoomMemory> read_RoomMemorys => roomMemories;

    [Tooltip("視認オブジェクトの記憶")]
    private Dictionary<CS_VisionTarget, CS_VisionTargetMemory> visionTargetMemories;

    [Tooltip("視認したギミックオブジェクト")]
    private List<CS_TrapTarget> gimmickObjectsMemories;
    public IReadOnlyList<CS_TrapTarget> read_GimmickObjectsMemories => gimmickObjectsMemories;

    [Tooltip("プレイヤーを無視するフラグ")]
    private bool ignorePlayer = false;
    [Tooltip("プレイヤーを追跡する残り時間")]
    private float remainingIgnorePlayerTime;
    [Tooltip("プレイヤーを追跡する残り時間の初期値")]
    private float initialRemainingIgnorePlayerTime;
    [Tooltip("プレイヤーを発見する猶予時間")]
    private float findPlayerGraceTime;
    [Tooltip("プレイヤーを発見する猶予時間の初期値")]
    private float initialFindPlayerGraceTime;

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

    [Tooltip("最初入ってきたドアの方向")]
    private CSE_RoomDoorDirection firstEntryDirection;
    public CSE_RoomDoorDirection read_FirstEntryDirection => firstEntryDirection;

    [Tooltip("この泥棒が回避する DangerZone の zoneID 一覧")]
    private List<int> avoidZoneIDs = new List<int>();

    /// <summary>
    /// 記憶システムの初期設定を行う処理
    /// </summary>
    /// <param name="thiefAI">ThiefAIのメインスクリプト</param>
    /// <param name="entryRoom">最初の部屋の情報</param>
    /// <param name="entryPoint">最初の部屋の入ってきたドアの位置</param>
    public CS_MemorySystem(CS_ThiefAI thiefAI, CS_RoomNode entryRoom, CSE_RoomDoorDirection doorDir, Transform entryPoint, CO_ThiefStatusData typedata, CO_ThiefCommonStatusData data)
    {
        // ThiefAIのメインスクリプトを取得
        this.thiefAI = thiefAI;
        // 最初の部屋の情報を保存
        firstRoom = entryRoom;
        firstEntryPoint = entryPoint;
        firstEntryDirection = doorDir;

        // プレイヤーを追跡する残り時間の初期値を設定
        initialRemainingIgnorePlayerTime = typedata.pursuitTime;
        remainingIgnorePlayerTime = typedata.pursuitTime;

        // プレイヤーを発見する猶予時間の初期値を設定
        initialFindPlayerGraceTime = data.findPlayerGraceTime;
        findPlayerGraceTime = 0.0f;

        // 次の部屋探索に切り替える探索度の閾値を設定
        nextRoomSearchThreshold = typedata.nextRoomSearchThreshold;

        // 探索対象の探索にかかる時間を設定
        searchTime = typedata.searchTime;

        // 記憶領域の作成
        roomMemories = new Dictionary<CS_RoomNode, CS_RoomMemory>();
        visionTargetMemories = new Dictionary<CS_VisionTarget, CS_VisionTargetMemory>();
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

        // currentRoom の記憶が存在しない場合は空を返す
        if (currentRoom == null) return unexploredObjects;

        // currentRoom の記憶が存在しない、もしくは currentRoom の記憶が破棄されている場合は空を返す
        CS_RoomMemory roomMemory;
        if (!roomMemories.TryGetValue(currentRoom, out roomMemory) || roomMemory == null || roomMemory.recognizedObjects == null)
        {
            return unexploredObjects;
        }

        // 視認しているオブジェクトの中から未探索のもののみをリストに追加する
        foreach (var entry in roomMemory.recognizedObjects)
        {
            var vt = entry as CS_VisionTarget;
            if (vt == null) continue;

            CS_VisionTargetMemory vtMemory;
            if (!visionTargetMemories.TryGetValue(vt, out vtMemory) || vtMemory == null)
            {
                // 記憶が作られていない（or破棄済み）場合はここで作成しておく
                vtMemory = new CS_VisionTargetMemory();
                visionTargetMemories[vt] = vtMemory;
            }

            // 未探索オブジェクト
            if (!vtMemory.isExplored)
            {
                // 探索している人がいない場合や、探索している人が自分である場合は、未探索のオブジェクトとしてリストに追加する
                if (vtMemory.searchThief == null || vtMemory.searchThief == thiefAI.gameObject)
                {
                    unexploredObjects.Add(entry);
                }
            }
        }

        return unexploredObjects;
    }

    /// <summary>
    /// 部屋のオブジェクトを視認して記憶に保存する処理
    /// </summary>
    public void RecognizeObjects()
    {
        // 視界内オブジェクトを取得
        List<CS_ThiefTarget> visionTargets = thiefAI?.read_VisionSensor?.Scan();

        // 現在の部屋の記憶がない場合は新たに作成
        CS_RoomMemory roomMemory;
        // 現在の部屋の記憶がない、もしくは現在の部屋の記憶が破棄されている場合は、新たに作成して保存する
        if (!roomMemories.TryGetValue(currentRoom, out roomMemory) || roomMemory == null)
        {
            roomMemory = new CS_RoomMemory();
            roomMemory.FirstSetting();
            roomMemories[currentRoom] = roomMemory;
        }

        bool isTreasureObject = false;  // 視界内に宝物があるかどうかを判定するフラグ
        bool isPlayerObject = false;    // 視界内にプレイヤーがいるかどうかを判定するフラグ

        // 視界内オブジェクトを記憶に保存
        foreach (var entry in visionTargets)
        {
            // プレイヤーオブジェクトの場合
            if (entry is CS_PlayerTarget)
            {
                // 無敵状態のプレイヤーは探索対象にしない
                if (entry.transform.GetComponent<CS_PlayerMove>().IsInvincible) continue;

                // プレイヤー視認フラグを立てる
                isPlayerObject = true;
                continue;
            }

            // 気絶した泥棒のオブジェクトの場合
            if (entry is CS_StunThiefTarget)
            {
                // そのオブジェクトの危険ゾーンを回避ゾーンとして記憶する
                AddAvoidZoneID(((CS_StunThiefTarget)entry).read_DangerZone.ZoneID);
                // 移動システムに回避ゾーンを設定する
                thiefAI?.read_MoveSystem?.read_SmartNavAgent?.SetAvoidZoneIDs(avoidZoneIDs.ToArray());
                // 移動システムを再構築する
                thiefAI?.read_MoveSystem?.read_SmartNavAgent?.RefreshPath();

                continue;
            }

            // 視認オブジェクト(VisionTarget)でない場合は、スキップする
            if (!(entry is CS_VisionTarget)) continue;

            // 視界内に宝物がある場合は、isTreasureObjectをtrueにする
            if (((CS_VisionTarget)entry).targetType == CS_VisionTarget.TargetType.Treasure) isTreasureObject = true;

            // 現在の部屋の認識しているオブジェクトのリストがない場合は、新たに作成する
            if (roomMemory.recognizedObjects == null) roomMemory.recognizedObjects = new List<CS_ThiefTarget>();

            //すでに記憶しているオブジェクトの場合
            if (roomMemory.recognizedObjects.Contains(entry))
            {
                var vt = entry as CS_VisionTarget;
                if (vt != null)
                {
                    // すでに記憶しているオブジェクトが視認オブジェクトの場合は、そのオブジェクトの記憶を取得する
                    CS_VisionTargetMemory vtMemory;
                    if (!visionTargetMemories.TryGetValue(vt, out vtMemory) || vtMemory == null)
                    {
                        vtMemory = new CS_VisionTargetMemory();
                        visionTargetMemories[vt] = vtMemory;
                    }

                    // 探索している人を更新する
                    vtMemory.searchThief = vt.searchThief;

                    if (vt.targetType == CS_VisionTarget.TargetType.Treasure)
                    {
                        // すでに記憶しているオブジェクトが宝物の場合は、宝物があるフラグを立てる
                        isTreasureObject = true;
                    }
                }


                // スキップする
                continue;
            }

            // 記憶に保存する
            roomMemory.recognizedObjects.Add(entry);

            // 視認オブジェクトの記憶も作成しておく
            var newVt = entry as CS_VisionTarget;
            if (newVt != null)
            {
                CS_VisionTargetMemory vtMemory;
                // 記憶が作られていない（or破棄済み）場合はここで作成しておく
                if (!visionTargetMemories.TryGetValue(newVt, out vtMemory) || vtMemory == null)
                {
                    vtMemory = new CS_VisionTargetMemory();
                    visionTargetMemories[newVt] = vtMemory;
                }
            }
        }

        // 視認しているギミックを記憶に保存
        if (gimmickObjectsMemories == null) gimmickObjectsMemories = new List<CS_TrapTarget>();
        gimmickObjectsMemories.Clear();
        foreach (var entry in visionTargets)
        {
            // 視認オブジェクトの中からギミックオブジェクトのみをリストに追加する
            if (!(entry is CS_TrapTarget trapTarget)) continue;

            // ギミックオブジェクトを記憶に保存する
            if (gimmickObjectsMemories == null) gimmickObjectsMemories = new List<CS_TrapTarget>();
            gimmickObjectsMemories.Add(trapTarget);
        }

        foreach (CS_TrapTarget trap in gimmickObjectsMemories)
        {
            switch (trap.gimmickScript.gimmick)
            {
                // 空の宝箱罠の場合は、宝物があるフラグを立てる
                case Gimmick.EmptyChest:
                    {
                        thiefAI?.read_ThiefGimmickAction?.EmptyChestStart(trap.gimmickScript);
                        isTreasureObject = true;
                    }
                    break;
            }
        }

        // 記憶の中に宝物オブジェクトがある場合は、視認オブジェクトリストの中に追加する
        foreach (var entry in roomMemory.recognizedObjects)
        {
            if (entry is CS_VisionTarget visionTarget && visionTarget.targetType == CS_VisionTarget.TargetType.Treasure)
            {
                if (!visionTargets.Contains(entry)) 
                    visionTargets.Add(entry);

                isTreasureObject = true;
            }
        }


        // 探索対象を決める処理
        DecideTarget(visionTargets, isTreasureObject, isPlayerObject);
    }

    /// <summary>
    /// 探索対象を決める処理
    /// </summary>
    private void DecideTarget(List<CS_ThiefTarget> visionTargets, bool isTreasureObject, bool isPlayerObject)
    {
        // 現在の部屋の探索度が閾値を超えている場合
        if (roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
        {
            if (!thiefAI.read_AStarSystem.HasRoute)
                if (!isTreasureObject && !isPlayerObject)
                    NextDoorElection();
        }

        // 現在の探索対象との距離
        float currrentTargetDistance = Mathf.Infinity;

        // 現在の探索対象が視認オブジェクトで、かつその探索している人が自分でない場合は、探索対象をリセットする
        if (currentTarget is CS_VisionTarget vt && !CanTarget(vt))
        {
            ClearTarget();
        }

        // 現在の探索対象がある場合は、現在の探索対象との距離を計算する
        if (currentTarget != null) currrentTargetDistance = Vector3.Distance(thiefAI.transform.position, currentTarget.transform.position);

        //ーーーーーーーーーーーーーーーーーーーーーーーー
        //--- 宝箱を視認している場合
        //ーーーーーーーーーーーーーーーーーーーーーーーー
        if (isTreasureObject)
        {
            if (currentTarget is CS_ThiefTarget)
            {
                currrentTargetDistance = Mathf.Infinity;
            }

            // 視認したオブジェクトを走査
            foreach (var entry in visionTargets)
            {
                // 現在の探索対象の場合はスキップする
                if (entry == currentTarget) continue;

                // 宝物オブジェクトの場合
                //  or 空の宝箱罠の場合
                if ((entry is CS_VisionTarget visionTarget && visionTarget.targetType == CS_VisionTarget.TargetType.Treasure)
                    || (entry is CS_TrapTarget trapTarget && trapTarget.gimmickScript.gimmick == Gimmick.EmptyChest))
                {
                    // VisionTargetで、他人が探索中なら対象にしない
                    var entryVt = entry as CS_VisionTarget;
                    if (entryVt != null && !CanTarget(entryVt))
                    {
                        continue;
                    }

                    // 現在の探索対象との距離よりも近い場合は、そのオブジェクトを探索対象に設定する
                    float distance = Vector3.Distance(thiefAI.transform.position, entry.transform.position);
                    if (distance < currrentTargetDistance)
                    {
                        currrentTargetDistance = distance;
                        SetTarget(entry);
                        continue;
                    }
                }
            }

            // 宝物を探索対象に設定した後は、A*システムのルートをクリアする
            if (thiefAI.read_AStarSystem.HasRoute) thiefAI?.read_AStarSystem?.ClearRoute();

            return;
        }

        //ーーーーーーーーーーーーーーーーーーーーーーーー
        //--- プレイヤーを視認している場合
        //ーーーーーーーーーーーーーーーーーーーーーーーー
        if (isPlayerObject)
        {
            CS_PlayerTarget playerTarget = null;
            // 視認したオブジェクトを走査
            foreach (var entry in visionTargets)
            {
                // プレイヤーオブジェクトの場合
                if (entry is CS_PlayerTarget)
                {
                    playerTarget = (CS_PlayerTarget)entry;
                    break;
                }
            }

            // 現在の耐久値が1の場合
            if (thiefAI?.read_Durability == 1)
            {
                // 探索対象に設定
                if (CS_ThiefDebugFlags.ChasePlayer)
                {
                    SetTarget(playerTarget);
                }
            }
            else
            {
                // プレイヤーを無視するフラグが立っていない場合
                if (!ignorePlayer)
                {
                    // プレイヤーを追跡する残り時間が0以下の場合は、探索対象に設定しない
                    if (thiefAI?.read_RemainingInvincibleTime <= 0) return;

                    if (CS_ThiefDebugFlags.ChasePlayer)
                    {
                        SetTarget(playerTarget);
                    }
                }
            }

            // プレイヤーを探索対象に設定した後は、A*システムのルートをクリアする
            if (thiefAI.read_AStarSystem.HasRoute) thiefAI?.read_AStarSystem?.ClearRoute();

            return;
        }
        // プレイヤーを視認していない場合
        else
        {
            // 追跡のリアクションをクリアする
            thiefAI?.read_ThiefReaction?.ClearReactionByType(CS_ThiefReaction.ThiefReactionType.ChasingCat);

            // 現在の探索対象がプレイヤーの場合は、探索対象をリセットする
            if (currentTarget is CS_PlayerTarget) ClearTarget();

        }

        // 以下は構築しているルート移動を優先させる
        if (thiefAI.read_AStarSystem.HasRoute) return;

        //ーーーーーーーーーーーーーーーーーーーーーーーー
        //--- 音に反応している場合
        //ーーーーーーーーーーーーーーーーーーーーーーーー
        if (thiefAI.read_HearingSystem.read_IsReactingToSound)
        {
            // 現在の探索対象をリセット
            ClearTarget();

            // 音に反応しているオブジェクトを探索対象に設定する
            thiefAI?.read_MoveSystem?.MoveTo(thiefAI.read_HearingSystem.read_SoundReactionPosition);

            return;
        }

        //ーーーーーーーーーーーーーーーーーーーーーーーー
        //--- 未探索の記憶オブジェクトがある場合
        //ーーーーーーーーーーーーーーーーーーーーーーーー
        List<CS_ThiefTarget> unexploredObjects = GetUnExploredObjectsList();
        if (unexploredObjects.Count > 0)
        {
            if (currentTarget is CS_ThiefTarget)
            {
                currrentTargetDistance = Mathf.Infinity;
            }

            CS_ThiefTarget target = null;
            // 未探索の記憶オブジェクトの中で最も近いものを探索対象に設定する
            foreach (var entry in unexploredObjects)
            {
                // 念のため：未探索リスト側で漏れてもここで弾く
                var entryVt = entry as CS_VisionTarget;
                if (entryVt != null && !CanTarget(entryVt))
                {
                    continue;
                }

                float distance = Vector3.Distance(thiefAI.transform.position, entry.transform.position);
                if (distance < currrentTargetDistance)
                {
                    currrentTargetDistance = distance;
                    target = entry;
                    continue;
                }
            }
            // 最も近い未探索の記憶オブジェクトを探索対象に設定する
            if (target != null) SetTarget(target);

            return;
        }

        //ーーーーーーーーーーーーーーーーーーーーーーーー
        //--- 部屋の移動ポイントを探索対象にする処理
        //ーーーーーーーーーーーーーーーーーーーーーーーー
        DecideTargetMovePoint();
    }

    /// <summary>
    /// 現在の探索対象に対して、探索が完了しているかどうかを判定する処理
    /// </summary>
    public void EvaluateCurrentTarget()
    {
        // 帰宅ルートが未構築なら構築する
        if (thiefAI.read_AStarSystem.HasRoute)
        {
            // ルートを更新
            thiefAI?.read_AStarSystem?.UpdateRoute(thiefAI.read_ExploredDistanceThreshold);
            return;
        }

        float distanceToTarget = Mathf.Infinity;

        // 現在の探索対象がある場合
        if (currentTarget != null)
        {
            distanceToTarget = Vector3.Distance(thiefAI.transform.position, currentTarget.transform.position);

            // 視認オブジェクトの場合
            if (currentTarget is CS_VisionTarget)
            {
                // 探索対象との距離が、探索済みとする距離の閾値以下になっている場合
                if (distanceToTarget <= ((CS_VisionTarget)currentTarget).exploredDistanceThreshold)
                {
                    thiefAI?.read_MoveSystem?.Stop();

                    // 探索対象の探索している人を自分に設定する
                    visionTargetMemories[((CS_VisionTarget)currentTarget)].searchThief = thiefAI.gameObject;
                    ((CS_VisionTarget)currentTarget).searchThief = thiefAI.gameObject;

                    // 探索しているオブジェクトの方を向くように回転させる
                    thiefAI?.transform.LookAt(new Vector3(currentTarget.transform.position.x, thiefAI.transform.position.y, currentTarget.transform.position.z));

                    // 泥棒のアニメーション状態をHuntingに変更する
                    thiefAI?.read_Animator?.SetBool("IsHunting", true);

                    // 泥棒のリアクションを探索に変更する
                    thiefAI?.read_ThiefReaction?.ChangeReaction(CS_ThiefReaction.ThiefReactionType.Searching);

                    // 探索対象の探索にかかる時間を経過させる
                    if (ProgressTargetSearchTime())
                    {
                        // 探索したものが宝物以外の場合は、探索対象をリセットする
                        if (((CS_VisionTarget)currentTarget).targetType != CS_VisionTarget.TargetType.Treasure) ClearTarget();
                    }
                }
                else
                {
                    // 探索対象との距離が、探索済みとする距離の閾値よりも大きい場合は、探索度をリセットする
                    ResetCurrentTargetExplorationProgress();
                    // 探索対象の方へ移動する
                    thiefAI?.read_MoveSystem?.MoveTo(currentTarget.transform.position);
                }
            }
            // プレイヤーの場合
            else if (currentTarget is CS_PlayerTarget)
            {
                // 常にプレイヤーの方を向くように回転させる
                thiefAI?.transform.LookAt(new Vector3(currentTarget.transform.position.x, thiefAI.transform.position.y, currentTarget.transform.position.z));

                // 現在の耐久値が1より大きい場合
                if (thiefAI.read_Durability > 1)
                {
                    remainingIgnorePlayerTime -= Time.deltaTime;
                    if (remainingIgnorePlayerTime <= 0)
                    {
                        ignorePlayer = true;
                        remainingIgnorePlayerTime = 0;
                        ClearTarget();
                        return;
                    }
                }

                // 泥棒のリアクションを追跡に変更する
                thiefAI?.read_ThiefReaction?.ChangeReaction(CS_ThiefReaction.ThiefReactionType.ChasingCat);

                // 探索対象との距離が、探索済みとする距離の閾値以下になっている場合は、探索対象をリセットする
                if (distanceToTarget <= thiefAI.read_ExploredDistanceThreshold)
                {
                    // フラグでプレイヤーを捕まえる挙動を制御
                    if (CS_ThiefDebugFlags.CatchPlayer)
                    {
                        thiefAI.CatchCat();

                        ignorePlayer = true;

                        // CS_PlayerMoveに通知
                        ((CS_PlayerTarget)currentTarget).transform.GetComponent<CS_PlayerMove>().CaughtByThief(thiefAI.read_RemainingHoldCatTime, thiefAI.transform);

                        // 泥棒のアニメーション状態をHuntingに変更する
                        thiefAI?.read_Animator?.SetBool("IsHunting", true);


                        return;
                    }
                    else
                    {
                        // フラグがオフの場合は捕まえず、探索対象を解除しておく
                        ClearTarget();
                        return;
                    }
                }
            }
            // 部屋の移動ポイントの場合
            else if (currentTarget is CS_ThiefTarget)
            {
                if (Vector3.Distance(thiefAI.transform.position, currentTarget.transform.position) > thiefAI.read_ExploredDistanceThresholdMovePoint)
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
                        thiefAI?.read_MoveSystem?.MoveTo(currentTarget.transform.position);
                        break;
                    }
                }
            }
        }
        // 現在の探索対象がない場合
        else
        {
            // 音に反応している場合
            if (thiefAI.read_HearingSystem.read_IsReactingToSound)
            {
                // 泥棒のリアクションを警戒に変更する
                thiefAI?.read_ThiefReaction?.ChangeReaction(CS_ThiefReaction.ThiefReactionType.Alert);

                // 音に反応している位置との距離が、探索済みとする距離の閾値以下になっている場合は、その位置を探索対象に設定する
                if (!thiefAI.read_HearingSystem.IsAtSoundReactionPosition(thiefAI.read_ExploredDistanceThreshold)) return;
            }
            else thiefAI?.read_ThiefReaction?.ClearReactionByType(CS_ThiefReaction.ThiefReactionType.Alert);
        }
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
    /// 部屋の移動ルートに沿って移動するための探索対象を決める処理
    /// </summary>
    private void DecideTargetMovePoint()
    {
        // 部屋の探索度が閾値を超えている場合
        if (IsCurrentRoomExplored())
        {
            NextDoorElection();
            return;
        }

        // 探索対象との距離
        float distanceToTarget = Mathf.Infinity;
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
                if (distance < distanceToTarget)
                {
                    distanceToTarget = distance;
                    currentTarget = target;
                }
                else continue;
            }
            thiefAI?.read_MoveSystem?.MoveTo(currentTarget.transform.position);
        }
    }

    /// <summary>
    /// 探索対象を強制的に変更する処理
    /// (対象：プレイヤーが攻撃してきたときや、ミミックの罠にかかったときなど)
    /// </summary>
    /// <param name="target">新しく設定する探索対象</param>
    public void SetTarget(CS_ThiefTarget target)
    {
        // 強制変更でも、他人が探索中の VisionTargetには乗り換えない
        var vt = target as CS_VisionTarget;
        if (vt != null && !CanTarget(vt))
        {
            return;
        }
        if (target == null)
        {
            ClearTarget();
            return;
        }

        currentTarget = target;

        // 探索対象に向かって移動
        thiefAI?.read_MoveSystem?.MoveTo(currentTarget.transform.position);
        thiefAI.read_AStarSystem.ResetUpdatedFlag();
    }

    /// <summary>
    /// プレイヤーを発見する猶予時間を経過させる処理
    /// </summary>
    public void UpdateFindPlayerGraceTime()
    {
        if (findPlayerGraceTime > 0)
        {
            findPlayerGraceTime -= Time.deltaTime;
        }
        else findPlayerGraceTime = 0;
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
        // 泥棒のアニメーション状態をHuntingに変更する
        thiefAI?.read_Animator?.SetBool("IsHunting", false);
    }

    /// <summary>
    /// プレイヤーを無視するフラグをリセットする処理
    /// </summary>
    public void ResetIgnorePlayer()
    {
        ignorePlayer = false;
        remainingIgnorePlayerTime = initialRemainingIgnorePlayerTime;
        findPlayerGraceTime = initialFindPlayerGraceTime;
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
        if (!(currentTarget is CS_VisionTarget)) return false;

        // 探索対象の探索にかかる時間を経過させる
        //　((VisionTarget)currentTarget).explorationProgress　: 対象の探索度(MAX : 100.0f)
        // searchTime : 探索対象の探索にかかる時間
        visionTargetMemories[((CS_VisionTarget)currentTarget)].explorationProgress += (100.0f / searchTime[((int)((CS_VisionTarget)currentTarget).targetType)]) * Time.deltaTime;

        GameObject Thief_Serach = GameObject.Find("Thief_Serach_" + thiefAI.transform.name);
        if (Thief_Serach == null)
            thiefAI?.read_ThiefSound?.PlayOneShotSE("Thief_Serach", thiefAI.transform.position, "Thief_Serach_" + thiefAI.transform.name);

        // 探索度が100%以上になった場合は、探索が終了していると判定してtrueを返す
        if (visionTargetMemories[((CS_VisionTarget)currentTarget)].explorationProgress >= 100.0f)
        {
            thiefAI?.read_ThiefReaction?.ClearReaction(); // 探索が終了したときのリアクションをクリアする

            // 探索対象が宝物の場合は、そのままtrueを返す
            if (((CS_VisionTarget)currentTarget).targetType == CS_VisionTarget.TargetType.Treasure)
            {
                // 泥棒のアニメーション状態をHuntingに変更する
                thiefAI?.read_Animator?.SetBool("IsHunting", false);
                // 泥棒のアニメーション状態をHuntingに変更する
                thiefAI?.read_Animator?.SetTrigger("FoundTrigger");
                // 泥棒の状態をFoundに変更する
                thiefAI?.ChangeStatus(CS_ThiefAI.ThiefState.Found);

                return true;
            }

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
    private bool IsCurrentRoomExplored()
    {
        // 現在の部屋の記憶がない場合は、探索度が閾値を超えていないと判定してfalseを返す
        if (roomMemories[currentRoom] == null) return false;
        // 現在の部屋の探索度が閾値を超えているかどうかを判定して返す
        return roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold;
    }

    /// <summary>
    /// 現在の探索対象の探索度をリセットする処理
    /// </summary>
    private void ResetCurrentTargetExplorationProgress()
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
        GameObject currentobject = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(thiefAI?.gameObject);
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
        //=== 今までいったことのある部屋の中から宝部屋があるかどうか

        // 記憶している部屋の中から宝部屋を探す
        foreach (var room in roomMemories)
        {
            if (room.Key.CompareTag("TreasureRoom"))
            {
                //---- 宝物部屋に宝物が残っているかどうか -----------------------------------------------------------------------------------
                Transform treasureRoomObjParent = room.Key.read_ObjectParent.transform;
                if (treasureRoomObjParent.childCount == 0) break;

                bool isTreasureObject = false;
                for (int n = 0 ; n < treasureRoomObjParent.childCount ; n++)
                {
                    if (treasureRoomObjParent.GetChild(n).GetComponent<CS_VisionTarget>().targetType == CS_VisionTarget.TargetType.Treasure)
                    {
                        isTreasureObject = true;
                        break;
                    }
                }
                if (!isTreasureObject) break;
                //---------------------------------------------------------------------------------------------------------------------------

                for (int i = 0 ; i < 4 ; i++)
                {
                    CS_RoomMoveConnection nextConnectionRoom = room.Key.GetComponentInParent<CS_RoomCreatePoint>().GetConnection((CSE_RoomDoorDirection)i);

                    if (nextConnectionRoom.TargetCreatePoint == null) continue;

                    CS_RoomNode nextConnectionRoomNode = nextConnectionRoom.TargetCreatePoint.GetComponentInChildren<CS_RoomNode>();

                    // 次の部屋の記憶がある場合
                    if (roomMemories.ContainsKey(nextConnectionRoomNode))
                    {
                        // 選択した方向にあるドアの位置を次の移動ポイントに設定
                        // ※ 接続先のドア方向は必ずしも自室と同じ(i)とは限らないため、
                        //   接続情報が持つ TargetOutDirection(接続先側の実際のドア方向)を使用する
                        thiefAI?.read_AStarSystem?.ConstructionRoute(nextConnectionRoomNode.GetDirectionWallToDoor(nextConnectionRoom.TargetOutDirection), false);
                        return;
                    }
                }
            }
        }

        // 現在いる部屋のオブジェクトが見つからない場合
        if (currentRoomObject == null)
        {
            // 部屋を再取得する
            FindNowRoomNode();
            return;
        }

        // 現在いる部屋のRoomCreatePointが見つからない場合
        CS_RoomCreatePoint roomCreatePoint = currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>();
        if (roomCreatePoint == null) return;

        // 現在いる部屋の接続している方向を取得
        List<CSE_RoomDoorDirection> connectDirs = roomCreatePoint.GetConnectDirections();
        if (connectDirs.Count == 0) return;

        //=== 接続している部屋の中に行ったことのない部屋があるかどうかを判定

        // 接続している部屋の中に宝部屋があるかどうか
        for (int i = 0 ; i < connectDirs.Count ; i++)
        {
            // 接続している部屋のRoomNodeを取得
            CS_RoomNode nextConnectionRoomNode = roomCreatePoint.GetConnection(connectDirs[i]).TargetCreatePoint.GetComponentInChildren<CS_RoomNode>();

            // タグがTreasureRoomの場合は、次の移動ポイントに設定して処理を終了する
            if (nextConnectionRoomNode.CompareTag("TreasureRoom"))
            {
                // 選択した方向にあるドアの位置を次の移動ポイントに設定
                thiefAI?.read_AStarSystem?.ConstructionRoute(roomCreatePoint.GetRoomDoorPosition(connectDirs[i]), false);
                return;
            }
        }

        // 接続リストから、自身の入ってきたドアの方向を除外する
        for (int i = 0 ; i < connectDirs.Count ; i++)
        {
            if (connectDirs[i] == roomMemories[currentRoom].enteredDoorDirection)
            {
                connectDirs.RemoveAt(i);
                break;
            }
        }

        // 次の部屋候補の中に行ったことのない部屋の方向リストを作成
        // ※ HasUnvisitedNextRooms() は入ってきたドア方向を除外していない全方向で判定するため、
        //   ここで別途フィルタした connectDirs から判定すると結果が食い違い、
        //   unvisitedDirs が空なのに選出しようとして OutOfRangeError になることがある。
        //   そのため、実際に使うリストを先に作ってその件数で判定する。
        List<CSE_RoomDoorDirection> unvisitedDirs = new List<CSE_RoomDoorDirection>();
        for (int i = 0 ; i < connectDirs.Count ; i++)
        {
            // 接続している部屋のRoomNodeを取得
            CS_RoomNode nextConnectionRoomNode = roomCreatePoint.GetConnection(connectDirs[i]).TargetCreatePoint.GetComponentInChildren<CS_RoomNode>();
            // 次の部屋の記憶がない場合は、行ったことのない部屋としてリストに追加
            if (!roomMemories.ContainsKey(nextConnectionRoomNode))
            {
                unvisitedDirs.Add(connectDirs[i]);
            }
        }

        // 次の部屋候補の中に行ったことのない部屋がある場合
        if (unvisitedDirs.Count > 0)
        {
            // 次の部屋候補の中に行ったことのない部屋の方向リストからランダムに選出
            int randomIndex = Random.Range(0, unvisitedDirs.Count);
            CSE_RoomDoorDirection selectedDir = unvisitedDirs[randomIndex];

            // 選択した方向にあるドアの位置を次の移動ポイントに設定
            thiefAI?.read_AStarSystem?.ConstructionRoute(roomCreatePoint.GetRoomDoorPosition(selectedDir), false);
            return;
        }
        // 次の部屋候補の中に行ったことのない部屋がない場合
        else
        {
            // 隣接している部屋で探索度が閾値を超えていない部屋があるかどうか
            List<CSE_RoomDoorDirection> unSearchedDirs = new List<CSE_RoomDoorDirection>();
            for (int i = 0 ; i < connectDirs.Count ; i++)
            {
                // 接続している部屋のRoomNodeを取得
                CS_RoomNode nextConnectionRoomNode = roomCreatePoint.GetConnection(connectDirs[i]).TargetCreatePoint.GetComponentInChildren<CS_RoomNode>();
                // 次の部屋の記憶がある場合は、探索度が閾値を超えていないかどうかを判定
                if (roomMemories.ContainsKey(nextConnectionRoomNode))
                {
                    if (roomMemories[nextConnectionRoomNode].explorationLevel < nextRoomSearchThreshold)
                    {
                        unSearchedDirs.Add(connectDirs[i]);
                    }
                }
            }

            // 隣接している部屋で探索度が閾値を超えていない部屋がある場合
            if (unSearchedDirs.Count > 0)
            {
                // 隣接している部屋で探索度が閾値を超えていない部屋の方向リストからランダムに選出
                int randomIndex = Random.Range(0, unSearchedDirs.Count);
                CSE_RoomDoorDirection selectedDir = unSearchedDirs[randomIndex];
                // 選択した方向にあるドアの位置を次の移動ポイントに設定
                thiefAI?.read_AStarSystem?.ConstructionRoute(roomCreatePoint.GetRoomDoorPosition(selectedDir), false);
                return;
            }
            // 隣接している部屋で探索度が閾値を超えていない部屋がない場合
            else
            {
                // 記憶の中に閾値を超えていない部屋があるかどうか
                List<CS_RoomNode> unSearchedRooms = new List<CS_RoomNode>();
                foreach (var room in roomMemories)
                {
                    if (room.Value.explorationLevel < nextRoomSearchThreshold)
                    {
                        unSearchedRooms.Add(room.Key);
                    }
                }

                // 記憶の中に閾値を超えていない部屋がある場合
                if (unSearchedRooms.Count > 0)
                {
                    // 記憶の中に閾値を超えていない部屋の中からランダムに選出
                    int randomIndex = Random.Range(0, unSearchedRooms.Count);
                    CS_RoomNode selectedRoom = unSearchedRooms[randomIndex];

                    // 選択した部屋のRoomCreatePointを取得
                    CS_RoomCreatePoint selectedRoomCreatePoint = selectedRoom.GetComponentInParent<CS_RoomCreatePoint>();

                    // 選択した部屋の接続している方向を取得
                    List<CSE_RoomDoorDirection> connectDirsOfSelectedRoom = selectedRoomCreatePoint.GetConnectDirections();

                    // 選択した部屋の接続している方向の中から、行ったことのある部屋の方向
                    for (int i = 0 ; i < connectDirsOfSelectedRoom.Count ; i++)
                    {
                        // 接続している部屋のRoomNodeを取得
                        CS_RoomNode nextConnectionRoomNode = selectedRoomCreatePoint.GetConnection(connectDirsOfSelectedRoom[i]).TargetCreatePoint.GetComponentInChildren<CS_RoomNode>();

                        // 次の部屋の記憶がある場合
                        if (roomMemories.ContainsKey(nextConnectionRoomNode))
                        {
                            // 選択した方向にあるドアの位置を次の移動ポイントに設定
                            thiefAI?.read_AStarSystem?.ConstructionRoute(selectedRoomCreatePoint.GetRoomDoorPosition(connectDirsOfSelectedRoom[i]), false);
                            return;
                        }
                    }
                }
            }
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
        // 現在いる部屋のオブジェクトが見つからない場合
        if (currentRoomObject == null)
        {
            // 部屋を再取得する
            FindNowRoomNode();
        }

        if (currentRoomObject == null) return false;

        if (currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>() == null) return false;

        if (currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>().GetConnectDirections().Count > 0)
        {
            // 接続している部屋の中に行ったことのない部屋があるかどうかを判定
            foreach (var dir in currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>().GetConnectDirections())
            {
                // 接続している部屋のRoomNodeを取得
                CS_RoomNode nextConnectionRoomNode = currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>().GetConnection(dir).TargetCreatePoint.GetComponentInChildren<CS_RoomNode>();
                // 次の部屋の記憶がない場合は、行ったことのない部屋があると判定してtrueを返す
                if (!roomMemories.ContainsKey(nextConnectionRoomNode))
                {
                    return true;
                }
            }
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
        thiefAI?.read_MoveSystem?.read_SmartNavAgent?.SetAvoidZoneIDs(avoidZoneIDs);
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
        thiefAI?.read_MoveSystem?.read_SmartNavAgent?.SetAvoidZoneIDs(avoidZoneIDs); 
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
    /// 指定の視認ターゲットを、自分が探索対象にしてよいか判定する
    /// </summary>
    private bool CanTarget(CS_VisionTarget vt)
    {
        if (vt == null) return false;

        //まずは記憶側の searchThief を優先（無ければコンポーネント側）
        CS_VisionTargetMemory mem;
        GameObject owner = null;
        if (visionTargetMemories != null && visionTargetMemories.TryGetValue(vt, out mem) && mem != null)
        {
            owner = mem.searchThief;
        }
        if (owner == null)
        {
            owner = vt.searchThief;
        }

        // owner が設定されていて、自分以外なら対象にしない
        return owner == null || owner == thiefAI?.gameObject;
    }
}
