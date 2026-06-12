/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の行動状態を定義する列挙型
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-12 | 初回作成
 * 
 */
using UnityEngine;

[Tooltip("泥棒の行動状態を定義する列挙型")]
public enum CSE_ThiefState
{
    [Tooltip("探索状態")]
    Explore,
    [Tooltip("発見状態")]
    Found,
    [Tooltip("逃走状態")]
    Escape,
    [Tooltip("気絶状態")]
    Stunned

}
