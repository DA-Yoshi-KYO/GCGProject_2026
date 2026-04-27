using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomMovePointGroup.cs
 *  制作者      : 吉本竜
 *  内容        : Room内にあるRoomMovePointを方向ごとに取得する管理クラス
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// 1つのRoom内にあるRoomMovePointを方向ごとに管理するクラスです。
/// </summary>
public class CS_RoomMovePointGroup : MonoBehaviour
{
    /// <summary>
    /// 指定方向のRoomMovePointを取得します。
    /// </summary>
    /// <param name="e_Direction">取得したい方向。</param>
    /// <returns>指定方向のRoomMovePoint。見つからない場合はnull。</returns>
    public CS_RoomMovePoint GetMovePoint(CSE_RoomDoorDirection e_Direction)
    {
        CS_RoomMovePoint[] roomMovePoints = GetComponentsInChildren<CS_RoomMovePoint>(true);

        for (int i = 0 ; i < roomMovePoints.Length ; i++)
        {
            if (roomMovePoints[i].MoveDirection == e_Direction)
            {
                return roomMovePoints[i];
            }
        }

        return null;
    }
}
