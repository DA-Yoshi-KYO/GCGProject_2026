/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    BGMデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-22 | 初回作成
 */

using System;
using UnityEngine;

//シーンの名前
public enum SceneName
{
    TitleScene,
    StageSelectScene,
}

//バックグランド用のBGMのキューの名前
public enum BackGround_BGM_CueName
{
    InGameBGM,
    SelectBGM,
    TitleBGM,
};

[Serializable]
public class BackGround_BGM_Data
{
    [Header("再生するシーン名")] public SceneName sceneName;//再生するシーン
    [Header("再生音源")] public BackGround_BGM_CueName cueName;//キュー
    [Header("音量")] public float volume;//音量
}

[CreateAssetMenu(fileName = "BackGround_BGM_DataSO", menuName = "ScriptableObjects/BackGround_BGM_DataSO")]
public class SO_BackGround_BGM_DataBase : ScriptableObject
{
    public BackGround_BGM_Data[] bgmDatas;
}
