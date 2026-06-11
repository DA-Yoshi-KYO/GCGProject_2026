using System.Collections;
using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectTransformController.cs
 概要     : EffectのTransform操作を行うクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/11 新規作成
=====================================+
*/

/// <summary>
/// EffectのTransform操作を行うクラスです。
/// Effectの再生処理とは分離して、
/// 再生中のPosition / Rotation / Scale変更だけを担当します。
/// </summary>
[DisallowMultipleComponent]
public class CS_EffectTransformController : MonoBehaviour, CSI_EffectTransformControllable
{
    /// <summary>
    /// Position変更処理のCoroutineです。
    /// </summary>
    private Coroutine co_MoveCoroutine;

    /// <summary>
    /// Rotation変更処理のCoroutineです。
    /// </summary>
    private Coroutine co_RotateCoroutine;

    /// <summary>
    /// Scale変更処理のCoroutineです。
    /// </summary>
    private Coroutine co_ScaleCoroutine;

    /// <summary>
    /// EffectのPositionを即時変更します。
    /// </summary>
    /// <param name="v3_Position">変更後のPosition。</param>
    public void SetEffectPosition(Vector3 v3_Position)
    {
        StopEffectMove();

        transform.position = v3_Position;
    }

    /// <summary>
    /// EffectのRotationを即時変更します。
    /// </summary>
    /// <param name="q_Rotation">変更後のRotation。</param>
    public void SetEffectRotation(Quaternion q_Rotation)
    {
        StopEffectRotate();

        transform.rotation = q_Rotation;
    }

    /// <summary>
    /// EffectのScaleを即時変更します。
    /// </summary>
    /// <param name="v3_Scale">変更後のScale。</param>
    public void SetEffectScale(Vector3 v3_Scale)
    {
        StopEffectScale();

        transform.localScale = v3_Scale;
    }

    /// <summary>
    /// EffectのPositionを時間をかけて変更します。
    /// </summary>
    /// <param name="v3_TargetPosition">目標Position。</param>
    /// <param name="f_MoveTime">移動時間。</param>
    public void MoveEffect(Vector3 v3_TargetPosition, float f_MoveTime)
    {
        StopEffectMove();

        if (f_MoveTime <= 0.0f)
        {
            transform.position = v3_TargetPosition;
            return;
        }

        co_MoveCoroutine = StartCoroutine(
            MoveEffectCoroutine(
                transform.position,
                v3_TargetPosition,
                f_MoveTime));
    }

    /// <summary>
    /// EffectのRotationを時間をかけて変更します。
    /// </summary>
    /// <param name="q_TargetRotation">目標Rotation。</param>
    /// <param name="f_RotateTime">回転時間。</param>
    public void RotateEffect(Quaternion q_TargetRotation, float f_RotateTime)
    {
        StopEffectRotate();

        if (f_RotateTime <= 0.0f)
        {
            transform.rotation = q_TargetRotation;
            return;
        }

        co_RotateCoroutine = StartCoroutine(
            RotateEffectCoroutine(
                transform.rotation,
                q_TargetRotation,
                f_RotateTime));
    }

    /// <summary>
    /// EffectのScaleを時間をかけて変更します。
    /// </summary>
    /// <param name="v3_TargetScale">目標Scale。</param>
    /// <param name="f_ScaleTime">変更時間。</param>
    public void ScaleEffect(Vector3 v3_TargetScale, float f_ScaleTime)
    {
        StopEffectScale();

        if (f_ScaleTime <= 0.0f)
        {
            transform.localScale = v3_TargetScale;
            return;
        }

        co_ScaleCoroutine = StartCoroutine(
            ScaleEffectCoroutine(
                transform.localScale,
                v3_TargetScale,
                f_ScaleTime));
    }

    /// <summary>
    /// Position変更処理を停止します。
    /// </summary>
    public void StopEffectMove()
    {
        if (co_MoveCoroutine == null)
        {
            return;
        }

        StopCoroutine(co_MoveCoroutine);
        co_MoveCoroutine = null;
    }

    /// <summary>
    /// Rotation変更処理を停止します。
    /// </summary>
    public void StopEffectRotate()
    {
        if (co_RotateCoroutine == null)
        {
            return;
        }

        StopCoroutine(co_RotateCoroutine);
        co_RotateCoroutine = null;
    }

    /// <summary>
    /// Scale変更処理を停止します。
    /// </summary>
    public void StopEffectScale()
    {
        if (co_ScaleCoroutine == null)
        {
            return;
        }

        StopCoroutine(co_ScaleCoroutine);
        co_ScaleCoroutine = null;
    }

    /// <summary>
    /// すべてのTransform変更処理を停止します。
    /// </summary>
    public void StopTransformControl()
    {
        StopEffectMove();
        StopEffectRotate();
        StopEffectScale();
    }

    /// <summary>
    /// 非アクティブになったとき、残っているTransform変更処理を止めます。
    /// </summary>
    private void OnDisable()
    {
        StopTransformControl();
    }

    /// <summary>
    /// Positionを補間して変更します。
    /// </summary>
    private IEnumerator MoveEffectCoroutine(
        Vector3 v3_StartPosition,
        Vector3 v3_TargetPosition,
        float f_MoveTime)
    {
        float f_CurrentTime = 0.0f;

        while (f_CurrentTime < f_MoveTime)
        {
            f_CurrentTime += Time.deltaTime;

            float f_Rate = Mathf.Clamp01(f_CurrentTime / f_MoveTime);

            transform.position = Vector3.Lerp(
                v3_StartPosition,
                v3_TargetPosition,
                f_Rate);

            yield return null;
        }

        transform.position = v3_TargetPosition;
        co_MoveCoroutine = null;
    }

    /// <summary>
    /// Rotationを補間して変更します。
    /// </summary>
    private IEnumerator RotateEffectCoroutine(
        Quaternion q_StartRotation,
        Quaternion q_TargetRotation,
        float f_RotateTime)
    {
        float f_CurrentTime = 0.0f;

        while (f_CurrentTime < f_RotateTime)
        {
            f_CurrentTime += Time.deltaTime;

            float f_Rate = Mathf.Clamp01(f_CurrentTime / f_RotateTime);

            transform.rotation = Quaternion.Slerp(
                q_StartRotation,
                q_TargetRotation,
                f_Rate);

            yield return null;
        }

        transform.rotation = q_TargetRotation;
        co_RotateCoroutine = null;
    }

    /// <summary>
    /// Scaleを補間して変更します。
    /// </summary>
    private IEnumerator ScaleEffectCoroutine(
        Vector3 v3_StartScale,
        Vector3 v3_TargetScale,
        float f_ScaleTime)
    {
        float f_CurrentTime = 0.0f;

        while (f_CurrentTime < f_ScaleTime)
        {
            f_CurrentTime += Time.deltaTime;

            float f_Rate = Mathf.Clamp01(f_CurrentTime / f_ScaleTime);

            transform.localScale = Vector3.Lerp(
                v3_StartScale,
                v3_TargetScale,
                f_Rate);

            yield return null;
        }

        transform.localScale = v3_TargetScale;
        co_ScaleCoroutine = null;
    }
}
