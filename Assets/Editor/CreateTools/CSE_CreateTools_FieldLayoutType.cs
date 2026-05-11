/*
+=====================================
 ファイル名 : CSE_CreateTools_FieldLayoutType.cs
 概要     : CreateToolsで使用する表示レイアウト種別の定義
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
=====================================+
*/

#if UNITY_EDITOR

/// <summary>
/// CreateToolsで扱う表示レイアウト種別を定義する列挙型です。
/// </summary>
public enum CSE_CreateTools_FieldLayoutType
{
    /// <summary>
    /// 1つの入力欄です。
    /// </summary>
    InputField,

    /// <summary>
    /// スライダーです。
    /// </summary>
    Slider,

    /// <summary>
    /// 最小値と最大値の入力欄です。
    /// </summary>
    MinMaxField,

    /// <summary>
    /// チェックボックスです。
    /// </summary>
    Toggle,

    /// <summary>
    /// 複数行テキストです。
    /// </summary>
    TextArea,

    /// <summary>
    /// 並び替え可能なList表示です。
    /// </summary>
    ReorderableList
}
#endif
