/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    BGMデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-22 | 初回作成
 * 2026-05-20 | 音源の追加
 */

using System;
using System.Collections.Generic;
using UnityEngine;

//シーンの名前
public enum SceneName
{
    TitleScene,
    OpenningMovieScene,
    StageSelectScene,
    MainScene,//MainSceneで切り替えて流すBGM
    ThiefEscape,//MainSceneで切り替えて流すBGM
}

//バックグランド用のBGMのキューの名前
public enum BackGroundBGMCueName
{
    NoneBGM,
    TitleBGM,
    SelectBGM,
    ThiefEscapeBGM,
    InGameBGM,
};

//ステータス
public enum BGMStatus
{
    NONE,
    InGame,
};


[Serializable]
public class BackGroundBGMData
{
    [Header("再生するシーン名")] public SceneName sceneName;//再生するシーン
    [Header("再生音源")] public BackGroundBGMCueName cueName;//キュー
    [Header("ステータス")]public BGMStatus status;//ステータス
}

[CreateAssetMenu(fileName = "BackGroundBGMDataSO", menuName = "ScriptableObjects/BackGroundBGMDataSO")]
public class SO_BackGroundBGMDataBase : ScriptableObject
{
    public BackGroundBGMData[] bgmDatas;
    public Dictionary<string, BackGroundBGMData> bgmData;
    private void OnEnable()
    {
        bgmData = new Dictionary<string, BackGroundBGMData>();
        foreach (var item in bgmDatas)
        {
            bgmData[item.sceneName.ToString()] = item;
        }
    }
}
