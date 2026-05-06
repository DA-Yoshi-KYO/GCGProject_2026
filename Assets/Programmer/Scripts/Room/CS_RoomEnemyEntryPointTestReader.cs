using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomEnemyEntryPointTestReader.cs
 *  制作者      : 吉本竜
 *  内容        : 収集済みの敵出入口リストを取得して確認する
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// CS_RoomEnemyEntryPointCollector が収集した敵出入口リストを取得して確認するクラスです。
/// </summary>
public class CS_RoomEnemyEntryPointTestReader : MonoBehaviour
{
    [Header("敵出入口収集クラス")]
    [SerializeField]
    private CS_RoomEnemyEntryPointCollector cs_RoomEnemyEntryPointCollector;

    /// <summary>
    /// Collectorの収集完了を待ってから、敵出入口リストを取得します。
    /// </summary>
    private IEnumerator Start()
    {
        if (cs_RoomEnemyEntryPointCollector == null)
        {
            Debug.LogWarning("[RoomEnemyEntryPointTestReader] Collectorが設定されていません。");
            yield break;
        }

        while (!cs_RoomEnemyEntryPointCollector.IsCollected)
        {
            yield return null;
        }

        IReadOnlyList<CS_RoomEnemyEntryPointData> list_EnemyEntryPointData =
            cs_RoomEnemyEntryPointCollector.EnemyEntryPointDataList;

        for (int i = 0 ; i < list_EnemyEntryPointData.Count ; i++)
        {
            CS_RoomEnemyEntryPointData cs_EntryPointData = list_EnemyEntryPointData[i];

            Debug.Log(
                "[EnemyEntryPoint取得確認] " +
                "RoomCreatePoint : " + cs_EntryPointData.RoomCreatePoint.name +
                " / Direction : " + cs_EntryPointData.EnemyEntryDirection +
                " / RoomMovePoint : " + cs_EntryPointData.RoomMovePoint.name +
                " / SpawnPosition : " + cs_EntryPointData.SpawnPosition +
                " / MaxCount : " + cs_EntryPointData.MaxEnemySpawnCount
            );
        }
    }
}
