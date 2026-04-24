/*
+=====================================
 ファイル名 : CSE_ScriptSearch_Tab.cs
 概要     : ScriptSearchツールの「タブUI（検索設定 / 検索結果）」
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ScriptSearch：タブUI（partial）
/// </summary>
public partial class CSE_ScriptSearch
{
    // 0 = 検索設定, 1 = 検索結果
    private int _tabIndex = 0;

    /// <summary>
    /// タブバーを描画する。
    /// </summary>
    private void DrawTabBar()
    {
        string[] tabs = { "検索設定", "検索結果" };
        _tabIndex = GUILayout.Toolbar(_tabIndex, tabs);
    }

    /// <summary>
    /// 「検索設定」タブの中身を描画する。
    /// </summary>
    private void DrawTab_SearchSettings()
    {
        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            GUILayout.Space(10.0f);

            // Scene選択UIを描画
            DrawTargetSceneField();
            GUILayout.Space(10.0f);

            // Script選択UIを描画
            DrawTargetScriptField();
            GUILayout.Space(10.0f);
        }

        // 検索開始ボタンを描画（処理は別CS側でOK）
        DrawSearchButtonField();
    }

    /// <summary>
    /// 「検索結果」タブの中身を描画する。
    /// </summary>
    private void DrawTab_SearchResults()
    {
        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            // タイトル
            EditorGUILayout.LabelField("検索結果", EditorStyles.boldLabel);
            GUILayout.Space(10.0f);

            // 表示対象の切り替え（Hierarchy / Assets）
            DrawResultTargetDropdown();
            GUILayout.Space(10.0f);

            // 選択中のみ描画（データは両方保持）
            DrawResultView_SelectedOnly();
        }
    }


}
#endif
