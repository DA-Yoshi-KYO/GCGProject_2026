/*
+=====================================
 ファイル名 : CSE_ScriptSearch_Script.cs
 概要     : ScriptSearchツールの「Script選択UI描画」
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ScriptSearch：Script選択UI（partial）
/// </summary>
public partial class CSE_ScriptSearch
{
    // 選択中のScript（Project上の .cs アセット）
    private MonoScript _targetScript;

    /// <summary>
    /// Script選択フィールドを描画する（クリックで一覧/検索して選択できる）
    /// </summary>
    private void DrawTargetScriptField()
    {
        EditorGUILayout.LabelField("検索対象Script", EditorStyles.boldLabel);
        GUILayout.Space(2.0f);

        _targetScript = (MonoScript)EditorGUILayout.ObjectField(
            "  Target Script",
            _targetScript,
            typeof(MonoScript),
            false
        );
    }
}
#endif
