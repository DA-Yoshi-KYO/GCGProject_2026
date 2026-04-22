/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒を生成するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-19 | 泥棒のパラメーター設定処理の記載(行動AIの設定、視界システムの設定)
 * 
 */
using UnityEditor;
using UnityEngine;


// 泥棒を生成するシステム
public class ThiefGenerator
{
    [Tooltip("泥棒のデータベース")]
    private ThiefDataSO thiefDB;
    [Tooltip("ステージごとのウェーブデータのデータベース")]
    private StageDataSO stageDataDB;
    [Tooltip("泥棒のプレハブ")]
    private GameObject thiefPrefab;

    [Tooltip("使用する泥棒データベースのパス")]
    private string thiefDBPath = "Assets/Programmer/ScriptableObject/ThiefDB.asset";
    [Tooltip("使用するステージデータベースのパス")]
    private string stageDataDBPath = "Assets/Programmer/ScriptableObject/StageDB.asset";
    [Tooltip("泥棒のプレハブのパス")]
    private string thiefPrefabPath = "Assets/Programmer/Prefabs/Thief.prefab";

    private void Start()
    {
        // 泥棒のデータベースをロード
        thiefDB = AssetDatabase.LoadAssetAtPath<ThiefDataSO>(thiefDBPath);
        // ステージごとのウェーブデータのデータベースをロード
        stageDataDB = AssetDatabase.LoadAssetAtPath<StageDataSO>(stageDataDBPath);
        // 泥棒のプレハブをロード
        thiefPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(thiefPrefabPath);
    }

    // 泥棒を生成するメソッド
    public void Notify()
    {
        // 現在のウェーブ数を取得
        int currentWave = GameObject.Find("Manager").GetComponent<WaveManager>().waveNumber;


        /*仮で実数変数として指定*/int stageNumber = 1;

        // 現在のウェーブ数に応じた
        StageDataSO stageData = ScriptableObject.Instantiate(stageDataDB);
        WaveData.ThiefData[] thiefDatas = stageData.stageData[stageNumber - 1].waveDatas[currentWave - 1].thiefDataArray;

        // 泥棒のデータをもとに泥棒を生成
        foreach (var thiefData in thiefDatas)
        {
            // 泥棒のタイプに応じたデータを取得
            ThiefData data = new ThiefData();

            for (int i = 0 ; i < thiefDB.thiefData.Length ; i++)
            {
                if(thiefDB.thiefData[i].typeName == thiefData.type)
                {
                    data = thiefDB.thiefData[i];
                    break;
                }
            }

            //泥棒の生成
            for (int i = 0 ; i < thiefData.count ; i++)
            {
                GameObject thief = GameObject.Instantiate(thiefPrefab);
                //--- 泥棒のデータを設定

                // 行動AIの設定
                ThiefAI thiefAI = thief.GetComponent<ThiefAI>();
                thiefAI.Setting(data.durability, data.speed, data.nextRoomSearchThreshold);

                // 視界システムの設定
                VisionSensor thiefView = thief.GetComponent<VisionSensor>();
                thiefView.Setting(data.viewDistance, data.viewAngle);

                // --- 生成した泥棒を管理するオブジェクトの子にする
                GameObject thiefManager = GameObject.Find("ThiefManager");
                if (thiefManager != null)
                {
                    thief.transform.parent = thiefManager.transform;
                }
                else
                {
                    GameObject parent  = GameObject.Instantiate(new GameObject("ThiefManager"));
                    thief.transform.parent = parent.transform;
                }

                //--- 生成した泥棒の生成位置を選定
            }
        }
    }
}
