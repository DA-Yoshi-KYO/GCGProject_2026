/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界検出ターゲットに関する記憶を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-07 | 初回作成
 * 2026-05-22 | ファイル名を変更（CS_VisionTargetMemory.cs → CS_VisionTargetMemory.cs）
 *            | クラス名を変更（CS_VisionTargetMemory → CS_VisionTargetMemory）
 * 
 */

using System;
using UnityEngine;

/// <summary>
/// 視界検出ターゲットに関する記憶を管理するシステム
/// </summary>
[Serializable]
public class CS_VisionTargetMemory
{
    [Header("探索進行度"), Tooltip("このターゲットの探索進行度")]
    public float explorationProgress = 0.0f;

    [Header("探索済みかどうか")]
    [Tooltip("探索済みかどうか")]
    public bool isExplored;

    [Header("探索している人")]
    [Tooltip("探索している人")]
    public GameObject searchThief;
}
