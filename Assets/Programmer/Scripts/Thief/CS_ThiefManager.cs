/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-19 | 泥棒のパラメーター設定処理の記載(行動AIの設定、視界システムの設定)
 * 2026-04-23 | 移動速度の設定処理の記載(プレイヤーの速度を仮で用意して、そこから泥棒の速度を計算するように変更)
 * 2026-04-26 | ファイル名・クラス名をThiefManagerに変更
 * 2026-05-07 | CS_RoomEnemyEntryPointDataを用いた泥棒の生成処理の記載
 *            | 生成タイムと生成数の管理の記載
 *            | 生成位置の選定の記載
 * 2026-05-22 | ファイル名を変更（ThiefManager.cs → CS_ThiefManager.cs）
 *            | クラス名を変更（ThiefManager → CS_ThiefManager）
 * 
 */
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 泥棒を管理するクラス
/// </summary>
public class CS_ThiefManager : MonoBehaviour
{
    [SerializeField, Tooltip("泥棒の種類共通パラメーターのデータベース")]
    private CO_ThiefCommonStatusData thiefCommonDB;
    public CO_ThiefCommonStatusData GetThiefCommonDB() { return GameObject.Instantiate(thiefCommonDB); }
    [SerializeField, Tooltip("泥棒のプレハブ")]
    private GameObject thiefPrefab;

    [Tooltip("敵の出入口のデータと、次にそこから泥棒が生成されるまでの時間を管理する辞書")]
    private Dictionary<CS_RoomEnemyEntryPointData, float> createTime = new Dictionary<CS_RoomEnemyEntryPointData, float>();
    [Tooltip("敵の出入口のデータと、そこから生成された泥棒の数を管理する辞書")]
    private Dictionary<CS_RoomEnemyEntryPointData, int> spawnCount = new Dictionary<CS_RoomEnemyEntryPointData, int>();

    [SerializeField, Header("次の泥棒を生成するまでの感覚(秒)"), Tooltip("泥棒を生成する間隔の基本値")]
    private float createInterval = 1.0f;
    [SerializeField, Header("最初の泥棒を生成するまでの時間(秒)"), Tooltip("最初の生成間隔")]
    private float firstCreateInterval = 5.0f;

    /// <summary>
    /// 毎フレーム、敵の出入口から泥棒を生成する処理を行う
    /// </summary>
    private void Update()
    {
        Notify();
    }

    // 泥棒を生成するメソッド
    private void Notify()
    {
        // 敵の出入口を取得
        IReadOnlyList<CS_RoomEnemyEntryPointData> EntryList = GameObject.Find("RoomCreatePoints").GetComponent<CS_RoomEnemyEntryPointCollector>().EnemyEntryPointDataList;
        if (EntryList.Count == 0)
        {
            return; // 出入口が存在しない場合は、処理を終了
        }

        foreach (var entry in EntryList)
        {
            // 出入口から 生成された泥棒の数が最大数に達している場合は、次の出入口の処理に移る
            if (spawnCount.ContainsKey(entry))
            {
                // NULLチェック
                if (entry.RoomEnemyEntryData == null) continue;
                if (entry.RoomEnemyEntryData.GetThiefStatusDataList() == null) continue;

                if (spawnCount[entry] >= entry.RoomEnemyEntryData.GetThiefStatusDataList().Count) continue;
            }
            else spawnCount.Add(entry, 0); // 新しい出入口を辞書に追加

            // 生成タイムの登録と更新
            if (createTime.ContainsKey(entry))
            {
                if (createTime[entry] <= 0)
                {
                    // 泥棒を生成する処理
                    createTime[entry] = createInterval; // 次の生成までの時間をリセット
                }
                else
                {
                    // 生成タイムを減らす
                    createTime[entry] -= Time.deltaTime;
                    continue; // 生成タイムがまだ残っている場合は、次の出入口の処理に移る
                }
            }
            // 新しい出入口を辞書に追加
            else
            {
                createTime.Add(entry, firstCreateInterval);
                continue; // 最初の生成間隔を待つため、次の出入口の処理に移る
            }


            // 生成する泥棒の親オブジェクトを取得、存在しない場合は生成
            GameObject thiefParent = GameObject.Find("ThiefParent");
            if (thiefParent == null)
            {
                thiefParent = new GameObject("ThiefParent");
            }

            // 生成位置の取得
            Transform entryPoint = entry.RoomMovePointObject.transform;
            // 生成される初期部屋の取得
            CS_RoomNode entryRoom = entry.RoomCreatePoint.transform.GetComponentInChildren<CS_RoomNode>();

            //泥棒の生成
            GameObject thief = GameObject.Instantiate(thiefPrefab);

            thief.name = "Thief_" + thiefParent.transform.childCount;

            // 基準となるプレイヤーの速度を取得
            float playerSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<CS_PlayerMove>().GetBasePlayerSpeed();

            // 泥棒のタイプに応じたデータを取得
            CO_ThiefStatusData typeData = entry.RoomEnemyEntryData.GetThiefStatusDataList()[spawnCount[entry]];

            // 行動AIの設定
            CS_ThiefAI thiefAI = thief.GetComponent<CS_ThiefAI>();
            thiefAI.Setting(GameObject.Instantiate(typeData), GetThiefCommonDB(), playerSpeed, entryRoom, entryPoint);

            // 視界システムの設定
            CS_VisionSensor thiefView = thief.GetComponent<CS_VisionSensor>();
            thiefView.Setting(typeData.viewDistance, typeData.viewAngle);

            // --- 泥棒をthiefParentの子オブジェクトに設定
            thief.transform.parent = thiefParent.transform;

            //--- 生成した泥棒の生成位置を選定
            thief.transform.position = entryPoint.position;

            // 生成された泥棒の数を更新
            spawnCount[entry]++;
        }
    }

