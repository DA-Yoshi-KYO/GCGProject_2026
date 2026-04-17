/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の定数値管理用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 
 */

// 泥棒の定数値管理用クラス
using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ThiefData
{
    [Header("このパラメーターを使用するウェーブ番号の範囲")]
    [Tooltip("このパラメーターを使用するウェーブ番号の最小値"), Min(1)]
    public int minWaveNumber;
    [Tooltip("このパラメーターを使用するウェーブ番号の最大値"), Min(1)]
    public int maxWaveNumber;

    [Header("耐久値の範囲")]
    [Tooltip("耐久値の最小値"), Min(1)]
    public int minDurability;
    [Tooltip("耐久値の最大値"), Min(1)]
    public int maxDurability;

    [Header("移動速度の範囲")]
    [Tooltip("移動速度の最小値"), Min(0.0f)]
    public float minSpeed;
    [Tooltip("移動速度の最大値"), Min(0.0f)]
    public float maxSpeed;
}

// ScriptableObjectとして定義することで、Unityエディタ上でデータを管理できるようにする
[CreateAssetMenu(fileName = "ThiefDataSO", menuName = "ScriptableObjects/ThiefDataSO", order = 1)]
public class ThiefDataSO : ScriptableObject
{
    // 泥棒のデータ
    public ThiefData[] thiefData; 
}
