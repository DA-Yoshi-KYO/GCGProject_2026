/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の種類別定数値管理用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗 | 吉本 竜
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成(ヨシモト)
 * 2026-04-23 | 仕様書の内容に合わせて項目を追加(ウルノ)
 * 2026-05-07 | CS_RoomEnemyEntryPointDataを用いた項目の追加(ウルノ)
 *            | このスクリプタブルオブジェクトは、種類感共通の定数値を管理するためのものに変更(ウルノ)
 * 2026-05-22 | ファイル名を変更（CSS_ThiefCommonStatusData.cs → SO_ThiefCommonStatusData.cs）(ウルノ)
 *            | クラス名を変更（SO_ThiefCommonStatusData → CO_ThiefCommonStatusData）(ウルノ)
 *            | ファイル名を変更（ThiefCommonStatusData → DB_ThiefCommonStatusData）(ウルノ)
 * 
 */
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 盗賊の種類別ステータスデータを管理するScriptableObjectです。
/// </summary>
[CreateAssetMenu(
    fileName = "DB_ThiefStatusData",
    menuName = "ScriptableObjects/Thief/TypeStatusData")]
public class CO_ThiefStatusData : ScriptableObject
{
    [Header("耐久値")]
    [Tooltip("泥棒の耐久値"), Min(1)]
    public int durability;

    [Header("泥棒が探索するのにかかる秒数")]
    [Header("0 : 宝物")]
    [Header("1 : 棚")]
    [Tooltip("泥棒が探索するのにかかる秒数")]
    public List<int> searchTime;

    [Header("探索に関する項目")]
    [Tooltip("泥棒の歩き速度倍率"), Range(0.0f, 3.0f)]
    public float walkSpeedMultiplier;
    [Tooltip("泥棒の走り速度倍率"), Range(0.0f, 3.0f)]
    public float runSpeedMultiplier;

    [Header("走り状態になる標的オブジェクトタイプ")]
    [Tooltip("泥棒が走り状態になる標的オブジェクトのタイプ")]
    public List<CS_VisionTarget.TargetType> runTargetTypes;

    [Header("猫を捕まえている時間")]
    [Tooltip("泥棒が猫を捕まえている時間(秒)"), Min(0)]
    public int holdCatTime;

    [Header("プレイヤーを追跡する時間(秒)")]
    [Tooltip("泥棒がプレイヤーを追跡する時間(秒)"), Min(0)]
    public int pursuitTime;

    [Header("視界に関する項目")]
    [Tooltip("泥棒の視界の半径"), Min(0)]
    public float viewDistance;
    [Tooltip("泥棒の視界の角度"), Range(0, 360)]
    public int viewAngle;

    [Header("次の部屋探索に切り替える探索度")]
    [Tooltip("次の部屋探索に切り替える探索度の閾値"), Range(0, 100)]
    public int nextRoomSearchThreshold;
}
