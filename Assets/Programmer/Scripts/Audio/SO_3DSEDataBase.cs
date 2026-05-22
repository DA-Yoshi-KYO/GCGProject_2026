/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    3DのSEデータ
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-18 | 初回作成
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SE3DData
{
    [Header("再生する場面")] public string situation;
    [Header("再生音源")] public AudioClip audioClip;
    [Header("AudioMixcerGroupのコンポーネント")] public AudioMixerGroup[] audioMixerGroup;
    [Header("スナップショットのコンポーネント")] public AudioMixerSnapshot[] audioMixerSnapshot;
}

[CreateAssetMenu(fileName = "SE3DDataSO", menuName = "ScriptableObjects/SE3DDataSO")]
public class SO_3DSEDataBase : ScriptableObject
{
    public SE3DData[] seDatas;
    public Dictionary<string , SE3DData> seData;
    private void OnEnable()
    {
        seData = new Dictionary<string, SE3DData>();
        foreach(var item in seDatas)
        {
            seData[item.situation] = item;
        }
    }
}
