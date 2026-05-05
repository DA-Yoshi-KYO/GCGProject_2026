using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomEnemyEntryPointCollector.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePoints配下から敵出入口として使うRoomMovePointを収集する
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePoints配下から、敵出入口として設定されているRoomMovePointを収集してキャッシュするクラスです。
/// </summary>
public class CS_RoomEnemyEntryPointCollector : MonoBehaviour
{
    [Header("検索対象の親")]
    [SerializeField]
    private Transform tr_RoomCreatePointsRoot;

    [Header("RoomCreatePointタグ名")]
    [SerializeField]
    private string str_RoomCreatePointTag = "RoomCreatePoint";

    [Header("開始時の検索遅延秒")]
    [SerializeField, Min(0.0f)]
    private float f_SearchDelaySeconds = 0.2f;

    [Header("デバッグログを出すか")]
    [SerializeField]
    private bool bool_IsDebugLog = true;

    [Header("取得済み 敵出入口リスト")]
    [SerializeField]
    private List<CS_RoomEnemyEntryPointData> list_EnemyEntryPointData = new List<CS_RoomEnemyEntryPointData>();

    /// <summary>
    /// 敵出入口リストを取得します。
    /// </summary>
    public IReadOnlyList<CS_RoomEnemyEntryPointData> EnemyEntryPointDataList => list_EnemyEntryPointData;

    /// <summary>
    /// 収集済みかどうかを取得します。
    /// </summary>
    public bool IsCollected { get; private set; }

    /// <summary>
    /// ゲーム開始時に少し遅らせて、敵出入口情報を1回だけ収集します。
    /// </summary>
    private IEnumerator Start()
    {
        yield return null;

        if (f_SearchDelaySeconds > 0.0f)
        {
            yield return new WaitForSeconds(f_SearchDelaySeconds);
        }

        CollectEnemyEntryPointData();
    }

    /// <summary>
    /// RoomCreatePoints配下から敵出入口情報を収集します。
    /// </summary>
    public void CollectEnemyEntryPointData()
    {
        list_EnemyEntryPointData.Clear();

        Transform tr_SearchRoot = tr_RoomCreatePointsRoot;

        if (tr_SearchRoot == null)
        {
            tr_SearchRoot = transform;
        }

        CS_RoomCreatePoint[] array_RoomCreatePoints =
            tr_SearchRoot.GetComponentsInChildren<CS_RoomCreatePoint>(true);

        for (int i = 0 ; i < array_RoomCreatePoints.Length ; i++)
        {
            CS_RoomCreatePoint cs_RoomCreatePoint = array_RoomCreatePoints[i];

            if (cs_RoomCreatePoint == null)
            {
                continue;
            }

            if (!IsTargetRoomCreatePoint(cs_RoomCreatePoint.gameObject))
            {
                continue;
            }

            CollectEnemyEntryPointDataFromRoomCreatePoint(cs_RoomCreatePoint);
        }

        IsCollected = true;

        if (bool_IsDebugLog)
        {
            Debug.Log($"[RoomEnemyEntryPointCollector] 敵出入口を {list_EnemyEntryPointData.Count} 件取得しました。");
        }
    }

    /// <summary>
    /// 1つのRoomCreatePointから敵出入口情報を収集します。
    /// </summary>
    /// <param name="cs_RoomCreatePoint">確認対象のRoomCreatePoint。</param>
    private void CollectEnemyEntryPointDataFromRoomCreatePoint(CS_RoomCreatePoint cs_RoomCreatePoint)
    {
        List<CSE_RoomDoorDirection> list_EnemyEntryDirections =
            cs_RoomCreatePoint.GetEnemyEntryDirections();

        if (list_EnemyEntryDirections.Count <= 0)
        {
            return;
        }

        CS_RoomMovePoint[] array_RoomMovePoints =
            cs_RoomCreatePoint.GetComponentsInChildren<CS_RoomMovePoint>(true);

        for (int i = 0 ; i < list_EnemyEntryDirections.Count ; i++)
        {
            CSE_RoomDoorDirection e_EnemyEntryDirection = list_EnemyEntryDirections[i];

            if (!cs_RoomCreatePoint.TryGetEnemyEntryData(
                    e_EnemyEntryDirection,
                    out CS_RoomEnemyEntryDataSO cs_RoomEnemyEntryDataSO))
            {
                continue;
            }

            CS_RoomMovePoint cs_RoomMovePoint =
                FindRoomMovePointByDirection(array_RoomMovePoints, e_EnemyEntryDirection);

            if (cs_RoomMovePoint == null)
            {
                Debug.LogWarning(
                    $"[RoomEnemyEntryPointCollector] {cs_RoomCreatePoint.name} の {e_EnemyEntryDirection} に対応するRoomMovePointが見つかりません。");

                continue;
            }

            CS_RoomEnemyEntryPointData cs_EnemyEntryPointData =
                new CS_RoomEnemyEntryPointData(
                    cs_RoomCreatePoint,
                    e_EnemyEntryDirection,
                    cs_RoomMovePoint,
                    cs_RoomEnemyEntryDataSO);

            list_EnemyEntryPointData.Add(cs_EnemyEntryPointData);

            if (bool_IsDebugLog)
            {
                Debug.Log(
                    $"[EnemyEntry] RoomCreatePoint:{cs_RoomCreatePoint.name} / Direction:{e_EnemyEntryDirection} / RoomMovePoint:{cs_RoomMovePoint.name} / MaxCount:{cs_EnemyEntryPointData.MaxEnemySpawnCount}");
            }
        }
    }

    /// <summary>
    /// 指定方向に設定されているRoomMovePointを取得します。
    /// </summary>
    /// <param name="array_RoomMovePoints">検索対象のRoomMovePoint配列。</param>
    /// <param name="e_Direction">探したい出入口方向。</param>
    /// <returns>一致したRoomMovePoint。見つからない場合はnull。</returns>
    private CS_RoomMovePoint FindRoomMovePointByDirection(
        CS_RoomMovePoint[] array_RoomMovePoints,
        CSE_RoomDoorDirection e_Direction)
    {
        for (int i = 0 ; i < array_RoomMovePoints.Length ; i++)
        {
            CS_RoomMovePoint cs_RoomMovePoint = array_RoomMovePoints[i];

            if (cs_RoomMovePoint == null)
            {
                continue;
            }

            if (cs_RoomMovePoint.MoveDirection == e_Direction)
            {
                return cs_RoomMovePoint;
            }
        }

        return null;
    }

    /// <summary>
    /// 指定GameObjectが検索対象のRoomCreatePointかどうかを確認します。
    /// </summary>
    /// <param name="targetObject">確認対象GameObject。</param>
    /// <returns>検索対象の場合はtrue。</returns>
    private bool IsTargetRoomCreatePoint(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(str_RoomCreatePointTag))
        {
            return true;
        }

        return targetObject.tag == str_RoomCreatePointTag;
    }
}
