/*
+=====================================
 ファイル名 : CSED_CreateTools_FieldData.cs
 概要     : CreateToolsで中央エリアに配置する変数データ
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/08 表示レイアウト情報を追加
=====================================+
*/

#if UNITY_EDITOR

/// <summary>
/// CreateToolsで扱う変数1つ分のデータです。
/// </summary>
public class CSED_CreateTools_FieldData
{
    /// <summary>
    /// 変数の型です。
    /// </summary>
    public CSE_CreateTools_FieldType FieldType { get; private set; }

    /// <summary>
    /// 変数名です。
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// 表示レイアウト種別です。
    /// </summary>
    public CSE_CreateTools_FieldLayoutType FieldLayoutType { get; set; }

    /// <summary>
    /// 変数データを作成します。
    /// </summary>
    /// <param name="f_fieldType">変数の型</param>
    /// <param name="f_fieldName">変数名</param>
    /// <param name="f_fieldLayoutType">表示レイアウト種別</param>
    public CSED_CreateTools_FieldData(
        CSE_CreateTools_FieldType f_fieldType,
        string f_fieldName,
        CSE_CreateTools_FieldLayoutType f_fieldLayoutType)
    {
        FieldType = f_fieldType;
        FieldName = f_fieldName;
        FieldLayoutType = f_fieldLayoutType;
    }
}
#endif
