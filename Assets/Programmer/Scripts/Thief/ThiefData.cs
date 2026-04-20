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
    [Tooltip("泥棒の耐久値"), Min(1)]
    public int durability;

    [Header("移動速度")]
    [Tooltip("泥棒の移動速度"), Min(0)]
    public float speed;

    [Header("視界の半径")]
    [Tooltip("泥棒の視界の半径"), Min(0)]
    public float viewDistance;
    [Header("視界の角度")]
    [Tooltip("泥棒の視界の角度"), Range(0, 360)]
    public float viewAngle;
}

// ScriptableObjectとして定義することで、Unityエディタ上でデータを管理できるようにする
[CreateAssetMenu(fileName = "ThiefDataSO", menuName = "ScriptableObjects/ThiefDataSO", order = 1)]
public class ThiefDataSO : ScriptableObject
{
    // 泥棒のデータ
    public ThiefData[] thiefData;
}
