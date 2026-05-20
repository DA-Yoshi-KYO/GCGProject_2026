/*
+=====================================
 ファイル名 : CSED_CreateTools.cs
 概要     : SCreateToolsツールのEditorWindowクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/22 構成整理
            2026/05/19 Generated Editorsウィンドウを同時に開く処理を追加
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ツール用のEditorWindow雛形（IMGUI）
/// </summary>
public partial class CSED_CreateTools : EditorWindow
{
    /// <summary>
    /// メニューからCreateTools一式を開きます。
    /// </summary>
    [MenuItem("Tools/CreateTools")]
    public static void ShowWindow()
    {
        CSED_CreateTools createToolsWindow =
            GetWindow<CSED_CreateTools>("CreateTools");

        createToolsWindow.position = new Rect(
            100.0f,
            100.0f,
            c_WindowInitWidth,
            c_WindowInitHeight);

        createToolsWindow.minSize = new Vector2(
            c_WindowMinWidth,
            c_WindowMinHeight);

        CSED_CreateTools_GeneratedEditorsWindow.OpenWindow();

        createToolsWindow.Focus();
    }

    /// <summary>
    /// GUIを描画する（IMGUI）。
    /// </summary>
    private void OnGUI()
    {
        InitializeHorizontalSplit();
        InitializeVerticalSplit();

        HandleHorizontalSplitDrag(Event.current);
        HandleVerticalSplitDrag(Event.current);

        DrawLeftWindowView();
        DrawCenterWindowView();
        DrawRightWindowView();

        DrawHorizontalSplitters();
        DrawVerticalSplitters();
    }
}
#endif
