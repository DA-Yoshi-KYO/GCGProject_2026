/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界検出ターゲットに関する情報
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 
 */
using UnityEngine;

// 視界検出ターゲットに関する情報
public class VisionTargetMemory
{
    [Tooltip("探索済みかどうか")]
    public bool isExplored;
    [Tooltip("探索済みとする距離")]
    public float exploredDistanceThreshold;
    [Tooltip("探索したときに得られる探索度")]
    public int explorationValue;
    [Tooltip("探索したときに得られる危険度")]
    public int dangerValue;

    public void FirstSetup()
    {
        isExplored = false;
        exploredDistanceThreshold = 2.0f;
        explorationValue = 10;
        dangerValue = 5;
    }
}
