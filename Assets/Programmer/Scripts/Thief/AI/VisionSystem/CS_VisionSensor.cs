/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界センサーシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 2026-04-24 | TrapTargetへの対応を追加
 * 2026-05-22 | ファイル名を変更（VisionSensor.cs → CS_VisionSensor.cs）
 *            | クラス名を変更（VisionSensor → CS_VisionSensor）
 * 2026-05-27 | MonoBehaviourから通常クラスに変更
 * 
 */
using System.Collections.Generic;
using UnityEngine;

// 視界を管理するセンサー
public class CS_VisionSensor : MonoBehaviour
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    [Tooltip("視界の半径"), Min(0)]
    public float viewDistance;
    [Tooltip("視界の角度"), Range(0, 360)]
    public float viewAngle;

    [Tooltip("視界に入る対象のレイヤー")]
    public List<LayerMask> targetLayer;
    [Tooltip("障害物のレイヤー")]
    public LayerMask obstacleLayer;

    /// <summary>
    /// 視界の設定を行うメソッド
    /// </summary>
    ///　<param name="viewDistance">視界の半径</param>
    ///　<param name="viewAngle">視界の角度</param>
    public CS_VisionSensor(CS_ThiefAI thiefAI, CO_ThiefStatusData typeData, List<LayerMask> targetLayer, LayerMask obstacleLayer)
    {
        // ThiefAIスクリプトへの参照を保存
        this.thiefAI = thiefAI;

        // 視界の半径と角度を保存
        this.viewDistance = typeData.viewDistance;
        this.viewAngle = typeData.viewAngle;

        // 対象のレイヤーと障害物のレイヤーを保存
        this.targetLayer = targetLayer;
        this.obstacleLayer = obstacleLayer;
    }

    /// <summary>
    /// 視界の設定を行うメソッド（セッター）
    /// </summary>
    /// <param name="thiefAI"></param>
    /// <param name="typeData"></param>
    /// <param name="targetLayer"></param>
    /// <param name="obstacleLayer"></param>
    public void Setting(CS_ThiefAI thiefAI, CO_ThiefStatusData typeData, List<LayerMask> targetLayer, LayerMask obstacleLayer)
    {
        // ThiefAIスクリプトへの参照を保存
        this.thiefAI = thiefAI;
        // 視界の半径と角度を保存
        this.viewDistance = typeData.viewDistance;
        this.viewAngle = typeData.viewAngle;
        // 対象のレイヤーと障害物のレイヤーを保存
        this.targetLayer = targetLayer;
        this.obstacleLayer = obstacleLayer;
    }

    /// <summary>
    /// 視界内のターゲットをスキャンしてリストで返すメソッド
    /// </summary>
    /// <returns>視界内のターゲットのリスト</returns>
    public List<CS_ThiefTarget> Scan()
    {
        // 視界内のターゲットを格納するリスト
        List<CS_ThiefTarget> visibleTargets = new List<CS_ThiefTarget>();

        // 視界内のコライダーを取得
        List<Collider> hits = new List<Collider>();
        foreach (var layer in targetLayer)
        {
            Collider[] scanObjects = Physics.OverlapSphere(thiefAI.transform.position, viewDistance, layer);

            // 取得したコライダーをリストに追加
            hits.AddRange(scanObjects);
        }

        // 取得したコライダーをループして、視界内にあるターゲットを判定
        foreach (var hit in hits)
        {
            // VisionTargetコンポーネントを取得
            CS_ThiefTarget target = hit.GetComponent<CS_VisionTarget>();
            if (target == null)
            {
                target = hit.GetComponent<CS_TrapTarget>();
                if (target == null)
                {
                    target = hit.GetComponent<CS_PlayerTarget>();
                    if (target == null)
                    {
                        continue; // VisionTargetもTrapTargetもPlayerTargetもない場合はスキップ
                    }
                }
            }

            // ターゲットが視界内にあるかどうかを判定
            if (IsVisible(target))
            {
                visibleTargets.Add(target);
            }
        }

        // 視界内のターゲットのリストを返す
        return visibleTargets;
    }

    /// <summary>
    /// ターゲットが視界内にあるかどうかを判定するメソッド
    /// </summary>
    /// <param name="target">判定するターゲット</param>
    /// <returns>ターゲットが視界内にある場合はtrue、そうでない場合はfalse</returns>
    private bool IsVisible(CS_ThiefTarget target)
    {
        Transform transform = thiefAI.transform;

        // ターゲットへの方向ベクトルを計算
        Vector3 dir = (target.transform.position - transform.position).normalized;

        // 角度チェック
        if (Vector3.Angle(transform.forward, dir) > viewAngle / 2)
            return false;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        // 障害物チェック
        if (Physics.Raycast(transform.position, dir, distance, obstacleLayer))
            return false;

        return true;
    }

    private void OnDrawGizmos()
    {
        // 視界の半径を表すワイヤースフィアを描画
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        // 視界の角度を表す線を描画
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
