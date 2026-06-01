using System;
using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomEnemyEntryPointData.cs
 *  制作者      : 吉本竜
 *  内容        : 敵出入口として使用するRoomMovePoint情報を保持する
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *                2026/05/06 敵出入口データScriptableObjectを保持する形へ変更(ヨシモト)
 *                2026/06/01 敵出入口データをListで保持する形へ変更(ヨシモト)
 *==================================================*/

/// <summary>
/// 敵出入口として使用するRoomCreatePoint、方向、RoomMovePoint、敵出入口データをまとめた情報です。
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

    [Header("敵出入口データ")]
    [SerializeField]
    private List<CSS_RoomEnemyEntryData> list_RoomEnemyEntryData =
        new List<CSS_RoomEnemyEntryData>();

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
    /// 敵出入口データリストを取得します。
    /// </summary>
    public IReadOnlyList<CSS_RoomEnemyEntryData> RoomEnemyEntryDataList => list_RoomEnemyEntryData;

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
    public int MaxEnemySpawnCount
    {
        get
        {
            if (list_RoomEnemyEntryData == null)
            {
                return 0;
            }

            int totalMaxEnemySpawnCount = 0;

            for (int i = 0 ; i < list_RoomEnemyEntryData.Count ; i++)
            {
                CSS_RoomEnemyEntryData roomEnemyEntryData = list_RoomEnemyEntryData[i];

                if (roomEnemyEntryData == null)
                {
                    continue;
                }

                totalMaxEnemySpawnCount += roomEnemyEntryData.GetMaxEnemySpawnCount();
            }

            return totalMaxEnemySpawnCount;
        }
    }

    /// <summary>
    /// 敵出入口情報を生成します。
    /// </summary>
    /// <param name="cs_RoomCreatePoint">対象RoomCreatePoint。</param>
    /// <param name="e_EnemyEntryDirection">敵が入ってくる方向。</param>
    /// <param name="cs_RoomMovePoint">敵生成位置として使うRoomMovePoint。</param>
    /// <param name="list_RoomEnemyEntryData">敵出入口データリスト。</param>
    public CS_RoomEnemyEntryPointData(
        CS_RoomCreatePoint cs_RoomCreatePoint,
        CSE_RoomDoorDirection e_EnemyEntryDirection,
        CS_RoomMovePoint cs_RoomMovePoint,
        List<CSS_RoomEnemyEntryData> list_RoomEnemyEntryData)
    {
        this.cs_RoomCreatePoint = cs_RoomCreatePoint;
        this.e_EnemyEntryDirection = e_EnemyEntryDirection;
        this.cs_RoomMovePoint = cs_RoomMovePoint;

        if (list_RoomEnemyEntryData == null)
        {
            this.list_RoomEnemyEntryData = new List<CSS_RoomEnemyEntryData>();
        }
        else
        {
            this.list_RoomEnemyEntryData = new List<CSS_RoomEnemyEntryData>(list_RoomEnemyEntryData);
        }
    }

    /// <summary>
    /// 既存処理との互換用として、敵出入口データ1つから敵出入口情報を生成します。
    /// </summary>
    /// <param name="cs_RoomCreatePoint">対象RoomCreatePoint。</param>
    /// <param name="e_EnemyEntryDirection">敵が入ってくる方向。</param>
    /// <param name="cs_RoomMovePoint">敵生成位置として使うRoomMovePoint。</param>
    /// <param name="css_RoomEnemyEntryData">敵出入口データ。</param>
    public CS_RoomEnemyEntryPointData(
        CS_RoomCreatePoint cs_RoomCreatePoint,
        CSE_RoomDoorDirection e_EnemyEntryDirection,
        CS_RoomMovePoint cs_RoomMovePoint,
        CSS_RoomEnemyEntryData css_RoomEnemyEntryData)
    {
        this.cs_RoomCreatePoint = cs_RoomCreatePoint;
        this.e_EnemyEntryDirection = e_EnemyEntryDirection;
        this.cs_RoomMovePoint = cs_RoomMovePoint;

        list_RoomEnemyEntryData = new List<CSS_RoomEnemyEntryData>();

        if (css_RoomEnemyEntryData != null)
        {
            list_RoomEnemyEntryData.Add(css_RoomEnemyEntryData);
        }
    }
}
