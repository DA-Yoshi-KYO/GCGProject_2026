using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomCreatePoint.cs
 *  制作者      : 吉本竜
 *  内容        : ランダム生成Roomの配置位置と出入口情報を管理する
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *                2026/05/06 出入口用途と敵侵入設定の取得処理を追加(ヨシモト)
 *==================================================*/

/// <summary>
/// ルーム生成位置と、各方向の出入口情報を持つポイントです。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomCreatePoint : MonoBehaviour
{
    [Header("部屋のタイプ")]
    [SerializeField]
    private CSE_RoomTypeEnum e_RoomType;

    /// <summary>
    /// 部屋の種類
    /// </summary>
    public CSE_RoomTypeEnum RoomType
    {
        get { return e_RoomType; }
    }

    [Header("右出口の設定")]
    [SerializeField]
    private CS_RoomMoveConnection cs_RightConnection = new CS_RoomMoveConnection();

    [Header("左出口の設定")]
    [SerializeField]
    private CS_RoomMoveConnection cs_LeftConnection = new CS_RoomMoveConnection();

    [Header("前出口の設定")]
    [SerializeField]
    private CS_RoomMoveConnection cs_FrontConnection = new CS_RoomMoveConnection();

    [Header("後ろ出口の設定")]
    [SerializeField]
    private CS_RoomMoveConnection cs_BackConnection = new CS_RoomMoveConnection();

    /// <summary>
    /// 右のドアのワールド座標
    /// </summary>
    private Transform RightDoorPoints;

    /// <summary>
    /// 左のドアのワールド座標
    /// </summary>
    private Transform LeftDoorPoints;

    /// <summary>
    /// 前のドアのワールド座標
    /// </summary>
    private Transform FrontDoorPoints;

    /// <summary>
    /// 後ろのドアのワールド座標
    /// </summary>
    private Transform BackDoorPoints;

    /// <summary>
    /// 指定方向のワープ接続情報を取得します。
    /// 敵出入口や未設定の場合はfalseを返します。
    /// </summary>
    /// <param name="e_FromDirection">このRoomから出る方向。</param>
    /// <param name="cs_Connection">取得した接続情報。</param>
    /// <returns>ワープ接続先がある場合はtrue。</returns>
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
    /// 指定方向の敵出入口データを取得します。
    /// 敵出入口ではない場合、またはデータ未設定の場合はfalseを返します。
    /// </summary>
    /// <param name="e_FromDirection">確認したい出入口方向。</param>
    /// <param name="cs_EnemyEntryDataSO">取得した敵出入口データ。</param>
    /// <returns>敵出入口データがある場合はtrue。</returns>
    public bool TryGetEnemyEntryData(
        CSE_RoomDoorDirection e_FromDirection,
        out CSS_RoomEnemyEntryData cs_EnemyEntryDataSO)
    {
        cs_EnemyEntryDataSO = null;

        CS_RoomMoveConnection cs_Connection = GetConnection(e_FromDirection);

        if (cs_Connection == null)
        {
            return false;
        }

        if (!(cs_Connection.GetEnemyEntryCount() > 0))
        {
            return false;
        }

        cs_EnemyEntryDataSO = cs_Connection.RoomEnemyEntryDataSO;
        return true;
    }

    /// <summary>
    /// 指定方向の敵最大出現数を取得します。
    /// 敵出入口ではない場合は0を返します。
    /// </summary>
    /// <param name="e_FromDirection">確認したい出入口方向。</param>
    /// <returns>敵の最大出現数。</returns>
    public int GetMaxEnemySpawnCount(CSE_RoomDoorDirection e_FromDirection)
    {
        CS_RoomMoveConnection cs_Connection = GetConnection(e_FromDirection);

        if (cs_Connection == null)
        {
            return 0;
        }

        return cs_Connection.GetMaxEnemySpawnCount();
    }

    /// <summary>
    /// 指定方向の敵侵入数を取得します。
    /// 互換用として、最大出現数を返します。
    /// </summary>
    /// <param name="e_FromDirection">確認したい出入口方向。</param>
    /// <returns>敵の最大出現数。</returns>
    public int GetEnemyEntryCount(CSE_RoomDoorDirection e_FromDirection)
    {
        return GetMaxEnemySpawnCount(e_FromDirection);
    }

    /// <summary>
    /// 指定方向の扉用途を取得します。
    /// </summary>
    /// <param name="e_FromDirection">確認したい出入口方向。</param>
    /// <returns>扉の用途。</returns>
    public CSE_RoomDoorUsageType GetDoorUsageType(CSE_RoomDoorDirection e_FromDirection)
    {
        CS_RoomMoveConnection cs_Connection = GetConnection(e_FromDirection);

        if (cs_Connection == null)
        {
            return CSE_RoomDoorUsageType.None;
        }

        return cs_Connection.GetDoorUsageType;
    }

    /// <summary>
    /// ワープ接続先が設定されている方向を全て取得します。
    /// </summary>
    /// <returns>ワープ接続先がある方向リスト。</returns>
    public List<CSE_RoomDoorDirection> GetConnectDirections()
    {
        List<CSE_RoomDoorDirection> list_ConnectDirections = new List<CSE_RoomDoorDirection>();

        if (cs_RightConnection.HasTarget)
        {
            list_ConnectDirections.Add(CSE_RoomDoorDirection.Right);
        }

        if (cs_LeftConnection.HasTarget)
        {
            list_ConnectDirections.Add(CSE_RoomDoorDirection.Left);
        }

        if (cs_FrontConnection.HasTarget)
        {
            list_ConnectDirections.Add(CSE_RoomDoorDirection.Front);
        }

        if (cs_BackConnection.HasTarget)
        {
            list_ConnectDirections.Add(CSE_RoomDoorDirection.Back);
        }

        return list_ConnectDirections;
    }

    /// <summary>
    /// 敵出入口として設定されている方向を全て取得します。
    /// </summary>
    /// <returns>敵出入口の方向リスト。</returns>
    public List<CSE_RoomDoorDirection> GetEnemyEntryDirections()
    {
        List<CSE_RoomDoorDirection> list_EnemyEntryDirections = new List<CSE_RoomDoorDirection>();

        if (cs_RightConnection.GetEnemyEntryCount() > 0)
        {
            list_EnemyEntryDirections.Add(CSE_RoomDoorDirection.Right);
        }

        if (cs_LeftConnection.GetEnemyEntryCount() > 0)
        {
            list_EnemyEntryDirections.Add(CSE_RoomDoorDirection.Left);
        }

        if (cs_FrontConnection.GetEnemyEntryCount() > 0)
        {
            list_EnemyEntryDirections.Add(CSE_RoomDoorDirection.Front);
        }

        if (cs_BackConnection.GetEnemyEntryCount() > 0)
        {
            list_EnemyEntryDirections.Add(CSE_RoomDoorDirection.Back);
        }

        return list_EnemyEntryDirections;
    }

    /// <summary>
    /// 全ての敵出入口データをクリアします。
    /// </summary>
    public void ClearEnemyEntryDirections()
    {
        cs_RightConnection.ClearEnemyEntryData();
        cs_LeftConnection.ClearEnemyEntryData();
        cs_FrontConnection.ClearEnemyEntryData();
        cs_BackConnection.ClearEnemyEntryData();
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
    /// 指定方向の敵出入口データを設定します。
    /// </summary>
    /// <param name="e_FromDirection">指定方向</param>
    /// <param name="newData">設定する敵出入口データ</param>
    public void SetEnemyData(CSE_RoomDoorDirection e_FromDirection, CSS_RoomEnemyEntryData newData)
    {
        CS_RoomMoveConnection cs_Connection = GetConnection(e_FromDirection);
        if (cs_Connection == null)
        {
            Debug.LogError($"指定された方向の接続情報が見つかりません。方向: {e_FromDirection}");
            return;
        }
        if (!cs_Connection.IsEnemyEntryDoor)
        {
            Debug.LogError($"指定された方向は敵出入口ではありません。方向: {e_FromDirection}");
            return;
        }
        cs_Connection.SetEnemyEntryData(newData);
    }

    /// <summary>
    /// 各ドアのワールド座標を設定します。
    /// </summary>
    private void SetDoorWorldPosition()
    {
        // 自分と子オブジェクトのCS_RoomMovePointコンポーネントを全て取得
        CS_RoomMovePoint[] components = GetComponentsInChildren<CS_RoomMovePoint>();

        // チェック用データ
        CSE_RoomDoorUsageType data = cs_RightConnection.GetDoorUsageType;

        // ドアがある場合のみ、ワールド座標を設定
        if (data != CSE_RoomDoorUsageType.None)
        {
            // 取得したコンポーネントの数だけループ
            for (int i = 0 ; i < components.Length ; i++)
            {
                // 右のドア座標を設定
                if (components[i].MoveDirection == CSE_RoomDoorDirection.Right)
                {
                    // ワールド座標を設定
                    RightDoorPoints = components[i].transform;
                }
            }
        }

        // 左のドア座標を設定
        data = cs_LeftConnection.GetDoorUsageType;
        if (data != CSE_RoomDoorUsageType.None)
        {
            for (int i = 0 ; i < components.Length ; i++)
            {
                if (components[i].MoveDirection == CSE_RoomDoorDirection.Left)
                {
                    LeftDoorPoints = components[i].transform;
                }
            }
        }

        // 前のドア座標を設定
        data = cs_FrontConnection.GetDoorUsageType;
        if (data != CSE_RoomDoorUsageType.None)
        {
            for (int i = 0 ; i < components.Length ; i++)
            {
                if (components[i].MoveDirection == CSE_RoomDoorDirection.Front)
                {
                    FrontDoorPoints = components[i].transform;
                }
            }
        }

        // 後ろのドア座標を設定
        data = cs_BackConnection.GetDoorUsageType;
        if (data != CSE_RoomDoorUsageType.None)
        {
            for (int i = 0 ; i < components.Length ; i++)
            {
                if (components[i].MoveDirection == CSE_RoomDoorDirection.Back)
                {
                    BackDoorPoints = components[i].transform;
                }
            }
        }
    }

    /// <summary>
    /// ドアのワールド座標を取得します。(RoomCreatePointをゲットコンポオ―ネントする前提)
    /// </summary>
    /// <param name="data">取得したい方向</param>
    /// <returns></returns>
    public Transform GetRoomDoorPosition(CSE_RoomDoorDirection data)
    {
        SetDoorWorldPosition();

        switch (data)
        {
        case CSE_RoomDoorDirection.Right:
            return RightDoorPoints;
        case CSE_RoomDoorDirection.Left:
            return LeftDoorPoints;
        case CSE_RoomDoorDirection.Front:
            return FrontDoorPoints;
        case CSE_RoomDoorDirection.Back:
            return BackDoorPoints;
        default:
            return null;
        }
    }
}
