/*
+=====================================
 ファイル名 : CSED_EffectDebugWindow.cs
 概要     : 選択中のエフェクト配下にあるCS_スクリプトを一覧表示して再生・停止操作を行うデバッグウィンドウ
 作者     : ヨシモト リョウ
 履歴     : 2026/06/01 新規作成
            2026/06/03 CS_スクリプトごとの操作プルダウンを追加
=====================================+
*/

using System;
using System.Reflection;
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

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.Toggle("Enabled", behaviour.enabled);
        }

        // Enabled表示の下に操作用プルダウンを出します。
        // ここから、そのCS_スクリプトの再生・停止を実行します。
        if (GUILayout.Button("操作 ▼", GUILayout.Width(90f)))
        {
            ShowScriptActionMenu(behaviour);
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// スクリプト操作用のプルダウンメニューを表示する。
    /// </summary>
    /// <param name="behaviour">操作対象のMonoBehaviour。</param>
    private void ShowScriptActionMenu(MonoBehaviour behaviour)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(
            new GUIContent("再生"),
            false,
            () => InvokePlayAction(behaviour));

        menu.AddItem(
            new GUIContent("停止"),
            false,
            () => InvokeStopAction(behaviour));

        menu.ShowAsContext();
    }

    /// <summary>
    /// 対象スクリプトの再生処理を呼び出す。
    /// </summary>
    /// <param name="behaviour">操作対象のMonoBehaviour。</param>
    private void InvokePlayAction(MonoBehaviour behaviour)
    {
        if (behaviour == null)
        {
            return;
        }

        // Coroutineは非アクティブなGameObjectでは開始できないため、
        // 再生前に対象GameObjectを有効化します。
        if (!behaviour.gameObject.activeSelf)
        {
            Undo.RecordObject(behaviour.gameObject, "Effect Debug Activate Object");
            behaviour.gameObject.SetActive(true);
            EditorUtility.SetDirty(behaviour.gameObject);
        }

        // 親が非アクティブだと、自分をSetActive(true)してもactiveInHierarchyはtrueになりません。
        // その場合はCoroutineを開始できないため、警告を出して止めます。
        if (!behaviour.gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                "[EffectDebug] 親Objectが非アクティブのため再生できません : " +
                behaviour.gameObject.name);

            return;
        }

        // 停止でenabledをfalseにしている可能性があるので、再生時は有効に戻します。
        Undo.RecordObject(behaviour, "Effect Debug Play");
        behaviour.enabled = true;

        MethodInfo playMethod = FindPlayMethod(behaviour.GetType());

        if (playMethod == null)
        {
            EditorUtility.SetDirty(behaviour);
            Debug.LogWarning("[EffectDebug] 再生用メソッドが見つかりません : " + behaviour.GetType().Name);
            return;
        }

        InvokeMethodSafe(behaviour, playMethod);
        EditorUtility.SetDirty(behaviour);
    }

    /// <summary>
    /// 対象スクリプトの停止処理を呼び出す。
    /// </summary>
    /// <param name="behaviour">操作対象のMonoBehaviour。</param>
    private void InvokeStopAction(MonoBehaviour behaviour)
    {
        if (behaviour == null)
        {
            return;
        }

        MethodInfo stopMethod = FindStopMethod(behaviour.GetType());

        Undo.RecordObject(behaviour, "Effect Debug Stop");

        if (stopMethod != null)
        {
            InvokeMethodSafe(behaviour, stopMethod);
            EditorUtility.SetDirty(behaviour);
            return;
        }

        // Stop系メソッドが無い場合の最低限停止です。
        // 再生中のCoroutineを止めて、Componentを無効化します。
        behaviour.StopAllCoroutines();
        behaviour.enabled = false;

        EditorUtility.SetDirty(behaviour);

        Debug.Log("[EffectDebug] Stop系メソッドが無いため、Coroutine停止 + enabled=false にしました : " + behaviour.GetType().Name);
    }

    /// <summary>
    /// 再生用メソッドを探す。
    /// </summary>
    /// <param name="targetType">確認対象の型。</param>
    /// <returns>見つかった再生用メソッド。</returns>
    private MethodInfo FindPlayMethod(Type targetType)
    {
        string[] playMethodNames =
        {
            "Play",
            "PlayEffect",
            "PlayReveal",
            "PlayHide",
            "Replay",
            "Restart",
            "StartEffect"
        };

        return FindActionMethod(targetType, playMethodNames, true);
    }

    /// <summary>
    /// 停止用メソッドを探す。
    /// </summary>
    /// <param name="targetType">確認対象の型。</param>
    /// <returns>見つかった停止用メソッド。</returns>
    private MethodInfo FindStopMethod(Type targetType)
    {
        string[] stopMethodNames =
        {
            "Stop",
            "StopEffect",
            "StopReveal",
            "StopHide",
            "StopPlay",
            "Pause",
            "SetHidden",
            "Hide"
        };

        return FindActionMethod(targetType, stopMethodNames, false);
    }

    /// <summary>
    /// 指定された候補名から実行可能なメソッドを探す。
    /// </summary>
    /// <param name="targetType">確認対象の型。</param>
    /// <param name="methodNames">優先して探すメソッド名一覧。</param>
    /// <param name="isPlayAction">再生処理として探す場合はtrue。</param>
    /// <returns>見つかったメソッド。</returns>
    private MethodInfo FindActionMethod(Type targetType, string[] methodNames, bool isPlayAction)
    {
        BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        for (int i = 0 ; i < methodNames.Length ; i++)
        {
            MethodInfo methodInfo = targetType.GetMethod(methodNames[i], flags);

            if (IsCallableEffectMethod(methodInfo))
            {
                return methodInfo;
            }
        }

        MethodInfo[] methods =
            targetType.GetMethods(flags | BindingFlags.DeclaredOnly);

        for (int i = 0 ; i < methods.Length ; i++)
        {
            MethodInfo methodInfo = methods[i];

            if (!IsCallableEffectMethod(methodInfo))
            {
                continue;
            }

            string lowerName = methodInfo.Name.ToLowerInvariant();

            if (isPlayAction)
            {
                if (lowerName.StartsWith("play") ||
                    lowerName.StartsWith("replay") ||
                    lowerName.StartsWith("restart"))
                {
                    return methodInfo;
                }
            }
            else
            {
                if (lowerName.StartsWith("stop") ||
                    lowerName.StartsWith("pause") ||
                    lowerName.StartsWith("hide") ||
                    lowerName == "sethidden")
                {
                    return methodInfo;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// EffectDebugから呼び出してよいメソッドか確認する。
    /// </summary>
    /// <param name="methodInfo">確認対象のメソッド。</param>
    /// <returns>呼び出し可能な場合はtrue。</returns>
    private bool IsCallableEffectMethod(MethodInfo methodInfo)
    {
        if (methodInfo == null)
        {
            return false;
        }

        if (methodInfo.IsSpecialName)
        {
            return false;
        }

        if (methodInfo.ReturnType != typeof(void))
        {
            return false;
        }

        if (methodInfo.GetParameters().Length > 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Reflectionで取得したメソッドを安全に実行する。
    /// </summary>
    /// <param name="behaviour">実行対象のMonoBehaviour。</param>
    /// <param name="methodInfo">実行するメソッド。</param>
    private void InvokeMethodSafe(MonoBehaviour behaviour, MethodInfo methodInfo)
    {
        try
        {
            methodInfo.Invoke(behaviour, null);

            Debug.Log(
                "[EffectDebug] 実行しました : " +
                behaviour.GetType().Name +
                "." +
                methodInfo.Name);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[EffectDebug] メソッド実行中にエラーが発生しました : " +
                behaviour.GetType().Name +
                "." +
                methodInfo.Name +
                "\n" +
                exception);
        }
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
