using UnityEngine;

/*
+=====================================
 ファイル名 : CS_EffectFactory.cs
 概要     : Effectを生成するFactoryクラス
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

/// <summary>
/// Effectを生成するFactoryクラスです。
/// </summary>
public static class CS_EffectFactory
{
    /// <summary>
    /// Effectを生成します。
    /// </summary>
    /// <param name="go_EffectPrefab">生成するEffectPrefab。</param>
    /// <param name="v3_Position">生成位置。</param>
    /// <param name="q_Rotation">生成回転。</param>
    /// <param name="tr_Parent">親Transform。</param>
    /// <returns>生成したEffect共通処理クラス。</returns>
    public static CSAD_EffectCommonProcessBase CreateEffect(
        GameObject go_EffectPrefab,
        Vector3 v3_Position,
        Quaternion q_Rotation,
        Transform tr_Parent)
    {
        if (go_EffectPrefab == null)
        {
            Debug.LogWarning("[CS_EffectFactory] EffectPrefabがnullです。");
            return null;
        }

        GameObject go_Effect = Object.Instantiate(
            go_EffectPrefab,
            v3_Position,
            q_Rotation,
            tr_Parent);

        CSAD_EffectCommonProcessBase csad_EffectProcess =
            go_Effect.GetComponent<CSAD_EffectCommonProcessBase>();

        if (csad_EffectProcess == null)
        {
            Debug.LogWarning("[CS_EffectFactory] EffectPrefabにCSAD_EffectCommonProcessBase継承クラスがありません : " + go_Effect.name);
            Object.Destroy(go_Effect);
            return null;
        }

        return csad_EffectProcess;
    }
}
