/*
+=====================================
 ファイル名 : CSE_CreateTools.cs
 概要     : SCreateToolsツールのEditorWindowクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/04/20 新規作成
            2026/04/22 構成整理
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ツール用のEditorWindow雛形（IMGUI）
/// </summary>
public partial class CSE_CreateTools : EditorWindow
{
    /// <summary>
    /// メニューからウィンドウを開く。
    /// </summary>
    [MenuItem("Tools/CreateTools")]
    public static void ShowWindow()
    {
        CSE_CreateTools window = GetWindow<CSE_CreateTools>("CreateTools");
        window.position = new Rect(100.0f, 100.0f, c_WindowInitWidth, c_WindowInitHeight);
        window.minSize = new Vector2(c_WindowMinWidth, c_WindowMinHeight);
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