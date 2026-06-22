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
/// CreateToolsで扱うFieldの表示Layout種別です。
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
    /// Min / Max入力欄です。
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
    /// 候補から選択するタイプのレイアウトです。
    /// </summary>
    Dropdown,

    /// <summary>
    /// 候補から選択するタイプのレイアウトです。
    /// </summary>
    Select
}
#endif
