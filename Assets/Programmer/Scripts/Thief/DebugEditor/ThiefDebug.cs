/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒デバッグツール本体
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 *　2026-05-19 | 初回作成
 *　2026-06-01 | タブ切り替えのUIをToolbarに変更、スクロール対応追加
 *　
 */
using UnityEditor;
using UnityEngine;

/// <summary>
/// 泥棒に関するデバッグ機能をタブでまとめたウィンドウ。
/// </summary>
public sealed class ThiefDebug : EditorWindow
{
    /// <summary>
    /// 表示するタブ種別
    /// </summary>
    private enum Tab
    {
        /// <summary>ダメージデバッグ</summary>
        Damage,
        /// <summary>部屋記憶（roomMemories）デバッグ</summary>
        RoomMemory,
    }

    // 現在表示しているタブ
    private Tab currentTab = Tab.Damage;

    // タブ内容用のスクロール位置
    private Vector2 scroll;

    // 各タブの描画クラス
    private ThiefDebugDamageTab damageTab;
    private ThiefDebugRoomMemoryTab roomMemoryTab;

    /// <summary>
    /// Unityメニューからデバッグウィンドウを開く
    /// </summary>
    [MenuItem("Tools/Debug/Thief Debug")]
    public static void Open()
    {
        //既存があれば再利用、無ければ生成して表示
        GetWindow<ThiefDebug>("ThiefDebug");
    }

    /// <summary>
    /// Script Reload / Window復帰時の初期化
    /// </summary>
    private void OnEnable()
    {
        // Script Reload後も保持される必要があるものは、ここで生成
        if (damageTab == null) damageTab = new ThiefDebugDamageTab();
        if (roomMemoryTab == null) roomMemoryTab = new ThiefDebugRoomMemoryTab();
    }

    /// <summary>
    /// ウィンドウ描画（毎フレーム呼ばれる）
    /// </summary>
    private void OnGUI()
    {
        // タブ切り替えUI
        DrawToolbar();

        // タブ内容領域（内容が増えた場合に備えてスクロール対応）
        using (var sv = new EditorGUILayout.ScrollViewScope(scroll))
        {
            scroll = sv.scrollPosition;

            // 現在のタブに応じて描画処理を委譲
            switch (currentTab)
            {
                case Tab.Damage:
                    damageTab.Draw();
                    break;
                case Tab.RoomMemory:
                    roomMemoryTab.Draw();
                    break;
            }
        }
    }

    /// <summary>
    /// タブ選択用のツールバー（上部）
    /// </summary>
    private void DrawToolbar()
    {
        EditorGUILayout.Space(4);
        currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new[] { "ダメージ", "ルーム記憶" });
        EditorGUILayout.Space(8);
    }
}
