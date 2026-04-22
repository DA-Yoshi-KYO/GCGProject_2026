/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒を生成するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-19 | 泥棒のパラメーター設定処理の記載(行動AIの設定、視界システムの設定)
 * 
 */
using UnityEngine;


// 泥棒を生成するシステム
public class ThiefGenerator : MonoBehaviour
{
    [SerializeField, Tooltip("泥棒のデータベース")]
    private ThiefDataSO thiefDB;
    [SerializeField, Tooltip("ステージごとのウェーブデータのデータベース")]
    private StageDataSO stageDataDB;
    [SerializeField, Tooltip("泥棒のプレハブ")]
    private GameObject thiefPrefab;

    private void Start()
    {
        Notify();
    }

    // 泥棒を生成するメソッド
    private void Notify()
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
                // (仮) 50,0,50 ~ -50,0,-50の範囲にランダムに生成
                float x = Random.Range(-50.0f, 50.0f);
                float z = Random.Range(-50.0f, 50.0f);
                thief.transform.position = new Vector3(x, 0, z);
            }
        }
    }
}
