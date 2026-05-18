/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedToolSourceBuilder.cs
 概要     : CreateToolsで生成するEditorWindowソースコードを作成するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System.Text;

/// <summary>
/// CreateToolsのEditorWindowソース生成処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 生成EditorWindowのソースコードを作成します。
    /// </summary>
    /// <param name="f_className">生成するクラス名</param>
    /// <returns>生成ソースコード</returns>
    private string CreateGeneratedEditorToolSource(string f_className)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("/*");
        builder.AppendLine("+=====================================");
        builder.AppendLine(" ファイル名 : " + f_className + ".cs");
        builder.AppendLine(" 概要     : CreateToolsから自動生成されたEditorWindow");
        builder.AppendLine(" 作者     : ヨシモト リョウ");
        builder.AppendLine(" 履歴     : 2026/05/18 CreateToolsから自動生成");
        builder.AppendLine("=====================================+");
        builder.AppendLine("*/");
        builder.AppendLine();
        builder.AppendLine("#if UNITY_EDITOR");
        builder.AppendLine("using UnityEditor;");
        builder.AppendLine("using UnityEngine;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// CreateToolsから自動生成されたEditorWindowです。");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public class " + f_className + " : EditorWindow");
        builder.AppendLine("{");
        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// メニューからウィンドウを開きます。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    [MenuItem(\"" + EscapeString(m_GeneratedToolMenuPath) + "\")]");
        builder.AppendLine("    public static void ShowWindow()");
        builder.AppendLine("    {");
        builder.AppendLine("        " + f_className + " window = GetWindow<" + f_className + ">(\"" + EscapeString(m_GeneratedToolWindowTitle) + "\");");
        builder.AppendLine("        window.minSize = new Vector2(360.0f, 240.0f);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// GUIを描画します。");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    private void OnGUI()");
        builder.AppendLine("    {");
        builder.AppendLine("        EditorGUILayout.LabelField(\"" + EscapeString(m_GeneratedToolWindowTitle) + "\", EditorStyles.boldLabel);");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine("#endif");

        return builder.ToString();
    }
}
#endif
