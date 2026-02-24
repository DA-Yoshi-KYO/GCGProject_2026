/*
+=====================================
 ファイル名 : CSE_ScriptSearch_ResultViewSingle.cs
 概要     : ScriptSearchツールの「検索結果表示（選択中のみ描画）」担当
           - 表示はHierarchy/Assetsのどちらか片方だけ
           - ただし両方のデータは保持する（消さない）
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public partial class CSE_ScriptSearch
{
    /// <summary>
    /// 検索結果表示（選択中のみ描画）
    /// </summary>
    private void DrawResultView_SelectedOnly()
    {
        // データは保持していることが分かるように件数だけは両方出す（描画は片方）
        int hierarchyCount = (_hierarchyHitPaths != null) ? _hierarchyHitPaths.Count : 0;
        int assetsCount = (_assetsHitPrefabPaths != null) ? _assetsHitPrefabPaths.Count : 0;

        using (new GUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"Hierarchy: {hierarchyCount} 件", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Assets: {assetsCount} 件", EditorStyles.miniLabel);
        }

        GUILayout.Space(6.0f);

        // 描画は選択中だけ
        if (GetResultTarget() == ResultTarget.Hierarchy)
        {
            DrawHierarchyResultsView();
        }
        else
        {
            DrawAssetsResultsView();
        }
    }
}
#endif
