/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    カットシーンビデオのデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-03 | 初回作成
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class CutSceneData
{
    [Header("再生する場面")] public string situation;
    [Header("再生する動画")] public VideoClip videoClip;
    [HideInInspector] public bool played;//再生したかどうか
}

[CreateAssetMenu(fileName = "CutSceneVideoDataSO", menuName = "ScriptableObjects/CutSceneVideoDataSO")]
public class SO_CutSceneVideo : ScriptableObject
{
    public CutSceneData[] cutSceneDatas;

    public Dictionary<string, CutSceneData> cutSceneData;
    private void OnEnable()
    {
        cutSceneData = new Dictionary<string, CutSceneData>();
        foreach (var item in cutSceneDatas)
        {
            cutSceneData[item.situation] = item;
        }
    }
}
