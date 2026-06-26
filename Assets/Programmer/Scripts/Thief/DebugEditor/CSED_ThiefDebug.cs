/*＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒デバッグツール本体
 *＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 *　2026-05-19 | 初回作成
 *　2026-05-22 | ファイル名を変更（ThiefDebug.cs → CSED_ThiefDebug.cs）
 *　           | クラス名を変更（ThiefDebug → CSED_ThiefDebug）
 *　
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 泥棒に関するデバッグ機能をタブでまとめたウィンドウ。
/// </summary>
public sealed class CSED_ThiefDebug : EditorWindow
{
    /// <summary>
    /// 表示するタブ種別
    /// </summary>
    private enum CSE_Tab
    {
        /// <summary>ダメージデバッグ</summary>
        Damage,
        /// <summary>部屋記憶（参照）</summary>
        RoomMemoryView,
        /// <summary>部屋記憶（追加）</summary>
        RoomMemoryAdd,
        /// <summary>機能フラグ</summary>
        Flags,
    }

    // 現在表示しているタブ
    private CSE_Tab currentTab = CSE_Tab.Damage;

    // タブ内容用のスクロール位置
    private Vector2 scroll;

    // 各タブの描画クラス
    private CSED_ThiefDebugDamageTab damageTab;
    private CSED_ThiefDebugRoomMemoryTab roomMemoryTab;
    private CSED_ThiefDebugRoomMemoryAddTab roomMemoryAddTab;
    private CSED_ThiefDebugFlagTab flagTab;

    /// <summary>
    /// Unityメニューからデバッグウィンドウを開く
    /// </summary>
    [MenuItem("Tools/Debug/Thief Debug")]
    public static void Open()
    {
        //既存があれば再利用、無ければ生成して表示
        GetWindow<CSED_ThiefDebug>("ThiefDebug");
    }

    /// <summary>
    /// Script Reload / Window復帰時の初期化
    /// </summary>
    private void OnEnable()
    {
        // Script Reload後も保持される必要があるものは、ここで生成
        if (damageTab == null) damageTab = new CSED_ThiefDebugDamageTab();
        if (roomMemoryTab == null) roomMemoryTab = new CSED_ThiefDebugRoomMemoryTab();
        if (roomMemoryAddTab == null) roomMemoryAddTab = new CSED_ThiefDebugRoomMemoryAddTab();
        if (flagTab == null) flagTab = new CSED_ThiefDebugFlagTab();
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
                case CSE_Tab.Damage:
                    damageTab.Draw();
                    break;
                case CSE_Tab.RoomMemoryView:
                    roomMemoryTab.Draw();
                    break;
                case CSE_Tab.RoomMemoryAdd:
                    roomMemoryAddTab.Draw();
                    break;
                case CSE_Tab.Flags:
                    flagTab.Draw();
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
        currentTab = (CSE_Tab)GUILayout.Toolbar((int)currentTab, new[] { "ダメージ", "記憶参照", "記憶追加", "フラグ" });
        EditorGUILayout.Space(8);
    }
}
#endif
