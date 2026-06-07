/*
+=====================================
 ファイル名 : CS_EffectScaleUtility.cs
 概要     : EffectのPrefab初期Scaleを基準にしたScale計算を行う
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using UnityEngine;

/// <summary>
/// EffectのScale計算を行うUtilityクラスです。
/// </summary>
public static class CS_EffectScaleUtility
{
    /// <summary>
    /// Prefab初期Scaleを基準に、外部指定Scaleから最終Scaleを計算します。
    /// 1.0f の軸はPrefab初期値を維持し、それ以外はPrefab初期値に加算します。
    /// </summary>
    /// <param name="defaultLocalScale">Prefab初期Scale。</param>
    /// <param name="scaleValue">外部から指定されたScale値。</param>
    /// <returns>最終Scale。</returns>
    public static Vector3 CalculateScale(Vector3 defaultLocalScale, Vector3 scaleValue)
    {
        return new Vector3(
            CalculateAxisScale(defaultLocalScale.x, scaleValue.x),
            CalculateAxisScale(defaultLocalScale.y, scaleValue.y),
            CalculateAxisScale(defaultLocalScale.z, scaleValue.z));
    }

    /// <summary>
    /// 1軸分のScaleを計算します。
    /// </summary>
    /// <param name="defaultAxisScale">Prefab初期Scaleの1軸分。</param>
    /// <param name="axisScaleValue">外部から指定された1軸分のScale値。</param>
    /// <returns>最終Scaleの1軸分。</returns>
    private static float CalculateAxisScale(float defaultAxisScale, float axisScaleValue)
    {
        if (Mathf.Approximately(axisScaleValue, 1.0f))
        {
            return defaultAxisScale;
        }

        return defaultAxisScale + axisScaleValue;
    }
}
