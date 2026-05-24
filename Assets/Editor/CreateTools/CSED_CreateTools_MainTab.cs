/*
+=====================================
 ファイル名 : CSED_CreateTools_MainTab.cs
 概要     : CreateToolsの上部タブ切り替え処理
 作者     : ヨシモト リョウ
 履歴     : 2026/05/19 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsの上部タブ切り替え処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 上部タブの高さです。
    /// </summary>
    private const float c_CreateToolsMainTabHeight = 24.0f;

    /// <summary>
    /// 現在選択中のメインタブ番号です。
    /// </summary>
    private int m_CreateToolsMainTabIndex = 0;

    /// <summary>
    /// 上部メインタブを描画します。
    /// </summary>
    private void DrawCreateToolsMainTabBar()
    {
        string[] tabNames =
        {
            "CreateTools",
            "Generated Editors"
        };

        Rect tabRect = new Rect(
            c_Margin,
            c_Margin,
            position.width - (c_Margin * 2.0f),
            c_CreateToolsMainTabHeight);

        m_CreateToolsMainTabIndex = GUI.Toolbar(
            tabRect,
            m_CreateToolsMainTabIndex,
            tabNames,
            EditorStyles.toolbarButton);
    }

    /// <summary>
    /// メインタブ下の開始Y座標を取得します。
    /// </summary>
    /// <returns>メインコンテンツ開始Y座標</returns>
    private float GetCreateToolsMainContentY()
    {
        return c_Margin + c_CreateToolsMainTabHeight + c_Margin;
    }

    /// <summary>
    /// メインタブ下の表示高さを取得します。
    /// </summary>
    /// <returns>メインコンテンツ高さ</returns>
    private float GetCreateToolsMainContentHeight()
    {
        return position.height - GetCreateToolsMainContentY() - c_Margin;
    }
}
#endif
