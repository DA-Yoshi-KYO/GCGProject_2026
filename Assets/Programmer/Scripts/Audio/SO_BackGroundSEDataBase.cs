/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    SEデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-17 | 初回作成
 * 2026-05-20 | 音源の追加
 */

using System;
using UnityEngine;

public enum BackGrondSECueName
{
    Cusor,
    Decision,
    Cancel,
    WinJingle,
    LoseJingle,
};

[Serializable]
public class BackGroundSEData
{
    [Header("再生する場面")] public string situation;//再生するシーン
    [Header("再生音源")] public BackGrondSECueName cueName;//キュー
}

[CreateAssetMenu(fileName = "BackGroundSEDataSO", menuName = "ScriptableObjects/BackGroundSEDataSO")]
public class SO_BackGroundSEDataBase : ScriptableObject
{
    public BackGroundSEData[] seDatas;
}
