using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CSS_ThiefStatusData.cs
 *  制作者      : 吉本竜
 *  内容        : 盗賊のステータスデータを管理するScriptableObject
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// 盗賊の種類別ステータスデータを管理するScriptableObjectです。
/// </summary>
[CreateAssetMenu(
    fileName = "ThiefStatusData",
    menuName = "ScriptableObjects/ThiefStatusData")]
public class CSS_ThiefStatusData : ScriptableObject
{
    [Header("耐久値")]
    [Tooltip("泥棒の耐久値"), Min(1)]
    public int durability;

    [Header("泥棒が探索するのにかかる秒数")]
    [Tooltip("泥棒が探索するのにかかる秒数"), Min(0)]
    public int searchTime;

    [Header("探索に関する項目")]
    [Tooltip("泥棒の歩き速度倍率"), Range(0.0f, 1.0f)]
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
