/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の定数値管理用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-23 | 仕様書の内容に合わせて項目を追加
 * 
 */
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

// 泥棒の定数値管理用クラス
[Serializable]
public class ThiefTypeData
{
    [Header("種類名")]
    [Tooltip("泥棒の種類名")]
    public string typeName;

    [Header("耐久値")]
    [Tooltip("泥棒の耐久値"), Min(1)]
    public int durability;

    [Header("泥棒が探索するのにかかる秒数")]
    [Tooltip("泥棒が探索するのにかかる秒数"), Min(0)]
    public int searchTime;

    [Header("探索に関する項目")]
    [Tooltip("泥棒の歩き速度倍率"), Range(0.0f,1.0f)]
    public float walkSpeedMultiplier;
    [Tooltip("泥棒の走り速度倍率"), Range(0.0f, 1.0f)]
    public float runSpeedMultiplier;

    [Header("走り状態になる標的オブジェクトタイプ")]
    [Tooltip("泥棒が走り状態になる標的オブジェクトのタイプ")]
    public List<VisionTarget.TargetType> runTargetTypes;

    [Header("ソウルのドロップ数")]
    [Tooltip("泥棒が倒されたときにドロップするソウルの数"), Min(0)]
    public int soulDropCount;

    [Header("視界に関する項目")]
    [Tooltip("泥棒の視界の半径"), Min(0)]
    public float viewDistance;
    [Tooltip("泥棒の視界の角度"), Range(0, 360)]
    public int viewAngle;

    [Header("次の部屋探索に切り替える探索度")]
    [Tooltip("次の部屋探索に切り替える探索度の閾値"), Range(0, 100)]
    public int nextRoomSearchThreshold;

    [Header("泥棒のリアクションスプライトリスト")]
    [Tooltip("泥棒のリアクションスプライトリスト")]
    public List<Sprite> reactionSprites;
}

// 泥棒の種類間で共通の定数値を管理するクラス
[Serializable]
public class ThiefData
{
    [Tooltip("ジャンプ可能な高さ(マス目)"), Min(0)]
    public int jumpHeight;

    [Header("警戒時間(秒)")]
    [Tooltip("泥棒が警戒状態の継続時間"), Min(0)]
    public int alertTime;

    [Header("気絶した後に退場するまでの間隔(秒)")]
    [Tooltip("泥棒が気絶した後に退場するまでの間隔"), Min(0)]
    public int exitAfterStunTime;
}


// ScriptableObjectとして定義することで、Unityエディタ上でデータを管理できるようにする
[CreateAssetMenu(fileName = "ThiefDataSO", menuName = "ScriptableObjects/ThiefDataSO", order = 1)]
public class ThiefDataSO : ScriptableObject
{
    [Header("泥棒の種類間で共通のデータ")]
    public ThiefData commonData;

    // 泥棒のデータ
    [Header("泥棒の種類ごとのデータ")]
    public ThiefTypeData[] thiefData;
}
