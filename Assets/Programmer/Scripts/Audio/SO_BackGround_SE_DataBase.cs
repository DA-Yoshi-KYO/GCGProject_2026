/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    SEデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-17 | 初回作成
 */

using System;
using UnityEngine;

public enum BackGrond_SE_CueName
{
    Cusor,
    Decision,
};

[Serializable]
public class BackGround_SE_Data
{
    [Header("再生する場面")] public string situation;//再生するシーン
    [Header("再生音源")] public BackGrond_SE_CueName cueName;//キュー
    [Header("音量")] public float volume;//音量
}

[CreateAssetMenu(fileName = "BackGround_SE_DataSO", menuName = "ScriptableObjects/BackGround_SE_DataSO")]
public class SO_BackGround_SE_DataBase : ScriptableObject
{
    public BackGround_SE_Data[] seDatas;
}
