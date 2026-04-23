/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    BGMデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-22 | 初回作成
 * 
 */

using System;
using UnityEngine;

[Serializable]
public enum SceneName
{
    CriWare,
    Shading,
    CreateRoom,
    CreateThiefScene,
    MainScene,
    BootStrap,
};
public enum CueSheetName
{
    WorkingTitle,
};
public enum CueName
{
    CatMeow,
};

[Serializable]
public class BGMData
{
    public SceneName sceneName;//再生するシーン
    public CueSheetName cueSheet;//キューシート
    public CueName cueName;//キュー
    public float volume;//音量
}

[CreateAssetMenu(fileName = "BGMDataSO", menuName = "ScriptableObjects/BGMDataSO")]
public class BGMDataBase : ScriptableObject
{
    public BGMData[] bgmDatas;
}
