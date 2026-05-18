/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedToolCreator.cs
 概要     : CreateToolsのEditorWindow生成処理をまとめるクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsのEditorWindow生成処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// EditorWindowスクリプトを生成します。
    /// </summary>
    private void CreateGeneratedEditorTool()
    {
        string safeClassName = SanitizeGeneratedToolClassName(m_GeneratedToolClassName);

        if (Directory.Exists(m_GeneratedToolOutputFolderPath) == false)
        {
            Directory.CreateDirectory(m_GeneratedToolOutputFolderPath);
        }

        string outputFilePath = Path.Combine(
            m_GeneratedToolOutputFolderPath,
            safeClassName + ".cs");

        File.WriteAllText(
            outputFilePath,
            CreateGeneratedEditorToolSource(safeClassName),
            Encoding.UTF8);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Create Complete",
            "EditorToolを生成しました。\n\n" + outputFilePath,
            "OK");
    }

    /// <summary>
    /// クラス名として使える文字列に変換します。
    /// </summary>
    /// <param name="f_className">変換前クラス名</param>
    /// <returns>変換後クラス名</returns>
    private string SanitizeGeneratedToolClassName(string f_className)
    {
        if (string.IsNullOrEmpty(f_className))
        {
            return "CSED_GeneratedToolWindow";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0 ; i < f_className.Length ; i++)
        {
            char currentChar = f_className[i];

            if (char.IsLetterOrDigit(currentChar) || currentChar == '_')
            {
                builder.Append(currentChar);
            }
        }

        if (builder.Length <= 0)
        {
            return "CSED_GeneratedToolWindow";
        }

        if (char.IsDigit(builder[0]))
        {
            builder.Insert(0, "_");
        }

        return builder.ToString();
    }

    /// <summary>
    /// C#文字列として使えるようにエスケープします。
    /// </summary>
    /// <param name="f_text">対象文字列</param>
    /// <returns>エスケープ済み文字列</returns>
    private string EscapeGeneratedToolString(string f_text)
    {
        if (string.IsNullOrEmpty(f_text))
        {
            return string.Empty;
        }

        return f_text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
#endif
