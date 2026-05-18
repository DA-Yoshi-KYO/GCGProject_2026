/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedToolNameUtility.cs
 概要     : CreateToolsの生成用名前変換処理をまとめるクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System.Text;

/// <summary>
/// CreateToolsの生成用名前変換処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// クラス名として使える文字列に変換します。
    /// </summary>
    /// <param name="f_className">変換前クラス名</param>
    /// <returns>変換後クラス名</returns>
    private string SanitizeClassName(string f_className)
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
    private string EscapeString(string f_text)
    {
        if (string.IsNullOrEmpty(f_text))
        {
            return string.Empty;
        }

        return f_text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
#endif
