/*
+=====================================
 ファイル名 : CSE_ScriptSearch_ResultTarget.cs
 概要     : ScriptSearchツールの「検索結果：対象切替（Hierarchy / Assets）」
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
    /// 検索結果の表示対象
    /// </summary>
    private enum ResultTarget
    {
        Hierarchy = 0,
        Assets = 1,
    }

    // 現在の選択（0=Hierarchy, 1=Assets）
    private int _resultTargetIndex = 0;

    // 表示名
    private static readonly string[] ResultTargetOptions =
    {
        "Hierarchy",
        "Assets"
    };

    /// <summary>
    /// 「検索対象（Hierarchy/Assets）」のプルダウンを描画する。
    /// </summary>
    private void DrawResultTargetDropdown()
    {
        _resultTargetIndex = EditorGUILayout.Popup("検索対象", _resultTargetIndex, ResultTargetOptions);
    }

    /// <summary>
    /// 現在の選択対象を取得する（処理側で分岐に使う用）
    /// </summary>
    private ResultTarget GetResultTarget()
    {
        return (ResultTarget)_resultTargetIndex;
    }
}
#endif
