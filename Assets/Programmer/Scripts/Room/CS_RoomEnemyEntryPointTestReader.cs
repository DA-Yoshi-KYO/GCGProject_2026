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
/// CS_RoomEnemyEntryPointCollector が収集した敵出入口リストを取得して、
/// Consoleに内容を表示する確認用クラスです。
/// 
/// このクラスは「敵を生成する本番処理」ではなく、
/// Collectorが正しく敵出入口情報を集められているか確認するために使います。
/// </summary>
public class CS_RoomEnemyEntryPointTestReader : MonoBehaviour
{
    [Header("敵出入口収集クラス")]
    [SerializeField]
    private CS_RoomEnemyEntryPointCollector cs_RoomEnemyEntryPointCollector;

    /// <summary>
    /// ゲーム開始時に実行されます。
    /// 
    /// 通常のStartではなくIEnumerator Startにしている理由は、
    /// Collector側の収集処理が終わるまで待ちたいからです。
    /// </summary>
    private IEnumerator Start()
    {
        // CollectorがInspectorで設定されていない場合、
        // これ以上処理しても敵出入口リストを取得できないので処理を終了します。
        if (cs_RoomEnemyEntryPointCollector == null)
        {
            Debug.LogWarning("[RoomEnemyEntryPointTestReader] Collectorが設定されていません。");
            yield break;
        }

        // Collectorが敵出入口リストを作り終わるまで待ちます。
        while (!cs_RoomEnemyEntryPointCollector.IsCollected)
        {
            yield return null;
        }

        // Collectorが収集した敵出入口リストを取得します。
        //
        // IReadOnlyListなので、このクラス側からは
        // AddやClearなどで中身を変更不可
        IReadOnlyList<CS_RoomEnemyEntryPointData> list_EnemyEntryPointData =
            cs_RoomEnemyEntryPointCollector.EnemyEntryPointDataList;

        // 取得した敵出入口リストを1件ずつ確認します。
        for (int i = 0 ; i < list_EnemyEntryPointData.Count ; i++)
        {
            // リスト内の敵出入口データを1件取り出します。
            //
            // ・どのRoomCreatePointか
            // ・どの扉方向か
            // ・どのRoomMovePointから敵を出すか
            // ・敵を出す座標
            // ・最大何体出せるか
            CS_RoomEnemyEntryPointData cs_EntryPointData = list_EnemyEntryPointData[i];

            // 取得した敵出入口情報をConsoleに表示します。
            //
            // ここで表示される内容を見れば、
            // Collectorが正しく敵生成位置を見つけられているか確認できます。
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
