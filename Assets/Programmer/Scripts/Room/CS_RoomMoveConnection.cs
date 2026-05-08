using System;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomMoveConnection.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePoint間の接続情報と出入口用途を管理する
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *                2026/05/06 出入口用途と敵出入口データを追加(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePoint同士の接続情報と、出入口の用途を管理するクラスです。
/// </summary>
[Serializable]
public class CS_RoomMoveConnection
{
    [Header("出入口の用途")]
    [SerializeField]
    private CSE_RoomDoorUsageType e_DoorUsageType = CSE_RoomDoorUsageType.RoomMove;

    [Header("移動先のRoomCreatePoint")]
    [SerializeField]
    private CS_RoomCreatePoint cs_TargetCreatePoint;

    [Header("移動先Roomのどの方向から出るか")]
    [SerializeField]
    private CSE_RoomDoorDirection e_TargetOutDirection = CSE_RoomDoorDirection.Left;

    [Header("敵出入口用データ")]
    [SerializeField]
    private CSS_RoomEnemyEntryData cs_RoomEnemyEntryDataSO;

    /// <summary>
    /// 出入口の用途を取得します。
    /// </summary>
    public CSE_RoomDoorUsageType DoorUsageType => e_DoorUsageType;

    /// <summary>
    /// この出入口がルーム移動用かどうかを取得します。
    /// </summary>
    public bool IsRoomMoveDoor => e_DoorUsageType == CSE_RoomDoorUsageType.RoomMove;

    /// <summary>
    /// この出入口が敵出入口用かどうかを取得します。
    /// </summary>
    public bool IsEnemyEntryDoor => e_DoorUsageType == CSE_RoomDoorUsageType.EnemyEntry;

    /// <summary>
    /// 移動先RoomCreatePointが設定されているか取得します。
    /// </summary>
    public bool HasTarget => IsRoomMoveDoor && cs_TargetCreatePoint != null;

    /// <summary>
    /// 敵出入口用データが設定されているか取得します。
    /// </summary>
    public bool HasEnemyEntryData => IsEnemyEntryDoor && cs_RoomEnemyEntryDataSO != null;

    /// <summary>
    /// 移動先RoomCreatePointを取得します。
    /// </summary>
    public CS_RoomCreatePoint TargetCreatePoint => cs_TargetCreatePoint;

    /// <summary>
    /// 移動先Roomの出現方向を取得します。
    /// </summary>
    public CSE_RoomDoorDirection TargetOutDirection => e_TargetOutDirection;

    /// <summary>
    /// 敵出入口用データを取得します。
    /// </summary>
    public CSS_RoomEnemyEntryData RoomEnemyEntryDataSO => cs_RoomEnemyEntryDataSO;

    /// <summary>
    /// この敵出入口から出現できる敵の最大数を取得します。
    /// </summary>
    /// <returns>最大出現数。</returns>
    public int GetMaxEnemySpawnCount()
    {
        if (!HasEnemyEntryData)
        {
            return 0;
        }

        return cs_RoomEnemyEntryDataSO.GetMaxEnemySpawnCount();
    }

    /// <summary>
    /// 敵侵入数を取得します。
    /// 互換用として、最大出現数を返します。
    /// </summary>
    /// <returns>最大出現数。</returns>
    public int GetEnemyEntryCount()
    {
        return GetMaxEnemySpawnCount();
    }
}
