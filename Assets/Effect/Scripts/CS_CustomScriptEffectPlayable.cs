/*
+=====================================
 ファイル名 : CS_CustomScriptEffectPlayable.cs
 概要     : 自作CS制御エフェクトを汎用的に再生・停止するPlayable
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 自作CSやShader制御で作られたエフェクトを、汎用的に再生・停止するクラスです。
/// </summary>
[DisallowMultipleComponent]
public class CS_CustomScriptEffectPlayable : MonoBehaviour, CSI_EffectPlayable
{
    [Header("非アクティブの子も検索するか")]
    [SerializeField]
    private bool bool_IsIncludeInactiveChildren = true;

    [Header("属性が無い場合にメソッド名で自動判定するか")]
    [SerializeField]
    private bool bool_IsUseMethodNameFallback = true;

    [Header("再生時にRootをActiveにするか")]
    [SerializeField]
    private bool bool_IsActivateOnPlay = true;

    /// <summary>
    /// エフェクトを再生します。
    /// </summary>
    public void PlayEffect()
    {
        if (bool_IsActivateOnPlay)
        {
            gameObject.SetActive(true);
        }

        InvokeEffectMethods(true);
    }

    /// <summary>
    /// エフェクトを停止します。
    /// </summary>
    public void StopEffect()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        InvokeEffectMethods(false);
    }

    /// <summary>
    /// エフェクトの再生速度を設定します。
    /// </summary>
    /// <param name="playSpeed">再生速度。</param>
    public void SetPlaySpeed(float playSpeed)
    {
        InvokeSetPlaySpeedMethods(playSpeed);
    }

    /// <summary>
    /// 再生または停止用メソッドを実行します。
    /// </summary>
    /// <param name="bool_IsPlay">再生処理ならtrue、停止処理ならfalse。</param>
    private void InvokeEffectMethods(bool bool_IsPlay)
    {
        MonoBehaviour[] behaviours =
            GetComponentsInChildren<MonoBehaviour>(bool_IsIncludeInactiveChildren);

        for (int i = 0 ; i < behaviours.Length ; i++)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour == null)
            {
                continue;
            }

            if (behaviour == this)
            {
                continue;
            }

            InvokeEffectMethodsFromBehaviour(behaviour, bool_IsPlay);
        }
    }

    /// <summary>
    /// 指定されたMonoBehaviourから再生または停止用メソッドを探して実行します。
    /// </summary>
    /// <param name="behaviour">確認対象のMonoBehaviour。</param>
    /// <param name="bool_IsPlay">再生処理ならtrue、停止処理ならfalse。</param>
    private void InvokeEffectMethodsFromBehaviour(MonoBehaviour behaviour, bool bool_IsPlay)
    {
        BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        MethodInfo[] methods = behaviour.GetType().GetMethods(flags);

        for (int i = 0 ; i < methods.Length ; i++)
        {
            MethodInfo methodInfo = methods[i];

            if (!IsCallableVoidMethod(methodInfo))
            {
                continue;
            }

            if (!IsTargetEffectMethod(methodInfo, bool_IsPlay))
            {
                continue;
            }

            InvokeMethodSafe(behaviour, methodInfo);
        }
    }

    /// <summary>
    /// 再生速度設定用メソッドを探して実行します。
    /// </summary>
    /// <param name="playSpeed">再生速度。</param>
    private void InvokeSetPlaySpeedMethods(float playSpeed)
    {
        MonoBehaviour[] behaviours =
            GetComponentsInChildren<MonoBehaviour>(bool_IsIncludeInactiveChildren);

        for (int i = 0 ; i < behaviours.Length ; i++)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour == null || behaviour == this)
            {
                continue;
            }

            MethodInfo methodInfo = behaviour.GetType().GetMethod(
                "SetPlaySpeed",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (methodInfo == null)
            {
                continue;
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(float))
            {
                continue;
            }

            methodInfo.Invoke(behaviour, new object[] { playSpeed });
        }
    }

    /// <summary>
    /// 対象メソッドが再生または停止用メソッドか判定します。
    /// </summary>
    /// <param name="methodInfo">確認対象のメソッド。</param>
    /// <param name="bool_IsPlay">再生処理ならtrue、停止処理ならfalse。</param>
    /// <returns>対象メソッドならtrue。</returns>
    private bool IsTargetEffectMethod(MethodInfo methodInfo, bool bool_IsPlay)
    {
        if (bool_IsPlay && Attribute.IsDefined(methodInfo, typeof(CS_EffectPlayAttribute)))
        {
            return true;
        }

        if (!bool_IsPlay && Attribute.IsDefined(methodInfo, typeof(CS_EffectStopAttribute)))
        {
            return true;
        }

        if (!bool_IsUseMethodNameFallback)
        {
            return false;
        }

        if (bool_IsPlay)
        {
            return
                methodInfo.Name == "PlayEffect" ||
                methodInfo.Name == "Play" ||
                methodInfo.Name == "PlayReveal" ||
                methodInfo.Name == "Replay" ||
                methodInfo.Name == "StartEffect";
        }

        return
            methodInfo.Name == "StopEffect" ||
            methodInfo.Name == "Stop" ||
            methodInfo.Name == "PlayHide" ||
            methodInfo.Name == "SetHidden" ||
            methodInfo.Name == "Hide";
    }

    /// <summary>
    /// 引数なしvoidメソッドか確認します。
    /// </summary>
    /// <param name="methodInfo">確認対象のメソッド。</param>
    /// <returns>呼び出し可能ならtrue。</returns>
    private bool IsCallableVoidMethod(MethodInfo methodInfo)
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
    /// メソッドを安全に実行します。
    /// </summary>
    /// <param name="behaviour">実行対象。</param>
    /// <param name="methodInfo">実行するメソッド。</param>
    private void InvokeMethodSafe(MonoBehaviour behaviour, MethodInfo methodInfo)
    {
        try
        {
            methodInfo.Invoke(behaviour, null);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[CustomScriptEffectPlayable] メソッド実行に失敗しました : " +
                behaviour.GetType().Name +
                "." +
                methodInfo.Name +
                "\n" +
                exception);
        }
    }
}
