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
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using static WaveData;


// 泥棒を生成するシステム
public class ThiefManager : MonoBehaviour
{
    [SerializeField, Tooltip("泥棒の種類共通パラメーターのデータベース")]
    private CSS_ThiefCommonStatusData thiefCommonDB;
    [SerializeField, Tooltip("ステージごとのウェーブデータのデータベース")]
    private StageDataSO stageDataDB;
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
            RoomNode entryRoom = entry.RoomCreatePoint.transform.GetComponentInChildren<RoomNode>();

            //泥棒の生成
            GameObject thief = GameObject.Instantiate(thiefPrefab);

            // 基準となるプレイヤーの速度を取得
            float playerSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<CS_PlayerMove>().GetBasePlayerSpeed();

            // 泥棒のタイプに応じたデータを取得
            CSS_ThiefStatusData typeData = entry.RoomEnemyEntryData.GetThiefStatusDataList()[spawnCount[entry]];

            // 行動AIの設定
            ThiefAI thiefAI = thief.GetComponent<ThiefAI>();
            thiefAI.Setting(typeData, thiefCommonDB, playerSpeed, entryRoom, entryPoint);

            // 視界システムの設定
            VisionSensor thiefView = thief.GetComponent<VisionSensor>();
            thiefView.Setting(typeData.viewDistance, typeData.viewAngle);

            // リアクションスの設定
            ThiefReaction thiefReaction = thief.GetComponent<ThiefReaction>();
            thiefReaction.RegisterReaction(typeData.reactionSprites);

            // --- 泥棒をthiefParentの子オブジェクトに設定
            thief.transform.parent = thiefParent.transform;

            //--- 生成した泥棒の生成位置を選定
            thief.transform.position = entryPoint.position;

            // 生成された泥棒の数を更新
            spawnCount[entry]++;
        }

        //// 現在のウェーブ数を取得
        //int currentWave = GameObject.Find("ThiefManager").GetComponent<WaveManager>().waveNumber;

        ///*仮で実数変数として指定*/int stageNumber = 1;

        //// 現在のウェーブ数に応じた
        //StageDataSO stageData = ScriptableObject.Instantiate(stageDataDB);
        //WaveData.ThiefData[] thiefDatas = stageData.stageData[stageNumber - 1].waveDatas[currentWave - 1].thiefDataArray;

        //// 泥棒のデータをもとに泥棒を生成
        //foreach (var thiefData in thiefDatas)
        //{
        //    // 泥棒のタイプに応じたデータを取得
        //    ThiefTypeData typeData = new ThiefTypeData();
        //    // 泥棒の種類間で共通のデータを取得
        //    ThiefData commonData = thiefDB.commonData;

        //    // 泥棒のデータベースから、泥棒のタイプに応じたデータを取得
        //    for (int i = 0 ; i < thiefDB.thiefData.Length ; i++)
        //    {
        //        if(thiefDB.thiefData[i].typeName == thiefData.type)
        //        {
        //            typeData = thiefDB.thiefData[i];
        //            break;
        //        }
        //    }


        //    // 生成する泥棒の親オブジェクトを取得、存在しない場合は生成
        //    GameObject thiefParent = GameObject.Find("ThiefParent");
        //    if (thiefParent == null)
        //    {
        //        thiefParent = new GameObject("ThiefParent");
        //    }


        //    //泥棒の生成
        //    for (int i = 0 ; i < thiefData.count ; i++)
        //    {
        //        GameObject thief = GameObject.Instantiate(thiefPrefab);
        //        //--- 泥棒のデータを設定

        //        /* 仮で実数変数でプレイヤー速度を用意 */
        //        float playerSpeed = 10.0f;

        //        // 行動AIの設定
        //        ThiefAI thiefAI = thief.GetComponent<ThiefAI>();
        //        thiefAI.Setting(typeData, commonData, playerSpeed, FindObjectOfType<RoomNode>());

        //        // 視界システムの設定
        //        VisionSensor thiefView = thief.GetComponent<VisionSensor>();
        //        thiefView.Setting(typeData.viewDistance, typeData.viewAngle);

        //        // リアクションスの設定
        //        ThiefReaction thiefReaction = thief.GetComponent<ThiefReaction>();
        //        thiefReaction.RegisterReaction(typeData.reactionSprites);

        //        // --- 泥棒をthiefParentの子オブジェクトに設定
        //        thief.transform.parent = thiefParent.transform;

        //        //--- 生成した泥棒の生成位置を選定

        //        GameObject debugPoint = GameObject.Find("Debug_ThiefPoint");
        //        if (debugPoint != null)
        //        {
        //            // デバッグ用の生成ポイントが存在する場合は、そこに生成
        //            thief.transform.position = debugPoint.transform.position;
        //            continue;
        //        }
        //    }
        //}
    }

    // 指定したオブジェクトの記憶を消去するメソッド
    public void EraseTheMemoryToAllThief(ThiefTarget obj)
    {
        // 全泥棒を取得
        GameObject[] thieves = GameObject.FindAnyObjectByType<ThiefAI>().gameObject.scene.GetRootGameObjects();
        foreach (var thief in thieves)
        {
            ThiefAI thiefAI = thief.GetComponentInChildren<ThiefAI>();
            if (thiefAI != null)
            {
                thiefAI.EraseTheMemory(obj);
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

        // 泥棒を再生成
        Notify();
    }

}
