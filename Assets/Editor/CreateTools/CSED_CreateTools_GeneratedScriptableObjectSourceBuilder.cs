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
        builder.AppendLine("[CreateAssetMenu(fileName = \"" + f_className + "\", menuName = \"ScriptableObjects/" + f_className + "\")]");
        builder.AppendLine("public class " + f_className + " : ScriptableObject");
        builder.AppendLine("{");

        AppendGeneratedScriptableObjectFields(builder);

        builder.AppendLine("}");

        return builder.ToString();
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

            if (IsGeneratedSingleMinMaxField(fieldData))
            {
                string minVariableName = CreateGeneratedMinVariableName(variableName);
                string maxVariableName = CreateGeneratedMaxVariableName(variableName);

                f_builder.AppendLine("    /// <summary>");
                f_builder.AppendLine("    /// " + minVariableName + "です。");
                f_builder.AppendLine("    /// </summary>");
                f_builder.AppendLine("    public " + typeName + " " + minVariableName + ";");
                f_builder.AppendLine();

                f_builder.AppendLine("    /// <summary>");
                f_builder.AppendLine("    /// " + maxVariableName + "です。");
                f_builder.AppendLine("    /// </summary>");
                f_builder.AppendLine("    public " + typeName + " " + maxVariableName + ";");
                f_builder.AppendLine();

                continue;
            }

            f_builder.AppendLine("    /// <summary>");
            f_builder.AppendLine("    /// " + variableName + "です。");
            f_builder.AppendLine("    /// </summary>");
            f_builder.AppendLine("    public " + typeName + " " + variableName + ";");
            f_builder.AppendLine();
        }
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
