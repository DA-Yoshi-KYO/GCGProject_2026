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
            EditorGUILayout.LabelField(
                GetDisplayText(f_record.titleName, "Untitled"),
                EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                "エディター名",
                GetDisplayText(f_record.titleName, "Untitled"));

            EditorGUILayout.LabelField(
                "制作者",
                GetDisplayText(f_record.authorName, "未設定"));

            EditorGUILayout.LabelField(
                "作成日時",
                GetDisplayText(f_record.createdDate, "未設定"));

            EditorGUILayout.LabelField(
                "更新日時",
                GetDisplayText(f_record.updatedDate, "未設定"));

            GUILayout.Space(4.0f);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Load", GUILayout.Height(24.0f)))
                {
                    CSED_CreateTools.OpenAndLoadGeneratedEditorRecord(f_record);
                }

                if (GUILayout.Button("Editor Script", GUILayout.Height(24.0f)))
                {
                    PingGeneratedEditorScript(f_record);
                }

                if (GUILayout.Button("Data Script", GUILayout.Height(24.0f)))
                {
                    PingGeneratedDataScript(f_record);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 生成済みEditorスクリプトをProject上で選択します。
    /// </summary>
    /// <param name="f_record">生成済みEditor情報</param>
    private void PingGeneratedEditorScript(CSED_CreateTools_GeneratedEditorRecord f_record)
    {
        UnityEngine.Object editorScriptAsset =
            AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(f_record.editorScriptPath);

        if (editorScriptAsset == null)
        {
            EditorUtility.DisplayDialog(
                "Ping Editor Script Error",
                "Editorスクリプトが見つかりません。\n\n" + f_record.editorScriptPath,
                "OK");

            return;
        }

        Selection.activeObject = editorScriptAsset;
        EditorGUIUtility.PingObject(editorScriptAsset);
    }

    /// <summary>
    /// 生成済みScriptableObjectスクリプトをProject上で選択します。
    /// </summary>
    /// <param name="f_record">生成済みEditor情報</param>
    private void PingGeneratedDataScript(CSED_CreateTools_GeneratedEditorRecord f_record)
    {
        UnityEngine.Object dataScriptAsset =
            AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(f_record.dataScriptPath);

        if (dataScriptAsset == null)
        {
            EditorUtility.DisplayDialog(
                "Ping Data Script Error",
                "ScriptableObjectスクリプトが見つかりません。\n\n" + f_record.dataScriptPath,
                "OK");

            return;
        }

        Selection.activeObject = dataScriptAsset;
        EditorGUIUtility.PingObject(dataScriptAsset);
    }

    /// <summary>
    /// 表示用文字列を取得します。
    /// </summary>
    /// <param name="f_text">表示したい文字列</param>
    /// <param name="f_defaultText">空の場合の表示文字列</param>
    /// <returns>表示用文字列</returns>
    private string GetDisplayText(
        string f_text,
        string f_defaultText)
    {
        if (string.IsNullOrEmpty(f_text))
        {
            return f_defaultText;
        }

        return f_text;
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
