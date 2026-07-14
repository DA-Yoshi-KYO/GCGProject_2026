/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *  　部屋の情報を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 2026-05-22 | ファイル名を変更（RoomNode.cs → CS_RoomNode.cs）
 *            | クラス名を変更（RoomNode → CS_RoomNode）
 * 
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 部屋の情報を管理するクラス
/// </summary>
public class CS_RoomNode : MonoBehaviour
{
    [Tooltip("部屋の移動ポイントリスト")]
    public List<CS_ThiefTarget> movePoints;

    [SerializeField, Header("壁の親オブジェクト")]
    private GameObject wallParent;

    [Header("部屋の初期探索度"), Range(0,100)]
    public int initialExplorationLevel = 0;

    [Header("部屋のオブジェクトの親オブジェクト"), SerializeField]
    private GameObject objectParent;
    public GameObject read_ObjectParent => objectParent;

    [SerializeField, Header("ギズモを表示")]
    private bool showGizmos = false;
    [SerializeField, Header("ギズモの開始側の色")]
    private Color gizmoColor = Color.green;
    [SerializeField, Header("ギズモの終了側の色")]
    private Color gizmoColor2 = Color.blue;


    /// <summary>
    /// 移動ポイントをギズモで表示する
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (movePoints.Count == 0)
            return;

        // 開始位置から終了位置までの線を引く
        for (int i = 0 ; i < movePoints.Count ; i++)
        {
            if (movePoints[i] == null)
                continue;

            Color currentColor = Color.Lerp(gizmoColor, gizmoColor2, (float)i / (movePoints.Count - 1));

            Vector3 currentPoint = movePoints[i].transform.position;
            Vector3 nextPoint = movePoints[(i + 1) % movePoints.Count].transform.position;
            // 線を引く
            Gizmos.color = currentColor;
            Gizmos.DrawLine(currentPoint, nextPoint);
        }
    }

    /// <summary>
    /// 方向の情報からドアの位置を取得する
    /// </summary>
    /// <param name="eDirection">指定の方向の情報</param>
    /// <returns>現在の部屋の指定方向にあるドアの座標情報</returns>
    public Transform GetDirectionWallToDoor(CSE_RoomDoorDirection eDirection)
    {
        Transform[] wallChilds = null;

        switch (eDirection)
        {
            case CSE_RoomDoorDirection.Right:
                {
                    Transform rightWall = wallParent.transform.Find("Right");

                    wallChilds = rightWall.GetComponentsInChildren<Transform>();
                }
                break;
            case CSE_RoomDoorDirection.Left:
                {
                    Transform leftWall = wallParent.transform.Find("Left");

                    wallChilds = leftWall.GetComponentsInChildren<Transform>();
                }
                break;
            case CSE_RoomDoorDirection.Front:
                {
                    Transform frontWall = wallParent.transform.Find("Front");

                    wallChilds = frontWall.GetComponentsInChildren<Transform>();
                }
                break;
            case CSE_RoomDoorDirection.Back:
                {
                    Transform backWall = wallParent.transform.Find("Back");

                    wallChilds = backWall.GetComponentsInChildren<Transform>();
                }
                break;
        }

        if (wallChilds.Length == 0) return null;

        foreach (Transform child in wallChilds)
        {
            if (child.tag == "RoomMovePoint")
            {
                return child;
            }
        }

        return null;
    }
}
