/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の機能のオンオフデバッグタブ
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-26 | 初回作成 
 *　
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 泥棒の機能オン/オフ用フラグを編集するタブ
/// </summary>
internal sealed class CSED_ThiefDebugFlagTab
{
    // ローカル編集用（GUIの初期表示に使う）
    private bool isCatchPlayer = false;
    private bool isChasePlayer = false;
    private bool isEnableInvincibilityAfterDamage = false;

    public CSED_ThiefDebugFlagTab()
    {
        // 初期化時に現在のグローバル値を読み込む
        isCatchPlayer = CS_ThiefDebugFlags.CatchPlayer;
        isChasePlayer = CS_ThiefDebugFlags.ChasePlayer;
        isEnableInvincibilityAfterDamage = CS_ThiefDebugFlags.EnableInvincibilityAfterDamage;
    }

    /// <summary>
    /// タブ全体の描画
    /// </summary>
    public void Draw()
    {
        EditorGUILayout.LabelField("機能フラグ", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "以下のフラグをオフにすると、該当機能が無効化されます。\n" +
            "例：『プレイヤーを捕まえる』をオフにすると、泥棒はプレイヤーを捕まえなくなります。",
            MessageType.Info);

        EditorGUILayout.Space(6);

        // フラグ群（必要に応じてここに増やす）
        isCatchPlayer = EditorGUILayout.ToggleLeft("プレイヤーを捕まえる", isCatchPlayer);
        isChasePlayer = EditorGUILayout.ToggleLeft("プレイヤーを追跡する", isChasePlayer);
        isEnableInvincibilityAfterDamage = EditorGUILayout.ToggleLeft("ダメージ後の無敵時間を有効にする", isEnableInvincibilityAfterDamage);

        EditorGUILayout.Space(8);

        // 操作ボタン群
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("グローバルに適用"))
            {
                ApplyToGlobal();
            }

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("実行中のシーンに即時反映"))
                {
                    ApplyToGlobal();
                    ApplyImmediateToRunningThieves();
                }
            }
        }

        EditorGUILayout.Space(6);

        // 現在のランタイム値の表示（読み取り専用、デバッグ用）
        EditorGUILayout.LabelField("現在のランタイム値", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("CatchPlayer = " + CS_ThiefDebugFlags.CatchPlayer.ToString());
        EditorGUILayout.LabelField("ChasePlayer = " + CS_ThiefDebugFlags.ChasePlayer.ToString());
        EditorGUILayout.LabelField("EnableInvincibilityAfterDamage = " + CS_ThiefDebugFlags.EnableInvincibilityAfterDamage.ToString());
    }

    /// <summary>
    /// グローバルフラグに反映（ランタイムで参照する静的フィールドに書き込む）
    /// </summary>
    private void ApplyToGlobal()
    {
        CS_ThiefDebugFlags.CatchPlayer = isCatchPlayer;
        CS_ThiefDebugFlags.ChasePlayer = isChasePlayer;
        CS_ThiefDebugFlags.EnableInvincibilityAfterDamage = isEnableInvincibilityAfterDamage;
        Debug.Log("[ThiefDebug] フラグを適用しました: CatchPlayer = " + isCatchPlayer + ", ChasePlayer = " + isChasePlayer + ", EnableInvincibilityAfterDamage = " + isEnableInvincibilityAfterDamage);
    }

    /// <summary>
    /// 実行中の泥棒AIに対して即時の影響（プレイヤー捕獲を無効にしたら現在捕獲対象になっているプレイヤーを解除）を行う
    /// </summary>
    private void ApplyImmediateToRunningThieves()
    {
        if (!Application.isPlaying) return;

        // 全ての CS_ThiefAI を取得して、必要ならターゲットをクリアする
        var allThieves = GameObject.FindObjectsOfType<CS_ThiefAI>();
        int cleared = 0;
        foreach (var t in allThieves)
        {
            var mem = t.read_MemorySystem;
            if (mem == null) continue;

            // プレイヤー捕獲をオフにした場合、現在ターゲットがプレイヤーであれば解除する
            if (!isCatchPlayer)
            {
                if (mem.read_CurrentTarget is CS_PlayerTarget)
                {
                    mem.ClearTarget();
                    cleared++;
                }
            }

            // 追跡をオフにした場合、現在プレイヤーをターゲットにしている泥棒のターゲットを解除
            if (!isChasePlayer)
            {
                if (mem.read_CurrentTarget is CS_PlayerTarget)
                {
                    mem.ClearTarget();
                    cleared++;
                }
            }
        }

        Debug.Log($"[ThiefDebug] 実行中の泥棒に即時反映しました。解除したターゲット数: {cleared}");
    }
}
#endif
