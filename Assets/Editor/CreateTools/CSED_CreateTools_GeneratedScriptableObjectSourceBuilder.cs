/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedScriptableObjectSourceBuilder.cs
 概要     : CreateToolsで生成するScriptableObjectソースコードを作成するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System.Text;

/// <summary>
/// CreateToolsのScriptableObjectソース生成処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 生成ScriptableObjectのソースコードを作成します。
    /// </summary>
    /// <param name="f_className">生成するScriptableObjectクラス名</param>
    /// <returns>生成ソースコード</returns>
    private string CreateGeneratedScriptableObjectSource(string f_className)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("/*");
        builder.AppendLine("+=====================================");
        builder.AppendLine(" ファイル名 : " + f_className + ".cs");
        builder.AppendLine(" 概要     : " + GetGeneratedHeaderCommentText(
            m_GeneratedDataHeaderContents,
            "CreateToolsから自動生成されたScriptableObjectデータ"));
        builder.AppendLine(" 作者     : " + GetGeneratedHeaderCommentText(
            m_GeneratedHeaderAuthorName,
            "ヨシモト リョウ"));
        builder.AppendLine(" 履歴     : " + GetGeneratedHeaderCommentText(
            m_GeneratedHeaderHistoryDate,
            System.DateTime.Now.ToString("yyyy/MM/dd")) + " CreateToolsから自動生成");
        builder.AppendLine("=====================================+");
        builder.AppendLine("*/");
        builder.AppendLine();
        builder.AppendLine("using System.Collections.Generic;");

        if (HasGeneratedMonoScriptField())
        {
            builder.AppendLine("using UnityEditor;");
        }

        builder.AppendLine("using UnityEngine;");
        builder.AppendLine();
        builder.AppendLine("[CreateAssetMenu(fileName = \"" + f_className + "\", menuName = \"Scriptable Objects/" + f_className + "\")]");
        builder.AppendLine("public class " + f_className + " : ScriptableObject");
        builder.AppendLine("{");

        AppendGeneratedScriptableObjectFields(builder);

        AppendGeneratedInitializeFromCreateTools(builder);

        builder.AppendLine("}");

        return builder.ToString();
    }

    /// <summary>
    /// CreateToolsから値を初期化するメソッドを生成します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    private void AppendGeneratedInitializeFromCreateTools(StringBuilder f_builder)
    {
        if (m_FieldDataList == null || m_FieldDataList.Count <= 0)
        {
            return;
        }

        int parameterCount = 0;

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

            if (IsGeneratedSingleMinMaxField(fieldData))
            {
                parameterCount += 2;
            }
            else
            {
                parameterCount++;
            }
        }

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// CreateToolsから値を初期化します。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("#if UNITY_EDITOR");
        f_builder.AppendLine("    public void InitializeFromCreateTools(");

        int parameterIndex = 0;

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

            string typeName = GetGeneratedFieldTypeName(fieldData);
            string variableName = CreateGeneratedVariableName(fieldData.FieldName, i);

            if (IsGeneratedSingleMinMaxField(fieldData))
            {
                string minVariableName = CreateGeneratedMinVariableName(variableName);
                string maxVariableName = CreateGeneratedMaxVariableName(variableName);

                AppendGeneratedInitializeParameter(
                    f_builder,
                    typeName,
                    minVariableName,
                    parameterIndex,
                    parameterCount);

                parameterIndex++;

                AppendGeneratedInitializeParameter(
                    f_builder,
                    typeName,
                    maxVariableName,
                    parameterIndex,
                    parameterCount);

                parameterIndex++;

                continue;
            }

            AppendGeneratedInitializeParameter(
                f_builder,
                typeName,
                variableName,
                parameterIndex,
                parameterCount);

            parameterIndex++;
        }

        f_builder.AppendLine("        )");
        f_builder.AppendLine("    {");

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

            string typeName = GetGeneratedFieldTypeName(fieldData);
            string variableName = CreateGeneratedVariableName(fieldData.FieldName, i);
            string propertyName = CreateGeneratedPropertyName(variableName, i);

            if (IsGeneratedSingleMinMaxField(fieldData))
            {
                string minVariableName = CreateGeneratedMinVariableName(variableName);
                string maxVariableName = CreateGeneratedMaxVariableName(variableName);

                string minPropertyName = CreateGeneratedPropertyName(minVariableName, i);
                string maxPropertyName = CreateGeneratedPropertyName(maxVariableName, i);

                f_builder.AppendLine("        " + minPropertyName + " = f_" + minVariableName + ";");
                f_builder.AppendLine("        " + maxPropertyName + " = f_" + maxVariableName + ";");

                continue;
            }

            if (fieldData.FieldType == CSE_CreateTools_FieldType.List)
            {
                f_builder.AppendLine("        " + propertyName + " = new " + typeName + "(f_" + variableName + ");");
            }
            else
            {
                f_builder.AppendLine("        " + propertyName + " = f_" + variableName + ";");
            }
        }

        f_builder.AppendLine("    }");
        f_builder.AppendLine("#endif");
        f_builder.AppendLine();
    }

    /// <summary>
    /// InitializeFromCreateTools用の引数を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_typeName">型名</param>
    /// <param name="f_variableName">変数名</param>
    /// <param name="f_parameterIndex">引数番号</param>
    /// <param name="f_parameterCount">引数数</param>
    private void AppendGeneratedInitializeParameter(
        StringBuilder f_builder,
        string f_typeName,
        string f_variableName,
        int f_parameterIndex,
        int f_parameterCount)
    {
        string commaText = f_parameterIndex < f_parameterCount - 1 ? "," : string.Empty;

        f_builder.AppendLine("        " + f_typeName + " f_" + f_variableName + commaText);
    }

    /// <summary>
    /// ScriptableObject用の変数定義を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    private void AppendGeneratedScriptableObjectFields(StringBuilder f_builder)
    {
        if (m_FieldDataList == null)
        {
            return;
        }

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

            string typeName = GetGeneratedFieldTypeName(fieldData);
            string variableName = CreateGeneratedVariableName(fieldData.FieldName, i);
            string propertyName = CreateGeneratedPropertyName(variableName, i);

            if (IsGeneratedSingleMinMaxField(fieldData))
            {
                string minVariableName = CreateGeneratedMinVariableName(variableName);
                string maxVariableName = CreateGeneratedMaxVariableName(variableName);

                string minPropertyName = CreateGeneratedPropertyName(minVariableName, i);
                string maxPropertyName = CreateGeneratedPropertyName(maxVariableName, i);

                AppendGeneratedScriptableObjectProperty(
                    f_builder,
                    fieldData,
                    typeName,
                    minPropertyName,
                    CreateGeneratedDefaultValueCode(fieldData.DefaultMinValueText, typeName, fieldData.IsDefaultMinValueNull));

                AppendGeneratedScriptableObjectProperty(
                    f_builder,
                    fieldData,
                    typeName,
                    maxPropertyName,
                    CreateGeneratedDefaultValueCode(fieldData.DefaultMaxValueText, typeName, fieldData.IsDefaultMaxValueNull));

                continue;
            }

            AppendGeneratedScriptableObjectProperty(
                f_builder,
                fieldData,
                typeName,
                propertyName,
                CreateGeneratedDefaultValueCode(fieldData.DefaultValueText, typeName, fieldData.IsDefaultValueNull));
        }
    }

    /// <summary>
    /// ScriptableObject用のプロパティ定義を追加します。
    /// </summary>
    /// <param name="f_builder">StringBuilder</param>
    /// <param name="f_fieldData">FieldData</param>
    /// <param name="f_typeName">型名</param>
    /// <param name="f_propertyName">プロパティ名</param>
    /// <param name="f_defaultValueCode">初期値コード</param>
    private void AppendGeneratedScriptableObjectProperty(
        StringBuilder f_builder,
        CSED_CreateTools_FieldData f_fieldData,
        string f_typeName,
        string f_propertyName,
        string f_defaultValueCode)
    {
        string setterText = f_fieldData.IsPublicSetter ? "set;" : "private set;";

        f_builder.AppendLine("    /// <summary>");
        f_builder.AppendLine("    /// " + f_propertyName + "です。");
        f_builder.AppendLine("    /// </summary>");
        f_builder.AppendLine("    [field: SerializeField]");

        if (string.IsNullOrEmpty(f_fieldData.TooltipText) == false)
        {
            f_builder.AppendLine("    [field: Tooltip(\"" + EscapeGeneratedString(f_fieldData.TooltipText) + "\")]");
        }

        if (string.IsNullOrEmpty(f_defaultValueCode))
        {
            f_builder.AppendLine("    public " + f_typeName + " " + f_propertyName + " { get; " + setterText + " }");
        }
        else
        {
            f_builder.AppendLine("    public " + f_typeName + " " + f_propertyName + " { get; " + setterText + " } = " + f_defaultValueCode + ";");
        }

        f_builder.AppendLine();
    }

    /// <summary>
    /// 変数名からプロパティ名を作成します。
    /// </summary>
    /// <param name="f_variableName">変数名</param>
    /// <param name="f_index">番号</param>
    /// <returns>プロパティ名</returns>
    private string CreateGeneratedPropertyName(string f_variableName, int f_index)
    {
        if (string.IsNullOrEmpty(f_variableName))
        {
            return "GeneratedField" + f_index.ToString("00");
        }

        if (f_variableName.Length == 1)
        {
            return f_variableName.ToUpper();
        }

        return char.ToUpper(f_variableName[0]) + f_variableName.Substring(1);
    }

    /// <summary>
    /// 初期値コードを作成します。
    /// </summary>
    /// <param name="f_defaultValueText">初期値文字列</param>
    /// <param name="f_typeName">型名</param>
    /// <param name="f_isNull">Null扱いかどうか</param>
    /// <returns>初期値コード</returns>
    private string CreateGeneratedDefaultValueCode(
        string f_defaultValueText,
        string f_typeName,
        bool f_isNull)
    {
        if (f_isNull)
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(f_defaultValueText))
        {
            return string.Empty;
        }

        if (f_typeName == "string")
        {
            return "\"" + EscapeGeneratedString(f_defaultValueText) + "\"";
        }

        if (f_typeName == "float")
        {
            if (f_defaultValueText.EndsWith("f"))
            {
                return f_defaultValueText;
            }

            return f_defaultValueText + "f";
        }

        if (f_typeName == "bool")
        {
            return f_defaultValueText.ToLower();
        }

        return f_defaultValueText;
    }

    /// <summary>
    /// 生成コード内の文字列用にエスケープします。
    /// </summary>
    /// <param name="f_text">元文字列</param>
    /// <returns>エスケープ後文字列</returns>
    private string EscapeGeneratedString(string f_text)
    {
        if (string.IsNullOrEmpty(f_text))
        {
            return string.Empty;
        }

        return f_text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    /// <summary>
    /// 生成対象FieldにMonoScriptが含まれるか判定します。
    /// </summary>
    /// <returns>MonoScriptが必要ならtrue</returns>
    private bool HasGeneratedMonoScriptField()
    {
        if (m_FieldDataList == null)
        {
            return false;
        }

        for (int i = 0 ; i < m_FieldDataList.Count ; i++)
        {
            CSED_CreateTools_FieldData fieldData = m_FieldDataList[i];

            if (fieldData.FieldType == CSE_CreateTools_FieldType.Script)
            {
                return true;
            }

            if (fieldData.FieldType == CSE_CreateTools_FieldType.List &&
                fieldData.ListElementFieldType == CSE_CreateTools_FieldType.Script)
            {
                return true;
            }
        }

        return false;
    }
}
#endif
