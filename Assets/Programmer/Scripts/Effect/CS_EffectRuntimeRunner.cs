/*
+=====================================
 ファイル名 : CS_EffectRuntimeRunner.cs
 概要     : Effect管理用の遅延・監視処理を実行する常駐Runner
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Effect管理用のCoroutineを実行する常駐Runnerです。
/// </summary>
public class CS_EffectRuntimeRunner : MonoBehaviour
{
    private static CS_EffectRuntimeRunner instance;

    /// <summary>
    /// Runnerインスタンスを取得します。
    /// </summary>
    private static CS_EffectRuntimeRunner Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject runnerObject = new GameObject("__EffectRuntimeRunner");
            UnityEngine.Object.DontDestroyOnLoad(runnerObject);

            instance = runnerObject.AddComponent<CS_EffectRuntimeRunner>();
            return instance;
        }
    }

    /// <summary>
    /// 対象Objectが非アクティブになったら処理を実行します。
    /// </summary>
    /// <param name="targetObject">監視対象Object。</param>
    /// <param name="action">非アクティブ後に実行する処理。</param>
    public static void RunWhenInactive(GameObject targetObject, Action action)
    {
        Instance.StartCoroutine(Instance.WaitUntilInactiveCoroutine(targetObject, action));
    }

    /// <summary>
    /// 対象Objectが非アクティブになるまで待機します。
    /// </summary>
    /// <param name="targetObject">監視対象Object。</param>
    /// <param name="action">非アクティブ後に実行する処理。</param>
    private IEnumerator WaitUntilInactiveCoroutine(GameObject targetObject, Action action)
    {
        while (targetObject != null && targetObject.activeInHierarchy)
        {
            yield return null;
        }

        if (targetObject == null)
        {
            yield break;
        }

        action?.Invoke();
    }
}
