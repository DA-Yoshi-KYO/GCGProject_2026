/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界センサーシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 2026-04-24 | TrapTargetへの対応を追加
 * 2026-05-22 | ファイル名を変更（VisionSensor.cs → CS_VisionSensor.cs）
 *            | クラス名を変更（VisionSensor → CS_VisionSensor）
 * 
 */
using System.Collections.Generic;
using UnityEngine;

// 視界を管理するセンサー
public class CS_VisionSensor : MonoBehaviour
{
    [Header("視界設定")]
    [Tooltip("視界の半径"), Min(0)]
    public float viewDistance;
    [Tooltip("視界の角度"), Range(0, 360)]
    public float viewAngle;

    [Header("レイヤー")]
    [Tooltip("視界に入る対象のレイヤー")]
    public LayerMask targetLayer;
    [Tooltip("障害物のレイヤー")]
    public LayerMask obstacleLayer;

    [SerializeField, Header("ギズモを表示")]
    private bool showGizmos = false;
    [SerializeField, Header("ギズモの色")]
    private Color gizmoColor = Color.yellow;


    /// <summary>
    /// 視界の設定を行うメソッド
    /// </summary>
    ///　<param name="viewDistance">視界の半径</param>
    ///　<param name="viewAngle">視界の角度</param>
    public void Setting(float viewDistance, float viewAngle)
    {
        this.viewDistance = viewDistance;
        this.viewAngle = viewAngle;
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
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, targetLayer);
       

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


    /// <summary>
    /// ギズモを描画するメソッド
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + left * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + right * viewDistance);

        // 視界内のターゲットをギズモで表示
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, targetLayer);
        foreach (var hit in hits)
        {
            CS_VisionTarget target = hit.GetComponent<CS_VisionTarget>();
            if (target == null) continue;

            if (!IsVisible(target)) Gizmos.color = Color.red;
            else Gizmos.color = Color.green;

            Gizmos.DrawLine(transform.position, target.transform.position);
        }

        // 探索対象をギズモで表示
        Gizmos.color = Color.blue;
        CS_ThiefAI thiefAI = this.GetComponent<CS_ThiefAI>();
        if (thiefAI != null && thiefAI.CurrentTarget != null)
        {
            Gizmos.DrawLine(transform.position, thiefAI.CurrentTarget.transform.position);
        }
    }
}
