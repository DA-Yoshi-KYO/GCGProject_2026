/*
+=====================================
 ファイル名 : CSED_NewToolWindow.cs
 概要     : CreateToolsから自動生成されたEditorWindow
 作者     : ヨシモト リョウ
 履歴     : 2026/05/20 CreateToolsから自動生成
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
        CreateAssetSettingsWindow.Open(this);
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
    }

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

    /// <summary>
    /// Create Asset設定専用のEditorWindowです。
    /// </summary>
    private class CreateAssetSettingsWindow : EditorWindow
    {
        /// <summary>
        /// 設定対象のEditorWindowです。
        /// </summary>
        private CSED_NewToolWindow m_OwnerWindow;

        /// <summary>
        /// Create Asset設定Windowを開きます。
        /// </summary>
        /// <param name="f_ownerWindow">設定対象のEditorWindow</param>
        public static void Open(CSED_NewToolWindow f_ownerWindow)
        {
            CreateAssetSettingsWindow window = CreateInstance<CreateAssetSettingsWindow>();
            window.titleContent = new GUIContent("Create Asset Settings");
            window.m_OwnerWindow = f_ownerWindow;
            window.minSize = new Vector2(360.0f, 120.0f);
            window.position = new Rect(
                f_ownerWindow.position.x + 40.0f,
                f_ownerWindow.position.y + 40.0f,
                360.0f,
                120.0f);
            window.ShowUtility();
        }

        /// <summary>
        /// GUIを描画します。
        /// </summary>
        private void OnGUI()
        {
            if (m_OwnerWindow == null)
            {
                EditorGUILayout.HelpBox("設定対象のEditorWindowが見つかりません。", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Create Asset Settings", EditorStyles.boldLabel);
            GUILayout.Space(6.0f);

            m_OwnerWindow.m_AssetFileName = EditorGUILayout.TextField("Asset Name", m_OwnerWindow.m_AssetFileName);
            m_OwnerWindow.m_AssetOutputFolderPath = EditorGUILayout.TextField("Asset Folder", m_OwnerWindow.m_AssetOutputFolderPath);

            if (GUI.changed)
            {
                m_OwnerWindow.Repaint();
            }
        }
    }

}
#endif
