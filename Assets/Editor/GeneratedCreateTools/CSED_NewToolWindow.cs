/*
+=====================================
 ファイル名 : CSED_NewToolWindow.cs
 概要     : CreateToolsから自動生成されたEditorWindow
 作者     : ヨシモト リョウ
 履歴     : 2026/05/18 CreateToolsから自動生成
=====================================+
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsから自動生成されたEditorWindowです。
/// </summary>
public class CSED_NewToolWindow : EditorWindow
{
    /// <summary>
    /// メニューからウィンドウを開きます。
    /// </summary>
    [MenuItem("Tools/Generated/New Tool")]
    public static void ShowWindow()
    {
        CSED_NewToolWindow window = GetWindow<CSED_NewToolWindow>("New Tool");
        window.minSize = new Vector2(360.0f, 240.0f);
    }

    /// <summary>
    /// GUIを描画します。
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.LabelField("New Tool", EditorStyles.boldLabel);
    }
}
#endif
