/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    部屋に関する記憶を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

// 部屋に関する記憶を管理するシステム
public class RoomMemory
{
    [Tooltip("部屋の探索度"), Range(0, 100)]
    public int explorationLevel;
    [Tooltip("部屋の危険度"), Range(0, 100)]
    public int dangerLevel;
    [Tooltip("認識したオブジェクトの情報リスト")]
    public List<ThiefTarget> recognizedObjects;

    public void FirstSetting()
    {
        explorationLevel = 0;
        dangerLevel = 0;
        recognizedObjects = new List<ThiefTarget>();
    }
}
