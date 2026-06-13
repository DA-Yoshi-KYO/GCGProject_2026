/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のギミックに対する行動を管理するクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-11 | 初回作成
 * 
 */
using UnityEngine;
using UnityEngine.AI;

public class CS_ThiefGimmickAction
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    [Tooltip("対象のギミック")]
    private GimmickBase targetGimmick = null;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="thiefAI">ThiefAIスクリプトへの参照</param>
    public CS_ThiefGimmickAction(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    /// <returns>ギミックの影響による移動をしたかどうか</returns>
    public bool UpdateAction()
    {
        if(IronBallUpdate()) return true;

        return false;
    }


    /// <summary>
    /// 落とし穴ギミック用行動の開始
    /// </summary>
    public void PitFallStart(Vector3 pitfallPoint)
    {
        // 状態を気絶に変更
        thiefAI.ChangeStatus(CS_ThiefAI.ThiefState.Stunned);
        // 気絶状態の更新処理を実行しないようにする
        thiefAI.SetStunnedUpdateFlag(false);

        // NavMeshAgentを停止させる
        thiefAI.read_MoveSystem.read_NavMeshAgent.ResetPath();
        thiefAI.read_MoveSystem.read_NavMeshAgent.enabled = false;
        // SmartNavAgentも停止させる
        thiefAI.read_MoveSystem.read_SmartNavAgent.enabled = false;

        // 落とし穴にハマったときの見た目の位置調整
        // 体の8割ほど埋める
        Vector3 pos = pitfallPoint;
        thiefAI.transform.position = new Vector3(pos.x, pos.y + thiefAI.transform.localScale.y * 0.5f, pos.z);
    }

    /// <summary>
    /// 落とし穴ギミック用行動の終了
    /// </summary>
    public void PitFallEnd()
    {
        // 気絶状態の更新処理を実行するようにする
        thiefAI.SetStunnedUpdateFlag(true);
        // NavMeshAgentを再度有効にする
        thiefAI.read_MoveSystem.read_NavMeshAgent.enabled = true;
        // SmartNavAgentも再度有効にする
        thiefAI.read_MoveSystem.read_SmartNavAgent.enabled = true;
    }

    /// <summary>
    /// 大岩ギミック用行動の開始
    /// </summary>
    public void IronBallStart(GimmickBase ironBall)
    {
        targetGimmick = ironBall;
    }

    /// <summary>
    /// 大岩ギミック用行動の更新
    /// </summary>
    private bool IronBallUpdate()
    {
        if (targetGimmick == null) return false;

        var moveSystem = thiefAI.read_MoveSystem;
        if (moveSystem == null) return false;

        var agent = moveSystem.read_NavMeshAgent;
        if (agent == null) return false;

        // 現在の位置と大岩の位置を取得
        Vector3 thiefPos = thiefAI.transform.position;
        Vector3 rockPos = targetGimmick.transform.position;
        thiefPos.y =0f;
        rockPos.y =0f;

        // 大岩から見て最短で離れる方向を計算
        Vector3 awayDir = thiefPos - rockPos;
        if (awayDir.sqrMagnitude <0.001f)
        {
            //もし同位置に近ければ、ギミックの向きベクトル（グリッド）を使う
            Vector2Int dirVec = targetGimmick.GetDirectionVec();
            Vector3 dir3 = new Vector3(dirVec.x,0f, dirVec.y);
            //既存実装と同様にギミックの向きの反対方向へ逃げる
            awayDir = -dir3;
        }
        awayDir.y =0f;
        awayDir.Normalize();

        // 検索する候補距離と角度オフセット
        float[] distances = new float[] {8f,6f,4f,2f };
        float[] angles = new float[] {0f, -45f,45f, -90f,90f,135f, -135f,180f };

        NavMeshHit hit;
        bool found = false;
        Vector3 chosenPoint = thiefPos;

        // 候補を順に試し、NavMesh上の到達可能な点を探す
        foreach (var dist in distances)
        {
            foreach (var ang in angles)
            {
                Vector3 dir = Quaternion.Euler(0f, ang,0f) * awayDir;
                Vector3 candidate = thiefPos + dir * dist;

                // SamplePositionでNavMesh上の近い点を取得
                if (NavMesh.SamplePosition(candidate, out hit,1.0f, NavMesh.AllAreas))
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
            for (int i =0; i <16; i++)
            {
                Vector3 randDir = Random.insideUnitSphere;
                randDir.y =0f;
                randDir.Normalize();
                Vector3 candidate = thiefPos + randDir *4f;
                if (NavMesh.SamplePosition(candidate, out hit,1.0f, NavMesh.AllAreas))
                {
                    // 遠い点を優先
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
        float fleeSpeed = thiefAI.read_MoveSystem.read_WalkSpeed *0.5f;
        agent.speed = fleeSpeed;

        // NavMeshを使って目的地へ移動する
        moveSystem.MoveTo(chosenPoint);

        return true;
    }

    /// <summary>
    /// 大岩ギミック用行動の終了
    /// </summary>
    public void IronBallEnd()
    {
        targetGimmick = null;
        thiefAI.read_MoveSystem.read_NavMeshAgent.ResetPath();
    }
}
