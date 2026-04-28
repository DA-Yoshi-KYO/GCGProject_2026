/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *  　部屋の情報を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

// 部屋の情報を管理するクラス
public class RoomNode : MonoBehaviour
{
    [Tooltip("部屋の移動ポイントリスト")]
    public List<ThiefTarget> movePoints;

    [Tooltip("移動ポイントの回り方")]
    public bool isListDown = true;// trueなら右回り、falseなら左回り

    // 部屋の移動ポイントを回る方向をギズモで表示する
    void OnDrawGizmos()
    {
        if (movePoints.Count == 0)
            return;
        Gizmos.color = Color.green;
        for (int i = 0; i < movePoints.Count; i++)
        {
            if (movePoints[i] == null)
                continue;

            Vector3 currentPoint = movePoints[i].transform.position;
            Vector3 nextPoint = movePoints[(i + 1) % movePoints.Count].transform.position;
            // 線を引く
            Gizmos.DrawLine(currentPoint, nextPoint);
            // 矢印を描く
            Vector3 direction = (nextPoint - currentPoint).normalized;
            Vector3 arrowHead = currentPoint + direction * 0.5f; // 矢印の長さ
            Gizmos.DrawLine(currentPoint, arrowHead);
            Gizmos.DrawLine(arrowHead, arrowHead + Quaternion.Euler(0, 0, isListDown ? -30 : 30) * direction * 0.2f);
            Gizmos.DrawLine(arrowHead, arrowHead + Quaternion.Euler(0, 0, isListDown ? 30 : -30) * direction * 0.2f);
        }
    }
}