    /// <summary>
    /// 全ての泥棒の記憶から指定されたターゲットを消す処理
    /// </summary>
    /// <param name="obj">記憶から消すターゲット</param>
    public void EraseTheMemoryToAllThief(CS_ThiefTarget obj)
    {
        // 全泥棒を取得
        GameObject[] thieves = GameObject.FindAnyObjectByType<CS_ThiefAI>().gameObject.scene.GetRootGameObjects();
        foreach (var thief in thieves)
        {
            CS_ThiefAI thiefAI = thief.GetComponentInChildren<CS_ThiefAI>();
            if (thiefAI != null)
            {
                thiefAI.EraseTheMemory(obj);
            }
        }

    }

    /// <summary>
    /// 逃走中の泥棒が存在するかを返す処理(宝物を持っていかれているかどうか)
    /// </summary>
    /// <returns>逃走中の泥棒が存在する場合はtrue、存在しない場合はfalse</returns>
    public bool IsEscapeThief()
    {
        // 泥棒の親オブジェクトを取得
        GameObject thiefParent = GameObject.Find("ThiefParent");
        if (thiefParent == null)
        {
            Debug.LogError("ThiefParentが存在しません。");
            return false;
        }

        // 全ての泥棒をチェックして、逃走中の泥棒が存在するかを確認
        for (int i = 0 ; i < thiefParent.transform.childCount ; i++)
        {
            GameObject thief = thiefParent.transform.GetChild(i).gameObject;
            CS_ThiefAI thiefAI = thief.GetComponent<CS_ThiefAI>();
            if (thiefAI == null) continue;

            if (thiefAI.CurrentState == CS_ThiefAI.ThiefState.Escape)
            {
                return true; // 逃走中の泥棒が存在する場合はtrueを返す
            }
        }

        return false;
    }

    // 指定の危険地帯IDを全ての泥棒の記憶から消す処理
    public void EraseTheAvoidZoneIDToAllThief(int zoneID)
    {
        // 泥棒の親オブジェクトを取得
        GameObject thiefParent = GameObject.Find("ThiefParent");
        if (thiefParent == null)
        {
            Debug.LogError("ThiefParentが存在しません。");
        }

        // 全ての泥棒をチェックして、指定の危険地帯IDを記憶から消す
        for (int i = 0; i < thiefParent.transform.childCount; i++)
        {
            GameObject thief = thiefParent.transform.GetChild(i).gameObject;
            CS_ThiefAI thiefAI = thief.GetComponent<CS_ThiefAI>();
            if (thiefAI != null)
            {
                thiefAI.RemoveAvoidZoneID(zoneID);
            }
        }
    }

    //////////////////////////////////////////////////////////////////
    /// デバック用の処理

    [ContextMenu("泥棒を再生成")]
    private void DebugNotify()
    {
        // 生成した泥棒を全て削除
        GameObject thiefParent = GameObject.Find("ThiefParent");
        if (thiefParent != null)
        {
            Destroy(thiefParent);
        }

        spawnCount.Clear(); // 生成数の管理をリセット

        // 泥棒を再生成
        Notify();
    }

}
