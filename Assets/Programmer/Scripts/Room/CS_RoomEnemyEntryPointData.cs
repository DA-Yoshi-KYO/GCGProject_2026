using System;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomEnemyEntryPointData.cs
 *  制作者      : 吉本竜
 *  内容        : 敵出入口として使用するRoomMovePoint情報を保持する
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *                2026/05/06 ScriptableObject参照を廃止し、最大出現数を直接保持する形へ変更(ヨシモト)
 *==================================================*/

/// <summary>
/// 敵出入口として使用するRoomCreatePoint、方向、RoomMovePoint、最大出現数をまとめた情報です。
/// </summary>
[Serializable]
public class CS_RoomEnemyEntryPointData
{
    [Header("対象RoomCreatePoint")]
    [SerializeField]
    private CS_RoomCreatePoint cs_RoomCreatePoint;

    [Header("敵が入ってくる方向")]
    [SerializeField]
    private CSE_RoomDoorDirection e_EnemyEntryDirection;

    [Header("敵生成位置として使うRoomMovePoint")]
    [SerializeField]
    private CS_RoomMovePoint cs_RoomMovePoint;

    [Header("最大敵出現数")]
    [SerializeField, Min(0)]
    private int int_MaxEnemySpawnCount;

    /// <summary>
    /// 対象RoomCreatePointを取得します。
    /// </summary>
    public CS_RoomCreatePoint RoomCreatePoint => cs_RoomCreatePoint;

    /// <summary>
    /// 敵が入ってくる方向を取得します。
    /// </summary>
    public CSE_RoomDoorDirection EnemyEntryDirection => e_EnemyEntryDirection;

    /// <summary>
    /// 敵生成位置として使うRoomMovePointを取得します。
    /// </summary>
    public CS_RoomMovePoint RoomMovePoint => cs_RoomMovePoint;

    /// <summary>
    /// 敵生成位置として使うRoomMovePointのGameObjectを取得します。
    /// </summary>
    public GameObject RoomMovePointObject
    {
        get
        {
            if (cs_RoomMovePoint == null)
            {
                return null;
            }

            return cs_RoomMovePoint.gameObject;
        }
    }

    /// <summary>
    /// 敵生成位置のTransformを取得します。
    /// </summary>
    public Transform SpawnTransform
    {
        get
        {
            if (cs_RoomMovePoint == null)
            {
                return null;
            }

            return cs_RoomMovePoint.transform;
        }
    }

    /// <summary>
    /// 敵生成位置を取得します。
    /// </summary>
    public Vector3 SpawnPosition
    {
        get
        {
            if (cs_RoomMovePoint == null)
            {
                return Vector3.zero;
            }

            return cs_RoomMovePoint.transform.position;
        }
    }

    /// <summary>
    /// この生成位置から出現できる敵の最大数を取得します。
    /// </summary>
    public int MaxEnemySpawnCount => Mathf.Max(0, int_MaxEnemySpawnCount);

    /// <summary>
    /// 敵出入口情報を生成します。
    /// </summary>
    /// <param name="cs_RoomCreatePoint">対象RoomCreatePoint。</param>
    /// <param name="e_EnemyEntryDirection">敵が入ってくる方向。</param>
    /// <param name="cs_RoomMovePoint">敵生成位置として使うRoomMovePoint。</param>
    /// <param name="int_MaxEnemySpawnCount">最大敵出現数。</param>
    public CS_RoomEnemyEntryPointData(
        CS_RoomCreatePoint cs_RoomCreatePoint,
        CSE_RoomDoorDirection e_EnemyEntryDirection,
        CS_RoomMovePoint cs_RoomMovePoint,
        int int_MaxEnemySpawnCount)
    {
        this.cs_RoomCreatePoint = cs_RoomCreatePoint;
        this.e_EnemyEntryDirection = e_EnemyEntryDirection;
        this.cs_RoomMovePoint = cs_RoomMovePoint;
        this.int_MaxEnemySpawnCount = Mathf.Max(0, int_MaxEnemySpawnCount);
    }
}
