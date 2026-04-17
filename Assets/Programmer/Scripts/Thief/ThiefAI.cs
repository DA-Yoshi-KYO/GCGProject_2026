/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のAIシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 
 */
using UnityEngine;

// 泥棒のAIシステム
public class ThiefAI : MonoBehaviour
{
    private enum ThiefState
    {
        Explore,      // 探索状態
        Found,        // 発見状態
        Escape,       // 逃走状態
    }

    [SerializeField,Header("現在の状態")]
    private ThiefState currentState;

    private int durability; // 耐久力(体力)
    private float speed;    // 移動速度

    private void Start()
    {
        // 初期状態を探索に設定
        currentState = ThiefState.Explore;

        // 初期耐久力
        durability = 4;

        // 初期移動速度
        speed = 5f;
    }
}

