/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界検出ターゲットクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 2026-05-07 | 探索度を記憶システムに移動
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

    [SerializeField, Header("ギズモを表示")]
    private bool showGizmos = false;
    [SerializeField, Header("ギズモの色")]
    private Color gizmoColor = Color.yellow;

    [SerializeField, Header("このオブジェクトを探索している敵")]
    public GameObject searchThief;

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 探索済みとする距離をギズモで表示
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, exploredDistanceThreshold);
    }
}
