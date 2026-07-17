/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の移動に関するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 * 2026-07-06 | スタック解消用のデバック移動を削除
 * 2026-07-12 | スタック解消用のデバック移動を再構築
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 泥棒の移動に関するシステムを管理するクラス。
/// </summary>
public class CS_MoveSystem
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    [SerializeField, Tooltip("泥棒の歩き移動速度")]
    private float walkSpeed;
    public float read_WalkSpeed => walkSpeed;

    [SerializeField, Tooltip("泥棒の走り移動速度")]
    private float runSpeed;
        public float read_RunSpeed => runSpeed;

    [Tooltip("泥棒が走り状態になる標的オブジェクトのタイプを指定するための列挙型")]
    public enum  RunTargetType
    {
        Player,         // プレイヤー
        Treasure,       // 宝物
        SearchObject,    // 捜索対象オブジェクト
    }

    [Tooltip("走り状態になる標的オブジェクトのタイプリスト")]
    private List<RunTargetType> runTargetTypes;

    [Tooltip("ナビメッシュエージェント")]
    private NavMeshAgent navMeshAgent;
    public NavMeshAgent read_NavMeshAgent => navMeshAgent;
    [Tooltip("DangerZone を考慮して移動するためのコンポーネント")]
    private CS_SmartNavAgent smartNavAgent;
    public CS_SmartNavAgent read_SmartNavAgent => smartNavAgent;

    [Tooltip("バグ対策用位置保存")]
    private Vector3 debugPos;

    [Tooltip("過去に指定した目的地")]
    private Vector3 lastDestination;

    [Tooltip("バグ対策用位置の同じ位置にいるフレーム数カウンター")]
    private int samePosFrameCount = 0;

    [Tooltip("気絶後の退場方向")]
    private Vector3 exitDirectionAfterStun;

    /// <summary>
    /// コンストラクタ。必要なコンポーネントやパラメータを受け取って初期化する。
    /// </summary>
    /// <param name="thiefAI">ThiefAIスクリプトへの参照</param>
    /// <param name="navMeshAgent">NavMeshAgent コンポーネント</param>
    /// <param name="smartNavAgent">DangerZone を考慮して移動するためのコンポーネント</param>
    /// <param name="walkSpeed">泥棒の歩き移動速度</param>
    /// <param name="runSpeed">泥棒の走り移動速度</param>
    public CS_MoveSystem(CS_ThiefAI thiefAI,NavMeshAgent navMeshAgent, CS_SmartNavAgent smartNavAgent, CO_ThiefStatusData typedata, float playerSpeed)
    {
        // ThiefAIスクリプトへの参照を保存
        this.thiefAI = thiefAI;

        // コンポーネントの取得
        this.navMeshAgent = navMeshAgent;
        this.smartNavAgent = smartNavAgent;

        // ナビメッシュエージェントの初期設定
        navMeshAgent.baseOffset = 0.65f;

        // 移動速度の設定
        this.walkSpeed = playerSpeed * typedata.walkSpeedMultiplier;
        this.runSpeed = playerSpeed * typedata.runSpeedMultiplier;

        // 走り状態になる標的オブジェクトのタイプリストを保存
        runTargetTypes = typedata.runTargetTypes;

        // ナビメッシュエージェントの速度を歩き速度に設定
        navMeshAgent.speed = walkSpeed;
    }

    /// <summary>
    /// 現在の標的に応じて移動状態を更新する処理
    /// </summary>
    /// <param name="currentTarget">現在の標的オブジェクト</param>
    public void UpdateMoveSpeed()
    {
        CS_ThiefTarget currentTarget = thiefAI.read_MemorySystem.read_CurrentTarget;

        if (currentTarget == null)
        {
            // 標的がいない場合は歩き速度に切り替える
            navMeshAgent.speed = walkSpeed;
            thiefAI?.read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Walk);
            if (thiefAI.read_IsPlayerHit)
            {
                navMeshAgent.speed *= 0.5f;
            }
            return;
        }


        foreach (var targetType in runTargetTypes)
        {
            switch (targetType)
            {
                case RunTargetType.Player:
                    if (currentTarget is CS_PlayerTarget)
                    {
                        // 現在の標的がプレイヤーの場合は走り速度に切り替える
                        navMeshAgent.speed = runSpeed;
                        thiefAI?.read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Run);
                        if(thiefAI.read_IsPlayerHit)
                        {
                            navMeshAgent.speed *= 0.5f;
                        }
                        return;
                    }
                    break;
                case RunTargetType.Treasure:
                    if (currentTarget is CS_VisionTarget vt && vt.targetType == CS_VisionTarget.TargetType.Treasure)
                    {
                        // 現在の標的が宝物の場合は走り速度に切り替える
                        navMeshAgent.speed = runSpeed;
                        thiefAI?.read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Run);
                        if (thiefAI.read_IsPlayerHit)
                        {
                            navMeshAgent.speed *= 0.5f;
                        }
                        return;
                    }
                    if (currentTarget is CS_TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest)
                    {
                        // 現在の標的が宝物ギミックの場合は走り速度に切り替える
                        navMeshAgent.speed = runSpeed;
                        thiefAI?.read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Run);
                        if (thiefAI.read_IsPlayerHit)
                        {
                            navMeshAgent.speed *= 0.5f;
                        }
                        return;
                    }
                    break;
                case RunTargetType.SearchObject:
                    if (currentTarget is CS_VisionTarget vt2 && vt2.targetType == CS_VisionTarget.TargetType.Shelf)
                    {
                        // 現在の標的が捜索対象オブジェクトの場合は走り速度に切り替える
                        navMeshAgent.speed = runSpeed;
                        thiefAI?.read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Run);
                        if (thiefAI.read_IsPlayerHit)
                        {
                            navMeshAgent.speed *= 0.5f;
                        }
                        return;
                    }
                    break;
            }
        }

        //標的の場合は歩き速度に切り替える
        navMeshAgent.speed = walkSpeed;
        thiefAI?.read_AnimatorSystem?.SetAnimationState(CS_ThiefAnimation.ThiefAnimationState.Walk);
        if (thiefAI.read_IsPlayerHit)
        {
            navMeshAgent.speed *= 0.5f;
        }

    }

    /// <summary>
    /// ナビメッシュエージェントを停止させる処理
    /// </summary>
    public void Stop()
    {
        // NavMeshAgentが存在しない場合は停止処理を行わない
        if (navMeshAgent == null) return;
        // NavMeshAgentが無効化されている場合は停止処理を行わない
        if (!navMeshAgent.enabled) return;
        // NavMesh上にない場合は停止処理を行わない
        if (!navMeshAgent.isOnNavMesh) return;

        navMeshAgent.isStopped = true;
    }

    /// <summary>
    /// 移動要求を統一する。SmartNavAgent がある場合は DangerZone を考慮して移動する。
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        // NavMeshAgentが存在しない、またはNavMesh上にない場合は移動要求を無視する
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh) return;

        // 漁り状態のときは移動要求を無視する
        if (thiefAI.read_AnimatorSystem.read_Animator.GetBool("IsHunting")) return;
        navMeshAgent.isStopped = false; // SmartNavAgentを使用する場合はNavMeshAgentを停止状態から解除する

        if (lastDestination == destination) return; // 目的地が前回と同じ場合は移動要求を無視する

        if (smartNavAgent != null)
        {
            smartNavAgent.MoveTo(destination);
            lastDestination = destination;
        }
        else if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(destination);
            lastDestination = destination;
        }
    }

    /// <summary>
    /// 指定した位置にワープする処理
    /// </summary>
    /// <param name="targetPos">指定位置</param>
    /// <param name="entryDoorDir">入ってきたドアの方向</param>
    public void WarpAction(Transform targetTransform, CSE_RoomDoorDirection entryDoorDir)
    {
        // 現在の経路をリセットして、ワープ後に新しい経路を計算させる
        navMeshAgent.ResetPath();
        // NavMeshAgentのWarpメソッドを使用して、指定した位置にワープする
        navMeshAgent.Warp(targetTransform.position);

        // ThiefAI経由でTransformの位置を更新

        Vector3 lookDir = targetTransform.position - targetTransform.parent.position;
        lookDir.y = 0.0f;

        Quaternion spawnRotation = targetTransform.rotation;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            spawnRotation = Quaternion.LookRotation(lookDir);
        }

        thiefAI.transform.SetPositionAndRotation(targetTransform.position, spawnRotation);


        // ワープ後に、ThiefAIのA*システムにワープアクションを通知する
        thiefAI?.read_AStarSystem?.WarpAction();

        // ワープ後に、ThiefAIの記憶システムにワープアクションを通知する
        thiefAI?.read_MemorySystem?.WarpAction(entryDoorDir);
    }

    /// <summary>
    /// 気絶状態後の退場移動処理
    /// </summary>
    public void StunMove()
    {
        // 退場方向が設定されていない場合
        if (exitDirectionAfterStun == Vector3.zero)
        {
            // 入ってきたドアの方向を基に退場方向を設定する
            exitDirectionAfterStun = Vector3.Normalize(thiefAI.read_MemorySystem.read_FirstEntryPoint.position - thiefAI.transform.position);
            // 移動方向に向きを変える
            thiefAI.transform.rotation = Quaternion.LookRotation(exitDirectionAfterStun);

        }

        // 気絶後の退場方向に向かって移動する
        Vector3 exitPosition = thiefAI.transform.position + exitDirectionAfterStun.normalized; // 退場する距離を適宜調整
        exitPosition.y = thiefAI.transform.position.y; // 高さは変えない
        thiefAI.transform.position = Vector3.MoveTowards(thiefAI.transform.position, exitPosition, walkSpeed * 0.5f * Time.deltaTime);
    }

    /// <summary>
    /// デバッグ用の移動処理
    /// </summary>
    public void DebugMove()
    {
        if (thiefAI.read_CurrentState == CS_ThiefAI.ThiefState.Stunned) return; // 気絶状態のときは移動要求を無視する
        if (Time.timeScale == 0.0f || Time.deltaTime == 0.0f) return; // ゲームが一時停止中の場合は移動要求を無視する

        // 同じ位置にいるフレーム数をカウントする
        if (debugPos == thiefAI.transform.position)
        {
            // 同じ位置にいるフレーム数が300フレームを超えた場合の処理
            samePosFrameCount++;
            if (samePosFrameCount > 60)
            {
                // A*システムの経路が存在する場合
                if (thiefAI.read_AStarSystem.HasRoute)
                {
                    // 現在の部屋とA*システムのターゲット部屋が異なる場合は経路をクリアする
                    if (thiefAI.read_MemorySystem.read_CurrentRoom != thiefAI.read_AStarSystem.GetCurrentTargetRoomNode())
                    {
                        // 経路をクリアして、再度経路計算を行う
                        thiefAI.read_AStarSystem.ClearRoute();
                    }
                    else
                    {
                        // 経路が存在するが、同じ位置に長時間いる場合はA*システムの更新フラグをリセットして再計算を促す
                        thiefAI.read_AStarSystem.ResetUpdatedFlag();
                    }
                }
                // 現在のターゲットが存在する場合
                else if (thiefAI.read_MemorySystem.read_CurrentTarget != null)
                {
                    // そのターゲットに向かって移動する
                    MoveTo(thiefAI.read_MemorySystem.read_CurrentTarget.transform.position);
                }
                // 現在のターゲットが存在しない場合は
                else
                {
                    // ターゲットをクリアする
                    thiefAI.read_MemorySystem.ClearTarget();
                    // 現在いる部屋の移動ポイントに向かって移動する
                    thiefAI.read_MemorySystem.DecideTargetMovePoint();
                }
            }
        }
        // 現在の位置が前回の位置と異なる場合はカウンターをリセットする
        else
        {
            debugPos = thiefAI.transform.position;
            samePosFrameCount = 0; // カウンターをリセット
        }
    }

    /// <summary>
    /// バグ対策用の情報をリセットする処理
    /// </summary>
    public void ResetInfo()
    {
        lastDestination = Vector3.zero;
        samePosFrameCount = 0;
    }
}
