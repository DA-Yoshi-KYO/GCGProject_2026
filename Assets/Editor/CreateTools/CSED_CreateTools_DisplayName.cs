/*
+=====================================
 ファイル名 : CSED_CreateTools_DisplayName.cs
 概要     : CreateToolsで使用する表示名変換処理
 作者     : ヨシモト リョウ
 履歴     : 2026/05/13 新規作成
=====================================+
*/

#if UNITY_EDITOR

/// <summary>
/// CreateToolsで使用する表示名変換処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 変数型の表示名を取得します。
    /// </summary>
    /// <param name="f_fieldType">表示名を取得する変数型</param>
    /// <returns>変数型の表示名</returns>
    private string GetFieldTypeDisplayName(CSE_CreateTools_FieldType f_fieldType)
    {
        switch (f_fieldType)
        {
            case CSE_CreateTools_FieldType.Int:
                return "int";

            case CSE_CreateTools_FieldType.Vector2Int:
                return "Vector2Int";

            case CSE_CreateTools_FieldType.Vector3Int:
                return "Vector3Int";

            case CSE_CreateTools_FieldType.Float:
                return "float";

            case CSE_CreateTools_FieldType.Vector2:
                return "Vector2";

            case CSE_CreateTools_FieldType.Vector3:
                return "Vector3";

            case CSE_CreateTools_FieldType.String:
                return "string";

            case CSE_CreateTools_FieldType.Bool:
                return "bool";

            case CSE_CreateTools_FieldType.Enum:
                return "Enum";

            case CSE_CreateTools_FieldType.ScriptableObject:
                return "ScriptableObject";

            case CSE_CreateTools_FieldType.Script:
                return "Script";

            case CSE_CreateTools_FieldType.GameObject:
                return "GameObject";

            case CSE_CreateTools_FieldType.List: 
                return "List<T>";

            default:
                return "Unknown";
        }
    }

    /// <summary>
    /// FieldLayoutTypeの表示名を取得します。
    /// </summary>
    /// <param name="f_layoutType">表示名を取得するLayout型</param>
    /// <returns>Layout型の表示名</returns>
    private string GetFieldLayoutDisplayName(CSE_CreateTools_FieldLayoutType f_layoutType)
    {
        switch (f_layoutType)
        {
            case CSE_CreateTools_FieldLayoutType.InputField:
                return "Input Field";

            case CSE_CreateTools_FieldLayoutType.Slider:
                return "Slider";

            case CSE_CreateTools_FieldLayoutType.MinMaxField:
                return "Min Max Field";

            case CSE_CreateTools_FieldLayoutType.Toggle:
                return "Toggle";

            case CSE_CreateTools_FieldLayoutType.TextArea:
                return "Text Area";

            case CSE_CreateTools_FieldLayoutType.Dropdown:
                return "Dropdown";

            default:
                return "Unknown";
        }
    }
}
#endif
