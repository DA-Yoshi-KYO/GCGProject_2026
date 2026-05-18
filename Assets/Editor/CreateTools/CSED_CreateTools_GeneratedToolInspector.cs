/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedToolInspector.cs
 概要     : CreateTools右側に生成設定とCreateボタンを表示するクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 新規作成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsの生成設定Inspector描画処理をまとめるクラスです。
/// </summary>
public partial class CSED_CreateTools
{
    /// <summary>
    /// 生成ツール設定を描画します。
    /// </summary>
    /// <param name="f_areaRect">描画範囲</param>
    private void DrawGeneratedToolInspector(Rect f_areaRect)
    {
        EditorGUI.DrawRect(f_areaRect, new Color(0.18f, 0.18f, 0.18f));

        Rect contentRect = new Rect(
            f_areaRect.x + 8.0f,
            f_areaRect.y + 8.0f,
            Mathf.Max(0.0f, f_areaRect.width - 16.0f),
            Mathf.Max(0.0f, f_areaRect.height - 16.0f));

        GUILayout.BeginArea(contentRect);
        {
            EditorGUILayout.LabelField("Tool生成設定", EditorStyles.boldLabel);

            GUILayout.Space(6.0f);

            m_GeneratedToolWindowTitle = EditorGUILayout.TextField(
                "Window Title",
                m_GeneratedToolWindowTitle);

            m_GeneratedToolClassName = EditorGUILayout.TextField(
                "Class Name",
                m_GeneratedToolClassName);

            m_GeneratedToolMenuPath = EditorGUILayout.TextField(
                "Menu Path",
                m_GeneratedToolMenuPath);

            EditorGUILayout.BeginHorizontal();
            {
                m_GeneratedToolOutputFolderPath = EditorGUILayout.TextField(
                    "Output Folder",
                    m_GeneratedToolOutputFolderPath);

                if (GUILayout.Button("...", GUILayout.Width(28.0f)))
                {
                    SelectGeneratedToolOutputFolder();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8.0f);

            EditorGUI.BeginDisabledGroup(CanCreateGeneratedTool() == false);
            {
                if (GUILayout.Button("Create Editor Tool", GUILayout.Height(26.0f)))
                {
                    CreateGeneratedEditorTool();
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 生成先フォルダを選択します。
    /// </summary>
    private void SelectGeneratedToolOutputFolder()
    {
        string selectedPath = EditorUtility.OpenFolderPanel(
            "生成先フォルダを選択",
            "Assets",
            string.Empty);

        if (string.IsNullOrEmpty(selectedPath))
        {
            return;
        }

        string projectPath = Application.dataPath.Replace("/Assets", string.Empty);

        if (selectedPath.StartsWith(projectPath) == false)
        {
            EditorUtility.DisplayDialog(
                "Output Folder Error",
                "Assetsフォルダ内を選択してください。",
                "OK");

            return;
        }

        m_GeneratedToolOutputFolderPath = selectedPath.Replace(projectPath + "/", string.Empty);
    }

    /// <summary>
    /// EditorToolを生成できる状態か判定します。
    /// </summary>
    /// <returns>生成できる場合true</returns>
    private bool CanCreateGeneratedTool()
    {
        if (string.IsNullOrEmpty(m_GeneratedToolWindowTitle))
        {
            return false;
        }

        if (string.IsNullOrEmpty(m_GeneratedToolClassName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(m_GeneratedToolMenuPath))
        {
            return false;
        }

        if (string.IsNullOrEmpty(m_GeneratedToolOutputFolderPath))
        {
            return false;
        }

        return true;
    }
}
#endif
