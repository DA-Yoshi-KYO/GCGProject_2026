/*
+=====================================
 ファイル名 : CSED_EffectDebugWindow.cs
 概要     : 選択中のエフェクト配下にあるCS_スクリプトを一覧表示するデバッグウィンドウ
 作者     : ヨシモト リョウ
 履歴     : 2026/06/01 新規作成
=====================================+
*/

using UnityEditor;
using UnityEngine;

/// <summary>
/// 選択中のGameObject配下にあるCS_で始まるMonoBehaviourを一覧表示するEditorWindow。
/// </summary>
public class CSED_EffectDebugWindow : EditorWindow
{
    private Vector2 scrollPosition;

    /// <summary>
    /// エフェクトデバッグウィンドウを開く。
    /// </summary>
    [MenuItem("Tools/Effect/Effect Debug Window")]
    private static void OpenWindow()
    {
        CSED_EffectDebugWindow window = GetWindow<CSED_EffectDebugWindow>("Effect Debug");
        window.minSize = new Vector2(420f, 300f);
        window.Show();
    }

    /// <summary>
    /// GUIを描画する。
    /// </summary>
    private void OnGUI()
    {
        GameObject selectedObject = Selection.activeGameObject;

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Effect Debug", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("選択中", selectedObject, typeof(GameObject), true);
        }

        EditorGUILayout.Space(8f);

        if (selectedObject == null)
        {
            EditorGUILayout.HelpBox("Hierarchyでエフェクトの親Objectを選択してください。", MessageType.Info);
            return;
        }

        DrawCsScriptList(selectedObject);
    }

    /// <summary>
    /// 選択中Object配下のCS_スクリプト一覧を描画する。
    /// </summary>
    /// <param name="rootObject">検索対象の親Object。</param>
    private void DrawCsScriptList(GameObject rootObject)
    {
        MonoBehaviour[] behaviours = rootObject.GetComponentsInChildren<MonoBehaviour>(true);

        int csScriptCount = CountCsScripts(behaviours);

        EditorGUILayout.LabelField($"CS_スクリプト一覧 : {csScriptCount} 個", EditorStyles.boldLabel);

        EditorGUILayout.Space(4f);

        if (csScriptCount <= 0)
        {
            EditorGUILayout.HelpBox("選択中Object配下に CS_ で始まるスクリプトはありません。", MessageType.Warning);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0 ; i < behaviours.Length ; i++)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour == null)
            {
                continue;
            }

            string scriptName = behaviour.GetType().Name;

            if (!scriptName.StartsWith("CS_"))
            {
                continue;
            }

            DrawScriptItem(rootObject.transform, behaviour);
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// CS_スクリプトの数を数える。
    /// </summary>
    /// <param name="behaviours">確認対象のMonoBehaviour配列。</param>
    /// <returns>CS_で始まるスクリプト数。</returns>
    private int CountCsScripts(MonoBehaviour[] behaviours)
    {
        int count = 0;

        for (int i = 0 ; i < behaviours.Length ; i++)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour == null)
            {
                continue;
            }

            if (behaviour.GetType().Name.StartsWith("CS_"))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 1つ分のスクリプト情報を描画する。
    /// </summary>
    /// <param name="rootTransform">選択中Root。</param>
    /// <param name="behaviour">表示対象のMonoBehaviour。</param>
    private void DrawScriptItem(Transform rootTransform, MonoBehaviour behaviour)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        string scriptName = behaviour.GetType().Name;
        string objectPath = GetRelativePath(rootTransform, behaviour.transform);

        EditorGUILayout.LabelField(scriptName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Object", objectPath);

        EditorGUILayout.BeginHorizontal();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.Toggle("Enabled", behaviour.enabled);
        }

        if (GUILayout.Button("選択", GUILayout.Width(70f)))
        {
            Selection.activeGameObject = behaviour.gameObject;
            EditorGUIUtility.PingObject(behaviour.gameObject);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Rootから見た相対パスを取得する。
    /// </summary>
    /// <param name="rootTransform">基準Root。</param>
    /// <param name="targetTransform">対象Transform。</param>
    /// <returns>相対パス。</returns>
    private string GetRelativePath(Transform rootTransform, Transform targetTransform)
    {
        if (rootTransform == targetTransform)
        {
            return rootTransform.name;
        }

        string path = targetTransform.name;
        Transform currentTransform = targetTransform.parent;

        while (currentTransform != null && currentTransform != rootTransform)
        {
            path = currentTransform.name + "/" + path;
            currentTransform = currentTransform.parent;
        }

        return rootTransform.name + "/" + path;
    }
}
