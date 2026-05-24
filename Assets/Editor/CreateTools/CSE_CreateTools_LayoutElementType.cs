/*
+=====================================
 ファイル名 : CSE_CreateTools_LayoutElementType.cs
 概要     : CreateToolsで使用するレイアウト要素種別の定義
 作者     : ヨシモト リョウ
 履歴     : 2026/05/13 新規作成
            2026/05/13 Label要素を追加
=====================================+
*/

#if UNITY_EDITOR

/// <summary>
/// CreateToolsで扱うレイアウト要素種別を定義する列挙型です。
/// </summary>
public enum CSE_CreateTools_LayoutElementType
{
    /// <summary>
    /// 空白です。
    /// </summary>
    Space,

    /// <summary>
    /// 横線です。
    /// </summary>
    Line,

    /// <summary>
    /// 見出しです。
    /// </summary>
    Header,

    /// <summary>
    /// ラベル文字です。
    /// </summary>
    Label,

    /// <summary>
    /// 枠です。
    /// </summary>
    Box,

    /// <summary>
    /// タブです。
    /// </summary>
    Tab
}
#endif
