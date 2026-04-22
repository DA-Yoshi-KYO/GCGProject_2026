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
public class BGMData
{
    public string cueSheet;
    public string cueName;
    public float volume;
}

[CreateAssetMenu(fileName = "BGMDataSO", menuName = "ScriptableObjects/BGMDataSO")]
public class BGMDataBase : ScriptableObject
{
    public BGMData[] bgmDatas;
}
