/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒を生成するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 
 */
using UnityEditor;
using UnityEngine;


// 泥棒を生成するシステム
public class ThieGenerator
{
    [Tooltip("泥棒のデータベース")]
    private ThiefDataSO thiefDB;
    [Tooltip("ステージごとのウェーブデータのデータベース")]
    private StageDataSO stageDataDB;

    [Tooltip("使用する泥棒データベースのパス")]
    private string thiefDBPath = "Assets/Programmer/ScriptableObject/ThiefDB.asset";
    [Tooltip("使用するステージデータベースのパス")]
    private string stageDataDBPath = "Assets/Programmer/ScriptableObject/StageDB.asset";

    private void Start()
    {
        // 泥棒のデータベースをロード
        thiefDB = AssetDatabase.LoadAssetAtPath<ThiefDataSO>(thiefDBPath);
        // ステージごとのウェーブデータのデータベースをロード
        stageDataDB = AssetDatabase.LoadAssetAtPath<StageDataSO>(stageDataDBPath);
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

        }
    }
}
