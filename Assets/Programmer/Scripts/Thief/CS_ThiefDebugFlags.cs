/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のデバッグ用フラグを管理するクラス
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-26 | 初回作成 
 *　
 */
using UnityEngine;

// ランタイムで参照するデバッグ用フラグ（Editorから書き換えられる）
// ※実行時の挙動を切り替えるために通常クラスとして配置します。
public static class CS_ThiefDebugFlags
{
    [Tooltip("プレイヤーを捕まえるかどうか")]
    public static bool CatchPlayer = true;

    [Tooltip("プレイヤーを追跡するかどうか")]
    public static bool ChasePlayer = true;

    [Tooltip("ダメージを受けた後の無敵時間を設定するかどうか")]
    public static bool EnableInvincibilityAfterDamage = true;
}
