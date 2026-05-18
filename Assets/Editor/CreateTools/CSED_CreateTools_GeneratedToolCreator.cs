/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedToolCreator.cs
 概要     : CreateToolsのEditorWindow生成実行処理をまとめるクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 新規作成
            2026/05/18 EditorWindowとScriptableObject箱を同時生成する処理を追加
=====================================+
*/

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;

/// <summary>
/// CreateToolsのEditorWindow生成実行処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// EditorWindowスクリプトとScriptableObjectスクリプトを生成します。
    /// </summary>
    private void CreateGeneratedEditorTool()
    {
        string safeEditorClassName = SanitizeClassName(m_GeneratedToolClassName);
        string safeDataClassName = SanitizeClassName(m_GeneratedScriptableObjectClassName);

        if (Directory.Exists(m_GeneratedToolOutputFolderPath) == false)
        {
            Directory.CreateDirectory(m_GeneratedToolOutputFolderPath);
        }

        if (Directory.Exists(m_GeneratedScriptableObjectOutputFolderPath) == false)
        {
            Directory.CreateDirectory(m_GeneratedScriptableObjectOutputFolderPath);
        }

        string editorOutputFilePath = Path.Combine(
            m_GeneratedToolOutputFolderPath,
            safeEditorClassName + ".cs");

        string dataOutputFilePath = Path.Combine(
            m_GeneratedScriptableObjectOutputFolderPath,
            safeDataClassName + ".cs");

        File.WriteAllText(
            editorOutputFilePath,
            CreateGeneratedEditorToolSource(safeEditorClassName, safeDataClassName),
            Encoding.UTF8);

        File.WriteAllText(
            dataOutputFilePath,
            CreateGeneratedScriptableObjectSource(safeDataClassName),
            Encoding.UTF8);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Create Complete",
            "EditorToolとScriptableObjectの箱を生成しました。\n\n"
            + editorOutputFilePath
            + "\n"
            + dataOutputFilePath,
            "OK");
    }
}
#endif
