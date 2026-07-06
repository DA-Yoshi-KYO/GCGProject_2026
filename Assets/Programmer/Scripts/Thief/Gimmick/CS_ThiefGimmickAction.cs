/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のギミックに対する行動を管理するクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-11 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CS_ThiefGimmickAction
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    [Tooltip("対象のギミック")]
    private List<GimmickBase> targetGimmick = null;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="thiefAI">ThiefAIスクリプトへの参照</param>
    public CS_ThiefGimmickAction(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
        // 複数ギミックに対応するため、リストを初期化しておく
        targetGimmick = new List<GimmickBase>();
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    /// <returns>ギミックの影響による移動をしたかどうか</returns>
    public bool UpdateAction()
    {
        if (IronBallUpdate()) return true;
        if (EmptyChestUpdate()) return true;

        return false;
    }


    /// <summary>
    /// 落とし穴ギミック用行動の開始
    /// </summary>
    public void PitFallStart(Vector3 pitfallPoint)
    {
        // 状態を気絶に変更
        thiefAI?.ChangeStatus(CS_ThiefAI.ThiefState.Stunned);
        // 気絶状態の更新処理を実行しないようにする
        thiefAI?.SetStunnedUpdateFlag(false);

        // NavMeshAgentを停止させる
        thiefAI?.read_MoveSystem?.read_NavMeshAgent?.ResetPath();
        thiefAI.read_MoveSystem.read_NavMeshAgent.enabled = false;
        // SmartNavAgentも停止させる
        thiefAI.read_MoveSystem.read_SmartNavAgent.enabled = false;

        // 落とし穴にハマったときの見た目の位置調整
        //体の8割ほど埋める
        Vector3 pos = pitfallPoint;
        thiefAI.transform.position = new Vector3(pos.x, pos.y + thiefAI.transform.localScale.y * 0.5f, pos.z);
    }

    /// <summary>
    /// 落とし穴ギミック用行動の終了
    /// </summary>
    public void PitFallEnd()
    {
        // 気絶状態の更新処理を実行するようにする
        thiefAI?.SetStunnedUpdateFlag(true);
        // NavMeshAgentを再度有効にする
        thiefAI.read_MoveSystem.read_NavMeshAgent.enabled = true;
        // SmartNavAgentも再度有効にする
        thiefAI.read_MoveSystem.read_SmartNavAgent.enabled = true;
    }

    /// <summary>
    /// 大岩ギミック用行動の開始
    /// 複数個の大岩に対応するため、受け取ったギミックをリストに追加する
    /// </summary>
    public void IronBallStart(GimmickBase ironBall)
    {
        if (ironBall == null) return;
        // IronBall以外のギミックが来たら無視
        if (ironBall.GetGimmickTag() != Gimmick.IronBall) return;
        if (targetGimmick == null) targetGimmick = new List<GimmickBase>();
        if (!targetGimmick.Contains(ironBall))
        {
            targetGimmick.Add(ironBall);
            GameObject Thief_RockRunAway = GameObject.Find("Thief_RockRunAway_" + thiefAI.transform.name);
            if (Thief_RockRunAway == null)
                thiefAI?.read_ThiefSound?.PlayOneShotSE("Thief_RockRunAway", thiefAI.transform.position, "Thief_RockRunAway_" + thiefAI.transform.name);
        }
    }

    /// <summary>
    /// 大岩ギミック用行動の更新
    /// 複数の大岩に対して、最も近い（脅威となる）大岩を選択して逃げる処理を行う
    /// </summary>
    private bool IronBallUpdate()
    {
        if (targetGimmick == null || targetGimmick.Count == 0) return false;
        foreach(var g in targetGimmick)
        {
            if (g == null) return false;
            if (g.GetGimmickTag() != Gimmick.IronBall) return false;
        }

        var moveSystem = thiefAI.read_MoveSystem;
        if (moveSystem == null) return false;

        var agent = moveSystem.read_NavMeshAgent;
        if (agent == null) return false;

        // 対象リストからnullや破壊済みの要素を除去する
        targetGimmick.RemoveAll(g => g == null);
        if (targetGimmick.Count == 0) return false;

        // 現在の位置を取得
        Vector3 thiefPos = thiefAI.transform.position;
        thiefPos.y = 0f;

        // 最も近い大岩を選択
        GimmickBase nearest = null;
        float minSqr = float.MaxValue;
        foreach (var g in targetGimmick)
        {
            float sq = (g.transform.position - thiefAI.transform.position).sqrMagnitude;
            if (sq < minSqr)
            {
                minSqr = sq;
                nearest = g;
            }
        }
        if (nearest == null) return false;

        Vector3 rockPos = nearest.transform.position;
        rockPos.y = 0f;

        // 大岩から見て最短で離れる方向を計算
        Vector3 awayDir = thiefPos - rockPos;
        if (awayDir.sqrMagnitude < 0.001f)
        {
            //もし同位置に近ければ、ギミックの向きベクトル（グリッド）を使う
            Vector2Int dirVec = nearest.GetDirectionVec();
            Vector3 dir3 = new Vector3(dirVec.x, 0f, dirVec.y);
            //既存実装と同様にギミックの向きの反対方向へ逃げる
            awayDir = -dir3;
        }
        awayDir.y = 0f;
        awayDir.Normalize();

        // 検索する候補距離と角度オフセット
        float[] distances = new float[] { 8f, 6f, 4f, 2f };
        float[] angles = new float[] { 0f, -45f, 45f, -90f, 90f, 135f, -135f, 180f };

        NavMeshHit hit;
        bool found = false;
        Vector3 chosenPoint = thiefPos;

        // 候補を順に試し、NavMesh上の到達可能な点を探す
        foreach (var dist in distances)
        {
            foreach (var ang in angles)
            {
                Vector3 dir = Quaternion.Euler(0f, ang, 0f) * awayDir;
                Vector3 candidate = thiefPos + dir * dist;

                // SamplePositionでNavMesh上の近い点を取得
                if (NavMesh.SamplePosition(candidate, out hit, 1.0f, NavMesh.AllAreas))
                {
                    chosenPoint = hit.position;
                    found = true;
                    break;
                }
            }
            if (found) break;
        }

        if (!found)
        {
            // 最後の手段：NavMesh上の最も遠いサンプルを探す（周囲をランダムにサンプル）
            for (int i = 0 ; i < 16 ; i++)
            {
                Vector3 randDir = Random.insideUnitSphere;
                randDir.y = 0f;
                randDir.Normalize();
                Vector3 candidate = thiefPos + randDir * 4f;
                if (NavMesh.SamplePosition(candidate, out hit, 1.0f, NavMesh.AllAreas))
                {
                    // 遠い点を優先（大岩から遠い点を選ぶ）
                    if ((hit.position - rockPos).sqrMagnitude > (chosenPoint - rockPos).sqrMagnitude)
                    {
                        chosenPoint = hit.position;
                        found = true;
                    }
                }
            }
        }

        if (!found)
        {
            // 移動先が見つからなければ何もしない
            return false;
        }

        // 移動速度を設定（歩行速度の半分）
        float fleeSpeed = thiefAI.read_MoveSystem.read_WalkSpeed * 0.5f;
        agent.speed = fleeSpeed;

        // NavMeshを使って目的地へ移動する
        moveSystem.MoveTo(chosenPoint);

        return true;
    }

    /// <summary>
    /// 大岩ギミック用行動の終了
    /// </summary>
    public void IronBallEnd(RockGimmick rockGimmick)
    {
        if (targetGimmick != null)
        {
            targetGimmick.Remove(rockGimmick);
        }
        thiefAI?.read_MoveSystem?.read_NavMeshAgent?.ResetPath();
        
    }

    /// <summary>
    /// 空の宝箱ギミック用行動の開始
    /// </summary>
    /// <param name="emptyChest">空の宝箱ギミック</param>
    public void EmptyChestStart(GimmickBase emptyChest)
    {
        if (emptyChest == null) return;
        // EmptyChest以外のギミックが来たら無視
        if (emptyChest.GetGimmickTag() != Gimmick.EmptyChest) return;

        if (targetGimmick == null) targetGimmick = new List<GimmickBase>();
        if (!targetGimmick.Contains(emptyChest))
        {
            EmptyChestGimmick emptyChests = null;
            foreach (var g in targetGimmick)
            {
                if (g == null) continue;
                if (g.GetGimmickTag() != Gimmick.EmptyChest) continue;
                emptyChests = (EmptyChestGimmick)g;
            }


            // 距離判定
            float oldDistance = Mathf.Infinity;

            if (emptyChests != null)
            {
                oldDistance = Vector3.Distance(emptyChests.transform.position, thiefAI.transform.position);
            }
            float newDistance = Vector3.Distance(emptyChest.transform.position, thiefAI.transform.position);
            // より近い空の宝箱が来たら、リストの先頭を入れ替える
            if (newDistance < oldDistance)
            {
                if (targetGimmick.Count != 0)
                {
                    targetGimmick.Remove(emptyChests);
                }
                targetGimmick.Add(emptyChest);
            }
        }
    }

    /// <summary>
    /// 空の宝箱ギミック用行動の更新
    /// </summary>
    /// <returns>ギミックの影響による移動をしたかどうか</returns>
    private bool EmptyChestUpdate()
    {
        if (targetGimmick == null || targetGimmick.Count == 0) return false;

        foreach (var g in targetGimmick)
        {
            if (g == null) return false;
            if (g.GetGimmickTag() != Gimmick.EmptyChest) return false;
        }

        return true;
    }

    /// <summary>
    /// 空の宝箱ギミック用行動の終了
    /// </summary>
    /// <param name="emptyChest">空の宝箱ギミック</param>
    public void EmptyChestEnd(GimmickBase emptyChest)
    {
        if (targetGimmick != null)
        {
            targetGimmick.Remove(emptyChest);
        }

        // 泥棒のアニメーション状態をHuntingに変更する
        if (thiefAI.read_Animator != null) thiefAI.read_Animator.SetBool("IsHunting", false);

        // 泥棒の反応状態をChasingCatに変更する
        thiefAI?.read_ThiefReaction?.ChangeReaction(CS_ThiefReaction.ThiefReactionType.Alert, 2.0f);
    }
}
