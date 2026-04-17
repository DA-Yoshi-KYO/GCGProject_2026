/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の定数値管理用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 
 */
using System;
using UnityEngine;

// 泥棒の定数値管理用クラス
[Serializable]
public class ThiefData
{
    [Header("種類名")]
    [Tooltip("泥棒の種類名")]
    public string typeName;

    [Header("次の部屋探索に切り替える探索度の閾値")]
    [Tooltip("次の部屋探索に切り替える探索度の閾値"), Range(0, 100)]
    public int nextRoomSearchThreshold;

    [Header("耐久値")]
    public int durability;

    [Header("移動速度")]
    public float speed;
}

// ScriptableObjectとして定義することで、Unityエディタ上でデータを管理できるようにする
[CreateAssetMenu(fileName = "ThiefDataSO", menuName = "ScriptableObjects/ThiefDataSO", order = 1)]
public class ThiefDataSO : ScriptableObject
{
    // 泥棒のデータ
    public ThiefData[] thiefData; 
}
