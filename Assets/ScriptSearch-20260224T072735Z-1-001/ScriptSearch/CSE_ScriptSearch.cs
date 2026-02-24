/*
+=====================================
 ファイル名 : CSE_ScriptSearch.cs
 概要     : ScriptSearchツールのEditorWindowクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ツール用のEditorWindow雛形（IMGUI）
/// </summary>
public partial class CSE_ScriptSearch : EditorWindow
{
    /// <summary>
    /// メニューからウィンドウを開く。
    /// </summary>
    [MenuItem("Tools/ScriptSearch")]
    public static void ShowWindow()
    {
        GetWindow<CSE_ScriptSearch>("ScriptSearch");
    }

    /// <summary>
    /// ウィンドウが有効になったタイミングで呼ばれる。
    /// </summary>
    private void OnEnable()
    {
        // 必要になったら初期化
    }

    /// <summary>
    /// GUIを描画する（IMGUI）。
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Space(10.0f);

        // タブ描画（ここで _tabIndex を更新）
        DrawTabBar();
        GUILayout.Space(8.0f);

        // タブ内容
        switch (_tabIndex)
        {
            case 0:
                DrawTab_SearchSettings();
                break;

            case 1:
                DrawTab_SearchResults();
                break;
        }

        DrawWikiLinkBottomLeft();
    }
}
#endif
