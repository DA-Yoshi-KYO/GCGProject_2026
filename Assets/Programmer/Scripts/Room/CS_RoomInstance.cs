using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomInstance.cs
 *  制作者      : 吉本竜
 *  内容        : 生成されたRoom内のRoomMovePointを方向ごとに管理する
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// 生成されたRoomの実体です。
/// 子階層にあるRoomMovePointを方向ごとに管理します。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomInstance : MonoBehaviour
{
    private readonly Dictionary<CSE_RoomDoorDirection, CS_RoomMovePoint> dic_MovePoints =
        new Dictionary<CSE_RoomDoorDirection, CS_RoomMovePoint>();

    /// <summary>
    /// 子階層にあるRoomMovePointを集めて初期化します。
    /// </summary>
    public void InitializeMovePoints()
    {
        dic_MovePoints.Clear();

        CS_RoomMovePoint[] movePoints = GetComponentsInChildren<CS_RoomMovePoint>(true);

        for (int i = 0 ; i < movePoints.Length ; i++)
        {
            CS_RoomMovePoint movePoint = movePoints[i];

            movePoint.ClearTarget();

            if (dic_MovePoints.ContainsKey(movePoint.MoveDirection))
            {
                Debug.LogWarning("[RoomInstance] 同じ方向のRoomMovePointが複数あります : " + movePoint.MoveDirection);
                continue;
            }

            dic_MovePoints.Add(movePoint.MoveDirection, movePoint);
        }
    }

    /// <summary>
    /// 指定方向のRoomMovePointを取得します。
    /// </summary>
    /// <param name="e_Direction">取得したい方向。</param>
    /// <param name="cs_MovePoint">取得したRoomMovePoint。</param>
    /// <returns>取得できた場合はtrue。</returns>
    public bool TryGetMovePoint(
        CSE_RoomDoorDirection e_Direction,
        out CS_RoomMovePoint cs_MovePoint)
    {
        if (dic_MovePoints.Count <= 0)
        {
            InitializeMovePoints();
        }

        return dic_MovePoints.TryGetValue(e_Direction, out cs_MovePoint);
    }

    /// <summary>
    /// 全RoomMovePointの接続を解除します。
    /// </summary>
    public void ClearAllMovePointTargets()
    {
        if (dic_MovePoints.Count <= 0)
        {
            InitializeMovePoints();
        }

        foreach (CS_RoomMovePoint movePoint in dic_MovePoints.Values)
        {
            movePoint.ClearTarget();
        }
    }
}
