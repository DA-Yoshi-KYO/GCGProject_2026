using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomCreatePoint.cs
 *  制作者      : 吉本竜
 *  内容        : ランダム生成Roomの配置位置と接続情報を管理する
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// ルーム生成位置と、各方向の移動接続情報を持つポイントです。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomCreatePoint : MonoBehaviour
{
    [Header("右出口の接続先")]
    [SerializeField]
    private CS_RoomMoveConnection cs_RightConnection = new CS_RoomMoveConnection();

    [Header("左出口の接続先")]
    [SerializeField]
    private CS_RoomMoveConnection cs_LeftConnection = new CS_RoomMoveConnection();

    [Header("前出口の接続先")]
    [SerializeField]
    private CS_RoomMoveConnection cs_FrontConnection = new CS_RoomMoveConnection();

    [Header("後ろ出口の接続先")]
    [SerializeField]
    private CS_RoomMoveConnection cs_BackConnection = new CS_RoomMoveConnection();

    /// <summary>
    /// 指定方向の接続情報を取得します。
    /// 接続先が未設定の場合はfalseを返します。
    /// </summary>
    /// <param name="e_FromDirection">このRoomから入る方向。</param>
    /// <param name="cs_Connection">取得した接続情報。</param>
    /// <returns>接続先がある場合はtrue。</returns>
    public bool TryGetConnection(
        CSE_RoomDoorDirection e_FromDirection,
        out CS_RoomMoveConnection cs_Connection)
    {
        cs_Connection = GetConnection(e_FromDirection);

        if (cs_Connection == null)
        {
            return false;
        }

        return cs_Connection.HasTarget;
    }

    /// <summary>
    /// 指定方向の接続情報を取得します。
    /// </summary>
    /// <param name="e_FromDirection">取得したい方向。</param>
    /// <returns>接続情報。</returns>
    private CS_RoomMoveConnection GetConnection(CSE_RoomDoorDirection e_FromDirection)
    {
        switch (e_FromDirection)
        {
            case CSE_RoomDoorDirection.Right:
                return cs_RightConnection;

            case CSE_RoomDoorDirection.Left:
                return cs_LeftConnection;

            case CSE_RoomDoorDirection.Front:
                return cs_FrontConnection;

            case CSE_RoomDoorDirection.Back:
                return cs_BackConnection;

            default:
                return null;
        }
    }

    /// <summary>
    /// 接続先が設定されている方向を全て取得します。
    /// </summary>
    /// <returns></returns>
    public List<CSE_RoomDoorDirection> GetConnectDirections()
    {
        List<CSE_RoomDoorDirection> list = new List<CSE_RoomDoorDirection>();
        if (cs_RightConnection.HasTarget)
        {
            list.Add(CSE_RoomDoorDirection.Right);
        }
        if (cs_LeftConnection.HasTarget)
        {
            list.Add(CSE_RoomDoorDirection.Left);
        }
        if (cs_FrontConnection.HasTarget)
        {
            list.Add(CSE_RoomDoorDirection.Front);
        }
        if (cs_BackConnection.HasTarget)
        {
            list.Add(CSE_RoomDoorDirection.Back);
        }
        return list;
    }
}
