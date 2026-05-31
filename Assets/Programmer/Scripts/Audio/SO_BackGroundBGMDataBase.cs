/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    BGMデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-22 | 初回作成
 * 2026-05-20 | 音源の追加
 */

using System;
using UnityEngine;

//シーンの名前
public enum SceneName
{
    TitleScene,
    StageSelectScene,
    MainScene,//MainSceneで切り替えて流すBGM
    ThiefEscape,//MainSceneで切り替えて流すBGM
}

//バックグランド用のBGMのキューの名前
public enum BackGroundBGMCueName
{
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
}
