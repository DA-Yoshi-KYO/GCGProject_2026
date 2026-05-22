using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomEnemyEntryPointTestReader.cs
 *  制作者      : 吉本竜
 *  内容        : 収集済みの敵出入口リスト、敵出入口データ、盗賊データリストを確認表示する
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *                2026/05/06 敵出入口データと盗賊データ一覧の表示を追加(ヨシモト)
 *                2026/05/06 処理内容が分かるようにコメントを追加(ヨシモト)
 *==================================================*/

/// <summary>
/// CS_RoomEnemyEntryPointCollector が収集した敵出入口リストを取得し、
/// その中に入っている敵出入口データと盗賊データリストをConsoleに表示する確認用クラス
/// 
/// ・どのRoomCreatePointが敵出入口を持っているか
/// ・どの方向の扉が敵出入口なのか
/// ・どのRoomMovePointを敵生成位置として使うのか
/// ・どの敵出入口データが設定されているのか
/// ・その出入口から最大何体出るのか
/// ・その出入口にどの盗賊データが登録されているのか
/// 
/// </summary>
public class CS_RoomEnemyEntryPointTestReader : MonoBehaviour
{
    [Header("敵出入口収集クラス")]
    [SerializeField]
    private CS_RoomEnemyEntryPointCollector cs_RoomEnemyEntryPointCollector;

    /// <summary>
    /// ゲーム開始時に実行されます。
    /// 対象のオブジェクトが取得するまで待ち。
    /// </summary>
    private IEnumerator Start()
    {
        // InspectorでCollectorが設定されていない場合、
        if (cs_RoomEnemyEntryPointCollector == null)
        {
            Debug.LogWarning("[RoomEnemyEntryPointTestReader] Collectorが設定されていません。");
            yield break;
        }

        // Collectorが敵出入口リストを作り終わるまで待ちます。
        // Collector側はゲーム開始後に少し遅れて収集する設計なので、
        // ここで待機して、Collectorが収集を終えるのを待つ
        while (!cs_RoomEnemyEntryPointCollector.IsCollected)
        {
            yield return null;
        }

        // Collectorが収集した敵出入口リストを取得します。
        // IReadOnlyListで、このクラス側からAddやClearは不可
        IReadOnlyList<CS_RoomEnemyEntryPointData> list_EnemyEntryPointData =
            cs_RoomEnemyEntryPointCollector.EnemyEntryPointDataList;

        // まず、敵出入口が何件取得できたかを表示します。
        Debug.Log("[EnemyEntryPoint取得確認] 敵出入口リスト数 : " + list_EnemyEntryPointData.Count);

        // 取得した敵出入口リストを1件ずつ確認します。
        for (int i = 0 ; i < list_EnemyEntryPointData.Count ; i++)
        {
            // リストから敵出入口データを1件取り出します。
            //
            // この1件の中には、
            // ・対象RoomCreatePoint
            // ・敵が入ってくる方向
            // ・敵生成位置として使うRoomMovePoint
            // ・敵出入口データ

            CS_RoomEnemyEntryPointData cs_EntryPointData = list_EnemyEntryPointData[i];

            // 念のためnullチェックをします。
            // nullの場合、この1件は正常なデータではないので次のデータへ進みます。
            if (cs_EntryPointData == null)
            {
                Debug.LogWarning("[EnemyEntryPoint取得確認] EntryPointDataがnullです。Index : " + i);
                continue;
            }

            // この敵出入口に設定されているScriptableObjectを取得。
            //
            // CSS_RoomEnemyEntryData の中には、
            // ・この出入口から最大何体敵が出るか
            // ・出現候補の盗賊データリスト

            CSS_RoomEnemyEntryData cs_RoomEnemyEntryData =
                cs_EntryPointData.RoomEnemyEntryData;

            // RoomCreatePoint名を取得します。
            // nullの場合でもログ表示でエラーにならないように "null" を入れます。
            string str_RoomCreatePointName =
                cs_EntryPointData.RoomCreatePoint != null
                    ? cs_EntryPointData.RoomCreatePoint.name
                    : "null";

            // RoomMovePoint名を取得します。
            // nullの場合でもログ表示でエラーにならないように "null" を入れます。
            string str_RoomMovePointName =
                cs_EntryPointData.RoomMovePoint != null
                    ? cs_EntryPointData.RoomMovePoint.name
                    : "null";

            // 敵出入口データ名を取得します。
            // nullの場合でもログ表示でエラーにならないように "null" を入れます。
            string str_RoomEnemyEntryDataName =
                cs_RoomEnemyEntryData != null
                    ? cs_RoomEnemyEntryData.name
                    : "null";

            // 敵出入口1件分の基本情報を表示します。
            //
            // ここで確認できるもの：
            // ・リスト上の番号
            // ・どのRoomCreatePointか
            // ・どの扉方向か
            // ・どのRoomMovePointを敵生成位置として使うか
            // ・敵を出す座標
            // ・どの敵出入口データを使っているか
            // ・その出入口から最大何体出るか

            Debug.Log(
                "[EnemyEntryPoint取得確認] " +
                "Index : " + i +
                " / RoomCreatePoint : " + str_RoomCreatePointName +
                " / Direction : " + cs_EntryPointData.EnemyEntryDirection +
                " / RoomMovePoint : " + str_RoomMovePointName +
                " / SpawnPosition : " + cs_EntryPointData.SpawnPosition +
                " / EntryData : " + str_RoomEnemyEntryDataName +
                " / MaxCount : " + cs_EntryPointData.MaxEnemySpawnCount
            );

            // この敵出入口データに登録されている盗賊データリストを表示します。
            //
            // 例：
            // EntryData : BackDoorEnemyData
            //   ThiefData 0 : 盗賊A
            //   ThiefData 1 : 盗賊B
            ShowThiefStatusDataList(cs_RoomEnemyEntryData, i);
        }
    }

