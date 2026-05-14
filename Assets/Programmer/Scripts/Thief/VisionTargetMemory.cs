/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界検出ターゲットに関する記憶を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-07 | 初回作成
 * 
 */

using System;
using UnityEngine;

[Serializable]
public class VisionTargetMemory
{
    [Header("探索進行度"), Tooltip("このターゲットの探索進行度")]
    public float explorationProgress = 0.0f;

    [Header("探索済みかどうか")]
    [Tooltip("探索済みかどうか")]
    public bool isExplored;
}
