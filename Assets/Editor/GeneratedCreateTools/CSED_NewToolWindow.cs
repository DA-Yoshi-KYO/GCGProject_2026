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
public class CSED_NewToolWindow : EditorWindow
{
    /// <summary>
    /// n_numです。
    /// </summary>
    private int n_num = 0;

    /// <summary>
    /// f_EnemySpeedです。
    /// </summary>
    private float f_EnemySpeed = 0.0f;

    /// <summary>
    /// lg_WeaponListです。
    /// </summary>
    private List<GameObject> lg_WeaponList = new List<GameObject>()
    {
        null,
        null
    };

    /// <summary>
    /// メニューからウィンドウを開きます。
    /// </summary>
    [MenuItem("Tools/Generated/New Tool")]
    public static void ShowWindow()
    {
        CSED_NewToolWindow window = GetWindow<CSED_NewToolWindow>("Test");
        window.minSize = new Vector2(360.0f, 240.0f);
    }

    /// <summary>
    /// GUIを描画します。
    /// </summary>
    private void OnGUI()
    {
        n_num = EditorGUILayout.IntField("敵の数", n_num);
        GUILayout.Space(6.0f);

        f_EnemySpeed = EditorGUILayout.Slider("敵のスピード", f_EnemySpeed, 0.0f, 0.0f);
        GUILayout.Space(6.0f);

        EditorGUILayout.LabelField("敵の武器リスト", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("-", GUILayout.Width(24.0f)) && lg_WeaponList.Count > 0)
            {
                lg_WeaponList.RemoveAt(lg_WeaponList.Count - 1);
            }

            EditorGUILayout.LabelField(lg_WeaponList.Count.ToString(), GUILayout.Width(32.0f));

            if (GUILayout.Button("+", GUILayout.Width(24.0f)))
            {
                lg_WeaponList.Add(null);
            }
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < lg_WeaponList.Count; i++)
        {
            lg_WeaponList[i] = (GameObject)EditorGUILayout.ObjectField("Element " + i.ToString(), lg_WeaponList[i], typeof(GameObject), false);
        }
        GUILayout.Space(6.0f);

        GUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Create Asset", EditorStyles.boldLabel);

        m_AssetFileName = EditorGUILayout.TextField("Asset Name", m_AssetFileName);
        m_AssetOutputFolderPath = EditorGUILayout.TextField("Asset Folder", m_AssetOutputFolderPath);

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
    private string m_AssetOutputFolderPath = "Assets/ScriptableObject/GeneratedData";

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
        asset.n_num = n_num;
        asset.f_EnemySpeed = f_EnemySpeed;
        asset.lg_WeaponList = new List<GameObject>(lg_WeaponList);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(
            System.IO.Path.Combine(m_AssetOutputFolderPath, m_AssetFileName + ".asset"));

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;
    }
}
#endif
