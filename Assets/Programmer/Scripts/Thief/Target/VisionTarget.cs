/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界検出ターゲットクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 
 */
using UnityEngine;

// 視界に入る対象を示すクラス
public class VisionTarget : ThiefTarget
{
    // ターゲットの種類
    public enum TargetType
    {
        [Tooltip("宝物")]
        Treasure,
        [Tooltip("宝物以外の部屋オブジェクト")]
        RoomObject,
    }

    [Tooltip("ターゲットの種類")]
    public TargetType targetType;

    [Header("探索したときに得る探索度")]
    [Tooltip("探索したときに得る探索度")]
    public int explorationValue;

    [Header("探索済みとする距離")]
    [Tooltip("探索済みとする距離")]
    public float exploredDistanceThreshold;
}
