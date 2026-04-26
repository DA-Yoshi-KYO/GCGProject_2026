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

public enum CueName
{
    Cat,
};

[Serializable]
public class BackGround_BGM_Data
{
    [Header("再生するシーン名")] public SceneName sceneName;//再生するシーン
    [Header("再生音源")] public CueName cueName;//キュー
    [Header("音量")] public float volume;//音量
}

[CreateAssetMenu(fileName = "BackGround_BGM_DataSO", menuName = "ScriptableObjects/BackGround_BGM_DataSO")]
public class BackGround_BGM_DataBase : ScriptableObject
{
    public BackGround_BGM_Data[] bgmDatas;
}
