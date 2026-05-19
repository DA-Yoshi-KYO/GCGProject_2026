/*
+=====================================
 ファイル名 : CSED_NewToolWindow.cs
 概要     : CreateToolsから自動生成されたEditorWindow
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 CreateToolsから自動生成
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
    /// newIntField01です。
    /// </summary>
    private int newIntField01 = 0;

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
        newIntField01 = EditorGUILayout.IntField("newIntField01", newIntField01);
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

        GUILayout.Space(8.0f);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Create Asset Settings", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("×", GUILayout.Width(24.0f)))
                {
                    m_IsCreateAssetSettingsOpen = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6.0f);

            m_AssetFileName = EditorGUILayout.TextField("Asset Name", m_AssetFileName);
            m_AssetOutputFolderPath = EditorGUILayout.TextField("Asset Folder", m_AssetOutputFolderPath);
        }
        EditorGUILayout.EndVertical();
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
        asset.newIntField01 = newIntField01;

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(
            System.IO.Path.Combine(m_AssetOutputFolderPath, m_AssetFileName + ".asset"));

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;
    }
}
#endif
