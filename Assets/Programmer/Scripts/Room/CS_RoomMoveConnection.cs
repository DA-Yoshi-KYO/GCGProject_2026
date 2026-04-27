using System;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomMoveConnection.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePoint間の接続情報
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePoint同士の接続情報です。
/// </summary>
[Serializable]
public class CS_RoomMoveConnection
{
    [Header("移動先のRoomCreatePoint")]
    [SerializeField]
    private CS_RoomCreatePoint cs_TargetCreatePoint;

    [Header("移動先Roomのどの方向から出るか")]
    [SerializeField]
    private CSE_RoomDoorDirection e_TargetOutDirection = CSE_RoomDoorDirection.Left;

    /// <summary>
    /// 移動先RoomCreatePointが設定されているか取得します。
    /// </summary>
    public bool HasTarget => cs_TargetCreatePoint != null;

    /// <summary>
    /// 移動先RoomCreatePointを取得します。
    /// </summary>
    public CS_RoomCreatePoint TargetCreatePoint => cs_TargetCreatePoint;

    /// <summary>
    /// 移動先Roomの出現方向を取得します。
    /// </summary>
    public CSE_RoomDoorDirection TargetOutDirection => e_TargetOutDirection;
}
