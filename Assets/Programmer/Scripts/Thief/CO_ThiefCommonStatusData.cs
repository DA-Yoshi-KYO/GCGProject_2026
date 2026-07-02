/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の定数値管理用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-23 | 仕様書の内容に合わせて項目を追加
 * 2026-05-07 | CS_RoomEnemyEntryPointDataを用いた項目の追加
 *            | このスクリプタブルオブジェクトは、種類感共通の定数値を管理するためのものに変更
 * 2026-05-22 | ファイル名を変更（CSS_ThiefCommonStatusData.cs → SO_ThiefCommonStatusData.cs）
 *            | クラス名を変更（SO_ThiefCommonStatusData → CO_ThiefCommonStatusData）
 *            | ファイル名を変更（ThiefCommonStatusData → DB_ThiefCommonStatusData）
 * 
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 泥棒の共通ステータスデータを管理するScriptableObjectです。
/// </summary>
[CreateAssetMenu(
    fileName = "DB_ThiefCommonStatusData", 
    menuName = "ScriptableObjects/Thief/CommonStatusData")]
public class CO_ThiefCommonStatusData : ScriptableObject
{
    [Header("ゲーム開始から泥棒を生成するまでの時間(秒)"), Min(0)]
    [Tooltip("ゲーム開始から最初の泥棒を生成するまでの時間")]
    public float firstCreateInterval;

    [Header("次のウェーブの敵を生成するまでの時間(秒)"), Min(0)]
    [Tooltip("次のウェーブの敵を生成するまでの時間")]
    public float nextWaveCreateInterval;

    [Header("泥棒を生成する間隔の基本値(秒)"), Min(0)]
    [Tooltip("泥棒を生成する間隔の基本値")]
    public float createInterval;

    [Header("攻撃命中後の気絶時間(秒)")]
    [Tooltip("泥棒が攻撃を受けた後の気絶時間"),Min(0)]
    public int stunTime;

    [Header("攻撃命中後の無敵時間(秒)")]
    [Tooltip("泥棒が攻撃を受けた後の無敵時間"),Min(0)]
    public int invincibleTime;

    [Header("気絶解消後、プレイヤーを発見する猶予時間(秒)")]
    [Tooltip("泥棒が気絶から回復した後、プレイヤーを見つけることのできる猶予時間"), Min(0)]
    public int findPlayerGraceTime;

    [Header("気絶した泥棒を避ける範囲")]
    [Tooltip("泥棒が気絶したときに、他の泥棒が避ける範囲"), Min(0)]
    public int avoidStunThiefRange;

    [Header("気絶した後に退場するまでの間隔(秒)")]
    [Tooltip("泥棒が気絶した後に退場するまでの間隔"), Min(0)]
    public int exitAfterStunTime;

    [Header("気絶した後のフェード時間(秒)")]
    [Tooltip("泥棒が気絶した後のフェード時間"), Min(0)]
    public int fadeAfterStunTime;

    [Header("泥棒のリアクションUI用スプライトリスト")]
    [Tooltip("泥棒のリアクションUI用スプライトリスト")]
    public List<Sprite> reactionUISprites;

    [Header("危険地帯の残存時間(警戒の継続時間)")]
    [Tooltip("罠発動で生成される DangerZone の残存時間(秒)")]
    [Min(0)]
    public float dangerZoneDuration =5f;
}
