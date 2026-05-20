/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の定数値管理用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-23 | 仕様書の内容に合わせて項目を追加
 * 2026-05-07 | CS_RoomEnemyEntryPointDataを用いた項目の追加
 *            | このスクリプタブルオブジェクトは、種類感共通の定数値を管理するためのものに変更 
 * 
 */
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

// ScriptableObjectとして定義することで、Unityエディタ上でデータを管理できるようにする
[CreateAssetMenu(fileName = "ThiefCommonStatusData", menuName = "ScriptableObjects/ThiefCommonStatusData", order = 1)]
public class CSS_ThiefCommonStatusData : ScriptableObject
{
    [Tooltip("ジャンプ可能な高さ(マス目)"), Min(0)]
    public int jumpHeight;

    [Header("警戒時間(秒)")]
    [Tooltip("泥棒が警戒状態の継続時間"), Min(0)]
    public int alertTime;

    [Header("攻撃命中後の気絶時間(秒)")]
    [Tooltip("泥棒が攻撃を受けた後の気絶時間"),Min(0)]
    public int stunTime;

    [Header("攻撃命中後の無敵時間(秒)")]
    [Tooltip("泥棒が攻撃を受けた後の無敵時間"),Min(0)]
    public int invincibleTime;

    [Header("気絶した後に退場するまでの間隔(秒)")]
    [Tooltip("泥棒が気絶した後に退場するまでの間隔"), Min(0)]
    public int exitAfterStunTime;

    [Header("気絶した後のフェード時間(秒)")]
    [Tooltip("泥棒が気絶した後のフェード時間"), Min(0)]
    public int fadeAfterStunTime;

    [Header("泥棒のリアクションスプライトリスト")]
    [Tooltip("泥棒のリアクションスプライトリスト")]
    public List<Sprite> reactionSprites;

    [Header("泥棒のリアクションUI用スプライトリスト")]
    [Tooltip("泥棒のリアクションUI用スプライトリスト")]
    public List<Sprite> reactionUISprites;
}
