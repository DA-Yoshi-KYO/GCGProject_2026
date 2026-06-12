/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の反応の種類を定義する列挙型
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-12 | 初回作成
 * 
 */
using UnityEngine;

[Tooltip("泥棒の反応の種類の列挙型")]
public enum CSE_ThiefReactionType
{
    [Tooltip("ネコを追跡中")]
    ChasingCat,
        [Tooltip("ギミックに直接被弾")]
    HitTrap,
        [Tooltip("ギミックが間近で被弾")]
    NearHitTrap,
        [Tooltip("警戒")]
    Alert,
        [Tooltip("お宝を見つける・お宝を運ぶ")]
    FoundTreasure,
        [Tooltip("物を探索")]
    Searching,
}
