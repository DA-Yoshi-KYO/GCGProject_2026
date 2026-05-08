/*
+=====================================
 ファイル名 : CSE_CreateTools_FieldType.cs
 概要     : CreateToolsで使用する変数型の定義
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
=====================================+
*/

#if UNITY_EDITOR

/// <summary>
/// CreateToolsで扱う変数型を定義する列挙型です。
/// </summary>
public enum CSE_CreateTools_FieldType
{
    /// <summary>
    /// int型です。
    /// </summary>
    Int,

    /// <summary>
    /// float型です。
    /// </summary>
    Float,

    /// <summary>
    /// string型です。
    /// </summary>
    String,

    /// <summary>
    /// bool型です。
    /// </summary>
    Bool,

    /// <summary>
    /// List型です。
    /// </summary>
    List
}
#endif
