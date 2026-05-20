/*
+=====================================
 ファイル名 : CSED_CreateTools_GeneratedEditorsWindow.cs
 概要     : CreateToolsで生成したEditor一覧を表示するEditorWindow
 作者     : ヨシモト リョウ
 履歴     : 2026/05/19 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsで生成したEditor一覧を表示するEditorWindowです。
/// </summary>
public class CSED_CreateTools_GeneratedEditorsWindow : EditorWindow
{
    /// <summary>
    /// 生成済みEditor一覧のスクロール位置です。
    /// </summary>
    private Vector2 m_ScrollPosition;

    /// <summary>
    /// Generated Editorsウィンドウを開きます。
    /// </summary>
    public static void OpenWindow()
    {
        CSED_CreateTools_GeneratedEditorsWindow window =
            GetWindow<CSED_CreateTools_GeneratedEditorsWindow>(
                "Generated Editors",
                false,
                typeof(CSED_CreateTools));

        window.minSize = new Vector2(
            420.0f,
            300.0f);
    }

    /// <summary>
    /// GUIを描画します。
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Generated Editors", EditorStyles.boldLabel);

        GUILayout.Space(8.0f);

        CSS_CreateToolsGeneratedEditorList editorList =
            LoadOrCreateGeneratedEditorList();

        if (editorList.generatedEditorRecordList.Count <= 0)
        {
            EditorGUILayout.HelpBox(
                "まだ生成済みEditorがありません。",
                MessageType.Info);

            return;
        }

        m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
        {
            for (int i = 0 ; i < editorList.generatedEditorRecordList.Count ; i++)
            {
                DrawGeneratedEditorRecord(editorList.generatedEditorRecordList[i]);
                GUILayout.Space(6.0f);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 生成済みEditor情報を1件描画します。
    /// </summary>
    /// <param name="f_record">生成済みEditor情報</param>
    private void DrawGeneratedEditorRecord(CSED_CreateTools_GeneratedEditorRecord f_record)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField(f_record.titleName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Editor Class", f_record.editorClassName);
            EditorGUILayout.LabelField("Data Class", f_record.dataClassName);
            EditorGUILayout.LabelField("Menu Path", f_record.menuPath);
            EditorGUILayout.LabelField("Created", f_record.createdDate);

            GUILayout.Space(4.0f);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Open Editor", GUILayout.Height(24.0f)))
                {
                    OpenGeneratedEditorWindow(f_record);
                }

                if (GUILayout.Button("Ping Script", GUILayout.Width(90.0f), GUILayout.Height(24.0f)))
                {
                    PingGeneratedEditorScript(f_record);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 生成済みEditorWindowを開きます。
    /// </summary>
    /// <param name="f_record">生成済みEditor情報</param>
    private void OpenGeneratedEditorWindow(CSED_CreateTools_GeneratedEditorRecord f_record)
    {
        Type editorWindowType = FindTypeByName(f_record.editorClassName);

        if (editorWindowType == null)
        {
            EditorUtility.DisplayDialog(
                "Open Editor Error",
                "EditorWindowクラスが見つかりません。\nUnityのコンパイル完了後にもう一度試してください。\n\n" + f_record.editorClassName,
                "OK");

            return;
        }

        EditorWindow window = GetWindow(editorWindowType, false, f_record.titleName);
        window.Show();
    }

    /// <summary>
    /// 指定名のTypeを現在読み込まれているAssemblyから探します。
    /// </summary>
    /// <param name="f_typeName">探すType名</param>
    /// <returns>見つかったType</returns>
    private Type FindTypeByName(string f_typeName)
    {
        System.Reflection.Assembly[] assemblies =
            AppDomain.CurrentDomain.GetAssemblies();

        for (int i = 0 ; i < assemblies.Length ; i++)
        {
            Type type = assemblies[i].GetType(f_typeName);

            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    /// <summary>
    /// 生成済みEditorスクリプトをProject上で選択します。
    /// </summary>
    /// <param name="f_record">生成済みEditor情報</param>
    private void PingGeneratedEditorScript(CSED_CreateTools_GeneratedEditorRecord f_record)
    {
        UnityEngine.Object scriptAsset =
            AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(f_record.editorScriptPath);

        if (scriptAsset == null)
        {
            return;
        }

        Selection.activeObject = scriptAsset;
        EditorGUIUtility.PingObject(scriptAsset);
    }

    /// <summary>
    /// 生成済みEditor一覧アセットを読み込みます。
    /// 存在しない場合は作成します。
    /// </summary>
    /// <returns>生成済みEditor一覧アセット</returns>
    private static CSS_CreateToolsGeneratedEditorList LoadOrCreateGeneratedEditorList()
    {
        const string assetPath = "Assets/Editor/CreateTools/CSS_CreateToolsGeneratedEditorList.asset";

        CSS_CreateToolsGeneratedEditorList editorList =
            AssetDatabase.LoadAssetAtPath<CSS_CreateToolsGeneratedEditorList>(assetPath);

        if (editorList != null)
        {
            return editorList;
        }

        string folderPath = System.IO.Path.GetDirectoryName(assetPath);

        if (System.IO.Directory.Exists(folderPath) == false)
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        editorList = CreateInstance<CSS_CreateToolsGeneratedEditorList>();

        AssetDatabase.CreateAsset(editorList, assetPath);
        AssetDatabase.SaveAssets();

        return editorList;
    }
}
#endif
