/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のギミックに対する行動を管理するクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-11 | 初回作成
 * 
 */
using UnityEngine;

public class CS_ThiefGimmickAction : MonoBehaviour
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="thiefAI">ThiefAIスクリプトへの参照</param>
    public CS_ThiefGimmickAction(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
    }



    /// <summary>
    /// 落とし穴ギミックにハマった
    /// </summary>
    [ContextMenu("落とし穴ギミックにハマったときの処理")]
    public void PitFallStart()
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
        thiefAI.SetStunnedUpdateFlag(true);
        // NavMeshAgentを再度有効にする
        thiefAI.read_MoveSystem.read_NavMeshAgent.enabled = true;
        // SmartNavAgentも再度有効にする
        thiefAI.read_MoveSystem.read_SmartNavAgent.enabled = true;
    }
}