    /// <summary>
    /// 指定された敵出入口データに登録されている盗賊データリストをConsoleに表示します。
    /// 
    /// CSS_RoomEnemyEntryData は、1つの敵出入口用の設定データです。
    /// この中に、出現候補となる盗賊データのリストが入っています。
    /// </summary>
    /// <param name="cs_RoomEnemyEntryData">確認対象の敵出入口データ。</param>
    /// <param name="int_EntryIndex">敵出入口リスト上の番号。</param>
    private void ShowThiefStatusDataList(
        CSS_RoomEnemyEntryData cs_RoomEnemyEntryData,
        int int_EntryIndex)
    {
        // 敵出入口データがnullの場合、
        // 盗賊データリストを取得できないため処理を終了します。
        if (cs_RoomEnemyEntryData == null)
        {
            Debug.LogWarning(
                "[EnemyEntryPoint取得確認] 敵出入口データがnullです。EntryIndex : " + int_EntryIndex);

            return;
        }

        // 敵出入口データから、登録されている盗賊データリストを取得します。
        //
        // このリストには、
        // この出入口から出現する可能性がある盗賊のステータスデータ。
        IReadOnlyList<CO_ThiefStatusData> list_ThiefStatusData =
            cs_RoomEnemyEntryData.GetThiefStatusDataList();

        // 盗賊データリスト自体がnullの場合は、
        // 登録内容を確認できないため処理を終了します。
        if (list_ThiefStatusData == null)
        {
            Debug.LogWarning(
                "[EnemyEntryPoint取得確認] 盗賊データリストがnullです。EntryData : " + cs_RoomEnemyEntryData.name);

            return;
        }

        // この敵出入口データに、盗賊データが何件登録されているかを表示します。
        Debug.Log(
            "[EnemyEntryPoint取得確認] " +
            "EntryIndex : " + int_EntryIndex +
            " / EntryData : " + cs_RoomEnemyEntryData.name +
            " / 登録盗賊データ数 : " + list_ThiefStatusData.Count
        );

        // 登録されている盗賊データを1件ずつ表示します。
        for (int i = 0 ; i < list_ThiefStatusData.Count ; i++)
        {
            // 盗賊データを1件取得します。
            CO_ThiefStatusData cs_ThiefStatusData = list_ThiefStatusData[i];

            // リストの中にnullが入っている場合、
            // その番号だけ警告を出して次のデータへ進みます。
            if (cs_ThiefStatusData == null)
            {
                Debug.LogWarning(
                    "[EnemyEntryPoint取得確認] " +
                    "EntryIndex : " + int_EntryIndex +
                    " / ThiefIndex : " + i +
                    " / 盗賊データ : null");

                continue;
            }

            // 盗賊データの内容を表示します。
            //
            // 表示するもの：
            // ・どの敵出入口データに登録されているか
            // ・盗賊リスト上の番号
            // ・盗賊データのScriptableObject名
            // ・盗賊名
            Debug.Log(
                "[EnemyEntryPoint取得確認] " +
                "EntryIndex : " + int_EntryIndex +
                " / ThiefIndex : " + i +
                " / ThiefData : " + cs_ThiefStatusData.name
                );
        }
    }
}
