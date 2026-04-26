/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-19 | 泥棒のパラメーター設定処理の記載(行動AIの設定、視界システムの設定)
 * 2026-04-23 | 移動速度の設定処理の記載(プレイヤーの速度を仮で用意して、そこから泥棒の速度を計算するように変更)
 * 2026-04-26 | ファイル名・クラス名をThiefManagerに変更
 * 
 */
using Unity.Mathematics;
using UnityEngine;


// 泥棒を生成するシステム
public class ThiefManager : MonoBehaviour
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
        int currentWave = GameObject.Find("ThiefManager").GetComponent<WaveManager>().waveNumber;


        /*仮で実数変数として指定*/int stageNumber = 1;

        // 現在のウェーブ数に応じた
        StageDataSO stageData = ScriptableObject.Instantiate(stageDataDB);
        WaveData.ThiefData[] thiefDatas = stageData.stageData[stageNumber - 1].waveDatas[currentWave - 1].thiefDataArray;

        // 泥棒のデータをもとに泥棒を生成
        foreach (var thiefData in thiefDatas)
        {
            // 泥棒のタイプに応じたデータを取得
            ThiefData data = new ThiefData();

            // 泥棒のデータベースから、泥棒のタイプに応じたデータを取得
            for (int i = 0 ; i < thiefDB.thiefData.Length ; i++)
            {
                if(thiefDB.thiefData[i].typeName == thiefData.type)
                {
                    data = thiefDB.thiefData[i];
                    break;
                }
            }


            // 生成する泥棒の親オブジェクトを取得、存在しない場合は生成
            GameObject thiefParent = GameObject.Find("ThiefParent");
            if (thiefParent == null)
            {
                thiefParent = new GameObject("ThiefParent");
            }


            //泥棒の生成
            for (int i = 0 ; i < thiefData.count ; i++)
            {
                GameObject thief = GameObject.Instantiate(thiefPrefab);
                //--- 泥棒のデータを設定

                /* 仮で実数変数でプレイヤー速度を用意 */
                float playerSpeed = 10.0f;

                // 行動AIの設定
                ThiefAI thiefAI = thief.GetComponent<ThiefAI>();
                thiefAI.Setting(data, playerSpeed, FindObjectOfType<RoomNode>());

                // 視界システムの設定
                VisionSensor thiefView = thief.GetComponent<VisionSensor>();
                thiefView.Setting(data.viewDistance, data.viewAngle);

                // --- 泥棒をthiefParentの子オブジェクトに設定
                thief.transform.parent = thiefParent.transform;
                
                //--- 生成した泥棒の生成位置を選定

                GameObject debugPoint = GameObject.Find("Debug_ThiefPoint");
                if (debugPoint != null)
                {
                    // デバッグ用の生成ポイントが存在する場合は、そこに生成
                    thief.transform.position = debugPoint.transform.position;
                    continue;
                }

                // (仮) 50,0,50 ~ -50,0,-50の範囲にランダムに生成
                float x = UnityEngine.Random.Range(-50.0f, 50.0f);
                float z = UnityEngine.Random.Range(-50.0f, 50.0f);
                thief.transform.position = new Vector3(x, 0, z);
            }
        }
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
