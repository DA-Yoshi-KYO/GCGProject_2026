/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    部屋に関する記憶を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 2026-05-22 | ファイル名を変更（CS_RoomMemory.cs → CS_RoomMemory.cs）
 *            | クラス名を変更（RoomMemory → CS_RoomMemory）
 * 
 */
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 部屋に関する記憶を管理するクラス
/// </summary>
[Serializable]
public class CS_RoomMemory
{
    [Tooltip("部屋の探索度"), Range(0, 100)]
    public int explorationLevel;
    [Tooltip("認識したオブジェクトの情報リスト")]
    public List<CS_ThiefTarget> recognizedObjects;
    [Tooltip("選ばなかったドアの方向情報")]
    public List<CSE_RoomDoorDirection> unchosenDoors;
    [Tooltip("入ってきたドアの方向情報")]
    public CSE_RoomDoorDirection enteredDoorDirection;

    /// <summary>
    /// 部屋の記憶を初期化するメソッド
    /// </summary>
    public void FirstSetting()
    {
        explorationLevel = 0;
        recognizedObjects = new List<CS_ThiefTarget>();
        unchosenDoors = new List<CSE_RoomDoorDirection>();
    }
}
