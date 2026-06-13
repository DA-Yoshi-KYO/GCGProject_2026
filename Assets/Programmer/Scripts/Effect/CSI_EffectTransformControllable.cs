/*
+=====================================
 ファイル名 : CSI_EffectTransformControllable.cs
 概要     : EffectのTransform操作を定義するInterface
 作者     : ヨシモト リョウ
 履歴     : 2026/06/11 新規作成
=====================================+
*/

using UnityEngine;

/// <summary>
/// EffectのTransform操作を定義するInterfaceです。
/// 再生中のEffectに対して、Position / Rotation / Scaleを変更します。
/// </summary>
public interface CSI_EffectTransformControllable
{
    void SetEffectPosition(Vector3 v3_Position);

    void SetEffectRotation(Quaternion q_Rotation);

    void SetEffectScale(Vector3 v3_Scale);

    void MoveEffect(Vector3 v3_TargetPosition, float f_MoveTime);

    void RotateEffect(Quaternion q_TargetRotation, float f_RotateTime);

    void ScaleEffect(Vector3 v3_TargetScale, float f_ScaleTime);

    void StopEffectMove();

    void StopEffectRotate();

    void StopEffectScale();

    void StopTransformControl();
}
