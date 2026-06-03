/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    カットシーンビデオのデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-03 | 初回作成
 */
using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class CutSceneData
{
    [Header("再生する場面")] public string situation;
    [Header("再生する動画")] public VideoClip videoClip;
    [Header("再生したかどうか")] public bool played;
}

[CreateAssetMenu(fileName = "CutSceneVideoDataSO", menuName = "ScriptableObjects/CutSceneVideoDataSO")]
public class SO_CutSceneVideo : ScriptableObject
{
    public CutSceneData[] cutSceneDatas;
}
