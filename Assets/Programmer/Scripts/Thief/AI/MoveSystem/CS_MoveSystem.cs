/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の移動に関するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
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
    [SerializeField, Tooltip("泥棒の走り移動速度")]
    private float runSpeed;

    [Tooltip("走り状態になる標的オブジェクトのタイプリスト")]
    private List<CS_VisionTarget.TargetType> runTargetTypes;

    [Tooltip("ナビメッシュエージェント")]
    private NavMeshAgent navMeshAgent;
    public NavMeshAgent read_NavMeshAgent => navMeshAgent;
    [Tooltip("DangerZone を考慮して移動するためのコンポーネント")]
    private CS_SmartNavAgent smartNavAgent;
    public CS_SmartNavAgent read_SmartNavAgent => smartNavAgent;

    [Tooltip("バグ対策用位置保存")]
    private Vector3 debugPos;

    [Tooltip("バグ対策用位置の同じ位置にいるフレーム数カウンター")]
    private int samePosFrameCount;

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
        navMeshAgent.baseOffset = 1.0f;

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
    public void UpdateMoveSpeed(CS_ThiefTarget currentTarget)
    {
        if (currentTarget == null) return;

        if (currentTarget is CS_VisionTarget)
        {
            if (currentTarget != null && runTargetTypes.Contains(((CS_VisionTarget)currentTarget).targetType))
            {
                // 現在の標的が走り状態になるタイプの場合、走り速度に切り替える
                navMeshAgent.speed = runSpeed;
            }
            else
            {
                // それ以外の場合は歩き速度に切り替える
                navMeshAgent.speed = walkSpeed;
            }
        }
        else
        {
            // CS_VisionTarget 以外の標的の場合は歩き速度に切り替える
            navMeshAgent.speed = walkSpeed;
        }
    }

    /// <summary>
    /// 移動要求を統一する。SmartNavAgent がある場合は DangerZone を考慮して移動する。
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (smartNavAgent != null)
        {
            smartNavAgent.MoveTo(destination);
        }
        else if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(destination);
        }
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

        // ThiefAI経由でTransformの位置を更新
        thiefAI.transform.position = targetPos;

        // ワープ後に、ThiefAIのA*システムにワープアクションを通知する
        thiefAI.read_AStarSystem.WarpAction();

        // ワープ後に、ThiefAIの記憶システムにワープアクションを通知する
        thiefAI.read_MemorySystem.WarpAction(entryDoorDir);
    }

    /// <summary>
    /// バグ対策：同じ位置に一定フレーム以上いる場合、MoveToを呼び出す
    /// </summary>
    public void FixStuck()
    {
        if (Vector3.Distance(thiefAI.transform.position, debugPos) < 0.1f)
        {
            samePosFrameCount++;
            if (samePosFrameCount > 120) // 2秒以上同じ位置にいる場合
            {
                // 音に反応しているとき
                if (thiefAI.read_HearingSystem.read_IsReactingToSound)
                {
                    MoveTo(thiefAI.read_HearingSystem.read_SoundReactionPosition);
                }
                // A*システムにルートがあるとき
                else if (thiefAI.read_AStarSystem.HasRoute)
                {
                    MoveTo(thiefAI.read_AStarSystem.read_MoveRoute[0].position);
                }
                // 現在の標的があるとき
                else if (thiefAI.read_MemorySystem.read_CurrentTarget != null)
                {
                    MoveTo(thiefAI.read_MemorySystem.read_CurrentTarget.transform.position);
                }

                samePosFrameCount = 0;
            }
        }
        else
        {
            samePosFrameCount = 0;
            debugPos = thiefAI.transform.position;
        }
    }
}
