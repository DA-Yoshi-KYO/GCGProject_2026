/*
+=====================================
 ファイル名 : CSED_NewToolWindow.cs
 概要     : CreateToolsから自動生成されたEditorWindow
 作者     : ヨシモト リョウ
 履歴     : 2026/05/19 CreateToolsから自動生成
=====================================+
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsから自動生成されたEditorWindowです。
/// </summary>
public class CSED_NewToolWindow : EditorWindow, IHasCustomMenu
{
    /// <summary>
    /// 高級車です。
    /// </summary>
    private int 高級車 = 0;

    /// <summary>
    /// メニューからウィンドウを開きます。
    /// </summary>
    [MenuItem("Tools/New Tool")]
    public static void ShowWindow()
    {
        CSED_NewToolWindow window = GetWindow<CSED_NewToolWindow>("Test");
        window.minSize = new Vector2(360.0f, 240.0f);
    }

    /// <summary>
    /// EditorWindow右上メニューに項目を追加します。
    /// </summary>
    /// <param name="f_menu">追加先メニュー</param>
    public void AddItemsToMenu(GenericMenu f_menu)
    {
        f_menu.AddItem(
            new GUIContent("Create Asset Settings"),
            false,
            OpenCreateAssetSettings);
    }

    /// <summary>
    /// Create Asset設定を開きます。
    /// </summary>
    private void OpenCreateAssetSettings()
    {
        m_IsCreateAssetSettingsOpen = true;
        Repaint();
    }

    /// <summary>
    /// GUIを描画します。
    /// </summary>
    private void OnGUI()
    {
        高級車 = EditorGUILayout.IntField("newIntField01", 高級車);
        GUILayout.Space(6.0f);

        GUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Create Asset", EditorStyles.boldLabel);

        if (GUILayout.Button("Create ScriptableObject", GUILayout.Height(28.0f)))
        {
            CreateScriptableObjectAsset();
        }

        DrawCreateAssetSettingsPanel();
    }

    /// <summary>
    /// Create Asset設定パネルを描画します。
    /// </summary>
    private void DrawCreateAssetSettingsPanel()
    {
        if (m_IsCreateAssetSettingsOpen == false)
        {
            return;
        }

        Rect panelRect = new Rect(
            24.0f,
            48.0f,
            Mathf.Min(360.0f, position.width - 48.0f),
            118.0f);

        EditorGUI.DrawRect(panelRect, new Color(0.24f, 0.24f, 0.24f));
        GUI.Box(panelRect, GUIContent.none);

        GUILayout.BeginArea(new Rect(
            panelRect.x + 10.0f,
            panelRect.y + 8.0f,
            panelRect.width - 20.0f,
            panelRect.height - 16.0f));
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Create Asset Settings", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("×", GUILayout.Width(24.0f)))
                {
                    m_IsCreateAssetSettingsOpen = false;
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6.0f);

            m_AssetFileName = EditorGUILayout.TextField("Asset Name", m_AssetFileName);
            m_AssetOutputFolderPath = EditorGUILayout.TextField("Asset Folder", m_AssetOutputFolderPath);
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// Create Asset設定を開いているかどうかです。
    /// </summary>
    private bool m_IsCreateAssetSettingsOpen = false;

    /// <summary>
    /// 作成するScriptableObjectアセット名です。
    /// </summary>
    private string m_AssetFileName = "NewData";

    /// <summary>
    /// ScriptableObjectアセットの保存先です。
    /// </summary>
    private string m_AssetOutputFolderPath = "Assets/Programmer/ScriptableObject";

    /// <summary>
    /// ScriptableObjectアセットを作成します。
    /// </summary>
    private void CreateScriptableObjectAsset()
    {
        if (System.IO.Directory.Exists(m_AssetOutputFolderPath) == false)
        {
            System.IO.Directory.CreateDirectory(m_AssetOutputFolderPath);
        }

        CSS_NewToolData asset = CreateInstance<CSS_NewToolData>();
        asset.高級車 = 高級車;

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(
            System.IO.Path.Combine(m_AssetOutputFolderPath, m_AssetFileName + ".asset"));

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;
    }
}
#endif
