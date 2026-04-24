/*
+=====================================
 ファイル名 : CSE_ScriptSearch_SearchAction.cs
 概要     : ScriptSearchツールの検索実行入口（Searchボタン押下時）
 作者     : ヨシモト リョウ
 履歴     : 2026/02/15 新規作成
=====================================+
*/

#if UNITY_EDITOR
public partial class CSE_ScriptSearch
{
    /// <summary>
    /// Searchボタン押下時に呼ばれる（UI側から）
    /// </summary>
    partial void OnClickSearchButton()
    {
        // 結果タブへ移動
        _tabIndex = 1;

        // Hierarchy / Assets で分岐
        if (GetResultTarget() == ResultTarget.Hierarchy)
        {
            ExecuteHierarchySearch();
            return;
        }

        if (GetResultTarget() == ResultTarget.Assets)
        {
            ExecuteAssetsSearch();
            return;
        }
    }
}
#endif
