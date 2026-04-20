/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ウェーブ数による変化するデータを管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 
 */
using System;
using UnityEngine;


[Serializable]
public class WaveData
{
    [Serializable]
    public struct ThiefData
    {
        [Header("盗賊の種類"), Tooltip("盗賊の種類")]
        public string type;

        [Header("生成される人数")]
        [Tooltip("このウェーブで生成される盗賊の人数"), Min(1)]
        public int count;
    }
    [Header("盗賊データ"), Tooltip("ウェーブごとの盗賊データの配列")]
    public ThiefData[] thiefDataArray;
}

[CreateAssetMenu(fileName = "StageDataSO", menuName = "ScriptableObjects/StageDataSO", order = 1)]
public class StageDataSO : ScriptableObject
{
    [Serializable]
    public struct StageData
    {
        public WaveData[] waveDatas;
    }
    [Header("ステージデータ"), Tooltip("ステージごとのウェーブデータの配列")]
    public StageData[] stageData;
}
