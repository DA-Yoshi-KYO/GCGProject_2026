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
    [Tooltip("泥棒の行動状態を定義する列挙型")]
    private enum ThiefState
    {
        [Tooltip("探索状態")]
        Explore,
        [Tooltip("発見状態")]
        Found,
        [Tooltip("逃走状態")]
        Escape,
    }

    [Tooltip("現在の行動状態")]
    private ThiefState currentState;
    [Tooltip("泥棒の耐久力")]
    private int durability;
    [Tooltip("泥棒の移動速度")]
    private float speed;

    private void Start()
    {
        // 初期状態を探索に設定
        currentState = ThiefState.Explore;

        // 初期耐久力
        durability = 4;

        // 初期移動速度
        speed = 5f;
    }

    private void Update()
    {
        // 現在の状態に応じた行動を実行
        switch (currentState)
        {
            case ThiefState.Explore:
                Explore();
                break;
            case ThiefState.Found:
                Found();
                break;
            case ThiefState.Escape:
                Escape();
                break;
        }
    }



    // 探索状態の行動
    private void Explore()
    {
        // TODO
        // 探索の移動処理
    }

    // 発見状態の行動
    private void Found()
    {
        // TODO
        // 発見後のバフ処理


        // 状態を逃走に変更
        currentState = ThiefState.Escape;
    }

    // 逃走状態の行動
    private void Escape()
    { 
    }
}

